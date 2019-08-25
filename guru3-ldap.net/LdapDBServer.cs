using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using eventphone.guru3.ldap.DAL;
using Microsoft.EntityFrameworkCore;
using zivillian.ldap;
using zivillian.ldap.Attributes;
using zivillian.ldap.ObjectClasses;

namespace eventphone.guru3.ldap
{
    public class LdapDBServer:LdapServer
    {
        private static readonly LdapDistinguishedName RootDN = new LdapDistinguishedName("dc=eventphone,dc=de");

        private readonly string _connectionString;
        protected readonly ConcurrentDictionary<Guid, int> Sessions = new ConcurrentDictionary<Guid, int>();
        private readonly ConcurrentDictionary<Guid, bool> _admins = new ConcurrentDictionary<Guid, bool>();
        
        public LdapDBServer(ushort port, string connectionString)
            : base(port, GetRootDSE())
        {
            _connectionString = connectionString;
        }

        public LdapDBServer(IPEndPoint endPoint, string connectionString)
            : base(endPoint, GetRootDSE())
        {
            _connectionString = connectionString;
        }
        
        public string AdminToken { get; set; }

        private static RootDSE _rootDse;
        private static RootDSE GetRootDSE()
        {
            if (_rootDse != null)
                return _rootDse;
            var domain = String.Join('.', RootDN.RDNs.Select(x => x.Values).Select(x => x.First()));
            return _rootDse = new RootDSE
            {
                O = {Entries = {domain}},
                Dc = {Entries = {RootDN.RDNs[0].Values[0].Value}},
                NamingContexts = new NamingContextsAttribute {Entries = {RootDN}},
                SupportedSASLMechanisms = new SupportedSASLMechanismsAttribute {Entries = {"PLAIN", "ANOMYMOUS"}},
                SupportedFeatures = new SupportedFeaturesAttribute {Entries = {"1.3.6.1.4.1.4203.1.5.1"}}
            };
        }

        protected override Task<ResultCode> OnBindAsync(LdapDistinguishedName bindDN, ReadOnlyMemory<byte> password, LdapClientConnection connection)
        {
            var username = bindDN.RDNs.SelectMany(x=>x.Values).Select(x=>x.Value).FirstOrDefault();
            return OnSaslBindAsync(bindDN, username, password, connection);
        }

        protected override async Task<ResultCode> OnSaslBindAsync(LdapDistinguishedName bindDN, string username, ReadOnlyMemory<byte> password, LdapClientConnection connection)
        {
            if (String.IsNullOrEmpty(username))
            {
                Console.WriteLine($"bind to {bindDN} (anon) [{connection.Id}]");
                return ResultCode.Success;
            }

            using (var context = GetContext())
            {
                Console.WriteLine($"bind to {bindDN} ({username}) [{connection.Id}]");
                var eventId = await context.Events.Where(x => x.Name == username).Select(x=>x.Id).FirstOrDefaultAsync(connection.CancellationToken);
                if (eventId != default)
                {
                    Sessions.AddOrUpdate(connection.Id, eventId, (x, y) => eventId);
                    if (!String.IsNullOrEmpty(AdminToken))
                    {
                        var pass = Encoding.UTF8.GetString(password.Span);
                        if (pass == AdminToken)
                            _admins.AddOrUpdate(connection.Id, true, (x, y) => true);
                    }
                    return ResultCode.Success;
                }
                else
                {
                    return ResultCode.InvalidCredentials;
                }
            }
        }

        protected override void OnClientDisconnected(Guid connectionId)
        {
            Console.WriteLine($"closed [{connectionId}]");
            Sessions.TryRemove(connectionId, out _);
            _admins.TryRemove(connectionId, out _);
        }

