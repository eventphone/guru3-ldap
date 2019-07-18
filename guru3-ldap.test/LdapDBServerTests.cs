using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using eventphone.guru3.ldap;
using eventphone.guru3.ldap.DAL;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using zivillian.ldap;

namespace guru3_ldap.test
{
    public class LdapDBServerTests
    {
        protected TestLdapServer GetServer(string name)
        {
            var serviceProvider = new ServiceCollection()
                .AddEntityFrameworkInMemoryDatabase()
                .BuildServiceProvider();

            var options = new DbContextOptionsBuilder<Guru3Context>()
                .UseInMemoryDatabase(databaseName: name)
                .UseInternalServiceProvider(serviceProvider)
                .Options;
            using (var context = new Guru3Context(options))
            {
                AddEvents(context);
                AddExtensions(context);
                context.SaveChanges();
            }
            return new TestLdapServer(options);
        }

        private void AddExtensions(Guru3Context context)
        {
            context.Extensions.Add(new Extension
            {
                Id = 1,
                EventId = 1,
                InPhonebook = true,
                Location = "PoC",
                Name = "zivillian 34c3",
                Number = "4502"
            });
            context.Extensions.Add(new Extension
            {
                Id = 2,
                EventId = 2,
                InPhonebook = true,
                Location = "PoC",
                Name = "zivillian current",
                Number = "4502"
            });
            context.Extensions.Add(new Extension
            {
                Id = 3,
                EventId = 3,
                InPhonebook = true,
                Location = "PoC",
                Name = "zivilliän",
                Number = "4502"
            });
            context.Extensions.Add(new Extension
            {
                Id = 4,
                EventId = 4,
                InPhonebook = true,
                Location = "PoC",
                Name = "zivillian future",
                Number = "4502"
            });
            
            context.Extensions.Add(new Extension
            {
                Id = 5,
                EventId = 1,
                InPhonebook = true,
                Location = "PoC",
                Name = "POC Hotline",
                Number = "2000"
            });
            context.Extensions.Add(new Extension
            {
                Id = 6,
                EventId = 2,
                InPhonebook = true,
                Location = "PoC",
                Name = "POC Hotline",
                Number = "2000"
            });
            context.Extensions.Add(new Extension
            {
                Id = 7,
                EventId = 3,
                InPhonebook = true,
                Location = "PoC",
                Name = "POC Hotline",
                Number = "2000"
            });
            context.Extensions.Add(new Extension
            {
                Id = 8,
                EventId = 4,
                InPhonebook = true,
                Location = "PoC",
                Name = "POC Hotline",
                Number = "2000"
            });
            
            context.Extensions.Add(new Extension
            {
                Id = 9,
                EventId = 1,
                InPhonebook = true,
                Location = "CERT",
                Name = "CERT",
                Number = "112"
            });
            context.Extensions.Add(new Extension
            {
                Id = 10,
                EventId = 2,
                InPhonebook = true,
                Location = "CERT",
                Name = "CERT",
                Number = "112"
            });
            context.Extensions.Add(new Extension
            {
                Id = 11,
                EventId = 3,
                InPhonebook = true,
                Location = "CERT",
                Name = "CÄRT",
                Number = "112"
            });
            context.Extensions.Add(new Extension
            {
                Id = 12,
                EventId = 4,
                InPhonebook = true,
                Location = "CERT",
                Name = "CERT",
                Number = "112"
            });

            context.Extensions.Add(new Extension
            {
                Id = 13,
                EventId = 2,
                InPhonebook = true,
                Location = "",
                Name = "Foo",
                Number = "4444"
            });
        }

        private void AddEvents(Guru3Context context)
        {
            context.Events.Add(new Event
            {
                Id = 1,
                Name = "34c3",
                RegistrationStart = new DateTime(2017, 12, 1),
                End = new DateTime(2017, 12, 30),
                Location = "Leipzig"
            });
            context.Events.Add(new Event
            {
                Id = 2,
                Name = "current",
                RegistrationStart = DateTime.Now.AddDays(-7),
                End = DateTime.Now.AddDays(7),
                Location = "somewhere"
            });
            context.Events.Add(new Event
            {
                Id = 3,
                Name = "ümläut",
                RegistrationStart = DateTime.Now.AddDays(-1),
                End = DateTime.Now.AddDays(14),
                Location = "Düsseldorf"
            });
            context.Events.Add(new Event
            {
                Id = 4,
                Name = "future",
                RegistrationStart = DateTime.Now.AddDays(7),
                End = DateTime.Now.AddDays(14),
                Location = "moon"
            });
        }

        protected class TestLdapServer : LdapDBServer
        {
            private readonly DbContextOptions<Guru3Context> _contextOptions;
            private readonly LdapClientConnection _connection;

            public TestLdapServer(DbContextOptions<Guru3Context> context)
                : base(389, null)
            {
                _contextOptions = context;
                _connection = new LdapClientConnection(null, null, new CancellationTokenSource());
            }

            protected override Guru3Context GetContext()
            {
                return new Guru3Context(_contextOptions);
            }

            protected override void Dispose(bool disposing)
            {
                base.Dispose(disposing);
                if (disposing)
                {
                    _connection.Dispose();
                }
            }

            public Task<ResultCode> Bind(string username, string password)
            {
                var request = new LdapBindRequest(1, username, password, new LdapControl[0]);
                return OnBindAsync(request.Name, request.Simple.Value, _connection);
            }

            public Task<ResultCode> BindSaslPlain(string dn, string username, string password)
            {
                return OnSaslBindAsync(new LdapDistinguishedName(dn), username, Encoding.UTF8.GetBytes(password),
                    _connection);
            }

            public Task<IEnumerable<LdapRequestMessage>> Search(string baseDN, string filter, SearchScope scope)
            {
                return Search(baseDN, filter, scope, 0);
            }

            public Task<IEnumerable<LdapRequestMessage>> Search(string baseDN, string filter, SearchScope scope, int sizeLimit)
            {
                return Search(baseDN, filter, scope, sizeLimit, new string[0]);
            }

            public Task<IEnumerable<LdapRequestMessage>> Search(string baseDN, string filter, SearchScope scope, int sizeLimit, string[] attributes)
            {
                var request = new LdapSearchRequest(2, baseDN, scope, filter, attributes, false, TimeSpan.Zero, sizeLimit, new LdapControl[0]);
                return OnSearchAsync(request, _connection, _connection.CancellationToken);
            }

            public bool HasEvent => !Sessions.IsEmpty;
        }
    }
}