using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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
                        var events = await SearchEvent(context, request, cancellationToken);
                        var results = new List<LdapRequestMessage>(events.Length);
                        foreach (var item in events)
                        {
                            var attributes = item.GetAttributes(request.Attributes, request.TypesOnly);
                            var rdn = new LdapAttributeTypeAndValue(item.Ou.Name, item.Ou.Entries[0], false);
                            var dn = new LdapDistinguishedName(new LdapRelativeDistinguishedName(rdn), RootDN);
                            results.Add(request.Result(dn, attributes, new LdapControl[0]));
                        }

                        if (request.Scope == SearchScope.WholeSubtree)
                        {
                            //include extensions
                            throw new NotImplementedException();
                        }

                        return results;
                    }
                }
                else
                {
                    //search extensions
                    throw new NotImplementedException();
                }
            }
        }

        private async Task<OrganizationalUnitObjectClass[]> SearchEvent(Guru3Context context, LdapSearchRequest request, CancellationToken cancellationToken)
        {
            var events = context.Events
                .Select(x => new LdapEvent
                {
                    Name = x.Name,
                    Description = x.DescriptionDe,
                    Location = x.Location
                })
                .Where(x=>x.Name != null)
                .Where(FilterEvent(request.Filter));

            var result = await events.ToArrayAsync(cancellationToken);
            return result.Select(x => new OrganizationalUnitObjectClass
            {
                Ou = {Entries = {x.Name}},
                Description = String.IsNullOrEmpty(x.Description)
                    ? null
                    : new DescriptionAttribute {Entries = {x.Description}},
                Locality = String.IsNullOrEmpty(x.Location) ? null : new LocalityAttribute {Entries = {x.Location}}
            }).ToArray();
        }

        private Expression<Func<LdapEvent, bool>> FilterEvent(LdapFilter filter)
        {
            var visitor = new LdapEventFilterVisitor();
            visitor.Visit(filter);
            return visitor.Filter;
        }
    }
}