        protected override async Task<IEnumerable<LdapRequestMessage>> OnSearchAsync(LdapSearchRequest request, LdapClientConnection connection, CancellationToken cancellationToken)
        {
            Console.WriteLine($"search for {request.Filter} in {request.BaseObject} ({request.Scope}) [{connection.Id}]");
            using (var context = GetContext())
            {
                IQueryable<Event> dbEvents = context.Events;
                if (Sessions.TryGetValue(connection.Id, out var eventId))
                {
                    dbEvents = dbEvents.Where(x => x.Id == eventId);
                }
                if (String.Equals(request.BaseObject.ToString(), RootDN.ToString(), StringComparison.OrdinalIgnoreCase))
                {
                    if (request.Scope == SearchScope.BaseObject)
                    {
                        //search root
                        var attributes = GetRootDSE().GetAttributes(request.Attributes, request.TypesOnly);
                        var result = request.Result(RootDN, attributes.ToArray(), new LdapControl[0]);
                        return new[] {result};
                    }
                    else
                    {
                        //search events
                        var events = await SearchEventAsync(dbEvents, request, connection, cancellationToken);
                        var results = events.ToList();

                        if (request.Scope == SearchScope.WholeSubtree)
                        {
                            //include extensions
                            var dbExtensions = SearchEvent(dbEvents, connection).Join(context.Extensions, x => x.Id, x => x.EventId, (x, y) => y);
                            var extension = await SearchExtensionAsync(dbExtensions, request, cancellationToken);
                            if (request.SizeLimit > 0)
                                extension = extension.Take(request.SizeLimit - results.Count);
                            results.AddRange(extension);
                        }
                        return results;
                    }
                }
                else
                {
                    var rdns = request.BaseObject.RDNs;
                    if (rdns.Count <= 2 || rdns.Count > 4)
                    {
                        return new LdapRequestMessage[0];
                    }

                    //search extensions
                    IQueryable<Extension> query = SearchEvent(dbEvents, connection).Join(context.Extensions, x => x.Id, x => x.EventId, (x, y) => y);

                    if (rdns.Count >= 3)
                    {
                        var eventName = rdns[rdns.Count-3].Values[0].Value;
                        if (rdns.Count == 3 && request.Scope == SearchScope.BaseObject)
                        {
                            //get event
                            var events = await SearchEventAsync(dbEvents.Where(x=>x.Name == eventName), request, connection, cancellationToken);
                            return events.ToList();
                        }
                        query = query.Where(x => x.Event.Name == eventName);
                    }

                    if (rdns.Count == 4)
                    {
                        if (request.Scope != SearchScope.BaseObject)
                            return new LdapRequestMessage[0];

                        var extension = rdns[0].Values[0].Value;
                        query = query.Where(x => x.Number == extension);
                    }

                    if (Sessions.TryGetValue(connection.Id, out eventId))
                    {
                        query = query.Where(x => x.EventId == eventId);
                    }
                    var result = await SearchExtensionAsync(query, request, cancellationToken);
                    return result;
                }
            }
        }

        protected virtual Guru3Context GetContext()
        {
            return new Guru3Context(_connectionString);
        }

        private async Task<IEnumerable<LdapRequestMessage>> SearchEventAsync(IQueryable<Event> query, LdapSearchRequest request, LdapClientConnection connection, CancellationToken cancellationToken)
        {
            var eventQuery = SearchEvent(query, request, connection);
            if (request.SizeLimit > 0)
                eventQuery = eventQuery.Take(request.SizeLimit);
            var result = await eventQuery.ToArrayAsync(cancellationToken);
            var events = result.Select(x => new OrganizationalUnitObjectClass
            {
                Ou = {Entries = {x.Name}},
                Description = String.IsNullOrEmpty(x.Description)
                    ? null
                    : new DescriptionAttribute {Entries = {x.Description}},
                Locality = String.IsNullOrEmpty(x.Location) ? null : new LocalityAttribute {Entries = {x.Location}}
            }).ToList();
            var results = new List<LdapRequestMessage>(events.Count);
            foreach (var item in events)
            {
                var attributes = item.GetAttributes(request.Attributes, request.TypesOnly);
                var dn = new LdapDistinguishedName(item.Ou.Name, item.Ou.Entries[0], RootDN);
                results.Add(request.Result(dn, attributes.ToArray(), new LdapControl[0]));
            }
            return results;

        }

