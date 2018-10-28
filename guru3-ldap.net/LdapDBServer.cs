using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using eventphone.guru3.ldap.DAL;
using Microsoft.EntityFrameworkCore;
using zivillian.ldap;
using zivillian.ldap.Attributes;
using zivillian.ldap.ObjectClasses;

namespace eventphone.guru3.ldap
{
    class LdapDBServer:LdapServer
    {
        private static readonly LdapDistinguishedName RootDN = new LdapDistinguishedName("dc=eventphone,dc=de");

        private readonly string _connectionString;
        private readonly ConcurrentDictionary<Guid, int> _sessions = new ConcurrentDictionary<Guid, int>();
        
        public LdapDBServer(ushort port, string connectionString)
            : base(port, GetRootDSE())
        {
            _connectionString = connectionString;
        }

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
                NamingContexts = new NamingContextsAttribute{Entries = {RootDN}},
            };
        }

        protected override async Task<LdapBindResponse> OnBindAsync(LdapBindRequest request, LdapClientConnection connection)
        {
            if (request.Simple == null)
                return request.Response(ResultCode.AuthMethodNotSupported, "only simple bind");

            var username = request.Name.RDNs.SelectMany(x=>x.Values).Select(x=>x.Value).FirstOrDefault();
            if (String.IsNullOrEmpty(username))
                return request.Response();

            using (var context = new Guru3Context(_connectionString))
            {
                var exists = await context.Events.Where(x => x.Name == username).Select(x=>x.Id).FirstOrDefaultAsync(connection.CancellationToken);
                if (exists != default)
                {
                    _sessions.AddOrUpdate(connection.Id, exists, (x, y) => exists);
                    return request.Response();
                }
                else
                {
                    return request.Response(ResultCode.InvalidCredentials, "specified Event does not exist");
                }
            }
        }

        protected override void OnClientDisconnected(Guid connectionId)
        {
            _sessions.TryRemove(connectionId, out _);
        }

        protected override async Task<IEnumerable<LdapRequestMessage>> OnSearchAsync(LdapSearchRequest request, LdapClientConnection connection, CancellationToken cancellationToken)
        {
            using (var context = new Guru3Context(_connectionString))
            {
                if (String.Equals(request.BaseObject.ToString(), RootDN.ToString(), StringComparison.OrdinalIgnoreCase))
                {
                    if (request.Scope == SearchScope.BaseObject)
                    {
                        //search root
                        var attributes = GetRootDSE().GetAttributes(request.Attributes, request.TypesOnly);
                        var result = request.Result(RootDN, attributes, new LdapControl[0]);
                        return new[] {result};
                    }
                    else
                    {
                        //search events
                        var events = await SearchEventAsync(context.Events, request, cancellationToken);
                        var results = events.ToList();

                        if (request.Scope == SearchScope.WholeSubtree)
                        {
                            //include extensions
                            var dbExtensions = SearchEvent(context.Events, request).Join(context.Extensions, x => x.Id, x => x.EventId, (x, y) => y);
                            var extension = await SearchExtensionAsync(dbExtensions, request, cancellationToken);
                            results.AddRange(extension);
                        }
                        return results;
                    }
                }
                else
                {
                    var rdns = request.BaseObject.RDNs;
                    if (rdns.Length <= 2 || rdns.Length > 4)
                    {
                        return new LdapRequestMessage[0];
                    }

                    //search extensions
                    IQueryable<Extension> query = context.Extensions;

                    if (rdns.Length >= 3)
                    {
                        var eventName = rdns[rdns.Length-3].Values[0].Value;
                        if (rdns.Length == 3 && request.Scope == SearchScope.BaseObject)
                        {
                            //get event
                            var events = await SearchEventAsync(context.Events.Where(x=>x.Name == eventName), request, cancellationToken);
                            return events.ToList();
                        }
                        query = query.Where(x => x.Event.Name == eventName);
                    }

                    if (rdns.Length == 4)
                    {
                        var extension = rdns[0].Values[0].Value;
                        query = query.Where(x => x.Number == extension);
                    }

                    if (_sessions.TryGetValue(connection.Id, out var eventId))
                    {
                        query = query.Where(x => x.EventId == eventId);
                    }
                    var result = await SearchExtensionAsync(query, request, cancellationToken);
                    return result;
                }
            }
        }

        private async Task<IEnumerable<LdapRequestMessage>> SearchEventAsync(IQueryable<Event> query, LdapSearchRequest request, CancellationToken cancellationToken)
        {
            var eventQuery = SearchEvent(query, request);
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
                results.Add(request.Result(dn, attributes, new LdapControl[0]));
            }
            return results;

        }
        private async Task<IEnumerable<LdapRequestMessage>> SearchExtensionAsync(IQueryable<Extension> query, LdapSearchRequest request, CancellationToken cancellationToken)
        {
            var extensionQuery = SearchExtension(query, request);
            var result = await extensionQuery.ToArrayAsync(cancellationToken);
            var extensions = result.Select(x => new OrganizationalPersonObjectClass
                {
                    Parent = new LdapDistinguishedName("ou", x.Event, RootDN),
                    Cn = {Entries = {x.Number}},
                    Sn = { Entries = { String.IsNullOrEmpty(x.Name)?x.Number:x.Name}},
                    Locality = String.IsNullOrEmpty(x.Location) ? null : new LocalityAttribute {Entries = {x.Location}},
                    TelephoneNumber = new TelephoneNumberAttribute{Entries = { x.Number}}
                })
                .ToList();
            var results = new List<LdapRequestMessage>(extensions.Count);
            foreach (var item in extensions)
            {
                var attributes = item.GetAttributes(request.Attributes, request.TypesOnly);
                var dn = new LdapDistinguishedName(item.Cn.Name, item.Cn.Entries[0], item.Parent);
                results.Add(request.Result(dn, attributes, new LdapControl[0]));
            }
            return results;
        }

        private IQueryable<LdapEvent> SearchEvent(IQueryable<Event> events, LdapSearchRequest request)
        {
            return events
                .Select(x => new LdapEvent
                {
                    Id = x.Id,
                    Name = x.Name,
                    Description = x.DescriptionDe,
                    Location = x.Location
                })
                .Where(x=>x.Name != null)
                .Where(FilterEvent(request.Filter));

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
                    Event = x.Event.Name
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