        private async Task<IEnumerable<LdapRequestMessage>> SearchExtensionAsync(IQueryable<Extension> query, LdapSearchRequest request, CancellationToken cancellationToken)
        {
            var extensionQuery = SearchExtension(query, request);
            if (request.SizeLimit > 0)
                extensionQuery = extensionQuery.Take(request.SizeLimit);
            var result = await extensionQuery.ToArrayAsync(cancellationToken);
            var extensions = result.Select(x => new OrganizationalPersonObjectClass
                {
                    Parent = new LdapDistinguishedName("ou", x.Event, RootDN),
                    Cn = {Entries = {x.Number}},
                    Sn = {Entries = {String.IsNullOrEmpty(x.Name) ? x.Number : x.Name}},
                    Locality = String.IsNullOrEmpty(x.Location) ? null : new LocalityAttribute {Entries = {x.Location}},
                    OrganizationalUnitName = new OuAttribute {Entries = {x.Event}},
                    TelephoneNumber = new TelephoneNumberAttribute {Entries = {x.Number}},
                    CreatorsName = new CreatorsNameAttribute {Entries = {new LdapDistinguishedName("cn", "GURU3", RootDN)}},
                    ModifiersName = new ModifiersNameAttribute {Entries = {new LdapDistinguishedName("cn", "GURU3", RootDN)}},
                    ModifyTimestamp = new ModifyTimestampAttribute {Entries = {x.LastModified}},
                })
                .ToList();
            var results = new List<LdapRequestMessage>(extensions.Count);
            foreach (var item in extensions)
            {
                var attributes = item.GetAttributes(request.Attributes, request.TypesOnly);
                var dn = new LdapDistinguishedName(item.Cn.Name, item.Cn.Entries[0], item.Parent);
                results.Add(request.Result(dn, attributes.ToArray(), new LdapControl[0]));
            }
            return results;
        }

        private IQueryable<LdapEvent> SearchEvent(IQueryable<Event> events, LdapSearchRequest request, LdapClientConnection connection)
        {
            return SearchEvent(events, connection).Where(FilterEvent(request.Filter));
        }

        private IQueryable<LdapEvent> SearchEvent(IQueryable<Event> events, LdapClientConnection connection)
        {
            var now = DateTime.Now.Date;
            if (!_admins.ContainsKey(connection.Id))
            {
                events = events
                    .Where(x => x.RegistrationStart != null || x.IsPermanentAndPublic)
                    .Where(x => x.RegistrationStart <= now || x.IsPermanentAndPublic)
                    .Where(x => x.End != null || x.IsPermanentAndPublic)
                    .Where(x => x.End >= now || x.IsPermanentAndPublic);
            }
            var filter = events
                .Select(x => new LdapEvent
                {
                    Id = x.Id,
                    Name = x.Name,
                    Description = x.DescriptionDe,
                    Location = x.Location
                })
                .Where(x => x.Name != null);
            return filter;
        }

        private IQueryable<LdapExtension> SearchExtension(IQueryable<Extension> extensions, LdapSearchRequest request)
        {
            return extensions
                .Where(x=>x.InPhonebook)
                .Where(x=>x.Number != String.Empty)
                .Select(x => new LdapExtension
                {
                    Number = x.Number,
                    Name = x.Name,
                    Location = x.Location,
                    Event = x.Event.Name,
                    LastModified = x.LastChanged
                })
                .Where(FilterExtension(request.Filter));
        }

        private Expression<Func<LdapEvent, bool>> FilterEvent(LdapFilter filter)
        {
            var visitor = new LdapEventFilterVisitor();
            visitor.Visit(filter);
            return visitor.Filter;
        }

        private Expression<Func<LdapExtension, bool>> FilterExtension(LdapFilter filter)
        {
            var visitor = new LdapExtensionFilterVisitor();
            visitor.Visit(filter);
            return visitor.Filter;
        }
    }
}
