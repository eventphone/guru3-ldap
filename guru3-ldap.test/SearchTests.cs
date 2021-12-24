using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using zivillian.ldap;

namespace guru3_ldap.test
{
    public class SearchTests : LdapDBServerTests
    {
        [Theory]
        [InlineData("", "dc=eventphone,dc=de", 7+1+1)]
        [InlineData("cn=34c3", "dc=eventphone,dc=de")]
        [InlineData("cn=current", "dc=eventphone,dc=de", 4+1)]
        [InlineData("cn=ümläut", "dc=eventphone,dc=de", 3+1)]
        [InlineData("cn=future", "dc=eventphone,dc=de")]

        [InlineData("", "ou=current,dc=eventphone,dc=de", 4)]
        [InlineData("cn=34c3", "ou=current,dc=eventphone,dc=de")]
        [InlineData("cn=current", "ou=current,dc=eventphone,dc=de", 4)]
        [InlineData("cn=ümläut", "ou=current,dc=eventphone,dc=de")]
        [InlineData("cn=future", "ou=current,dc=eventphone,dc=de")]

        [InlineData("", "ou=34c3,dc=eventphone,dc=de")]
        [InlineData("cn=34c3", "ou=34c3,dc=eventphone,dc=de")]
        [InlineData("cn=current", "ou=34c3,dc=eventphone,dc=de")]
        [InlineData("cn=ümläut", "ou=34c3,dc=eventphone,dc=de")]
        [InlineData("cn=future", "ou=34c3,dc=eventphone,dc=de")]

        [InlineData("", "ou=future,dc=eventphone,dc=de")]
        [InlineData("cn=34c3", "ou=future,dc=eventphone,dc=de")]
        [InlineData("cn=current", "ou=future,dc=eventphone,dc=de")]
        [InlineData("cn=ümläut", "ou=future,dc=eventphone,dc=de")]
        [InlineData("cn=future", "ou=future,dc=eventphone,dc=de")]

        [InlineData("", "cn=4502,ou=current,dc=eventphone,dc=de")]
        [InlineData("cn=34c3", "cn=4502,ou=current,dc=eventphone,dc=de")]
        [InlineData("cn=current", "cn=4502,ou=current,dc=eventphone,dc=de")]
        [InlineData("cn=ümläut", "cn=4502,ou=current,dc=eventphone,dc=de")]
        [InlineData("cn=future", "cn=4502,ou=current,dc=eventphone,dc=de")]
        public async Task CanListSubtree(string username, string baseDN, int resultCount = 0)
        {
            var results = await Search(username, baseDN, "(objectclass=*)", $"{nameof(CanListSubtree)}_{username}_{baseDN}");
            Assert.Equal(resultCount, results.Length);
            if (resultCount > 0)
            {
                Assert.Contains(results, x => x.ObjectName.RDNs[0].Values[0].Value == "2000");
                Assert.Contains(results, x => x.ObjectName.RDNs[0].Values[0].Value == "4502");
            }
        }
        
        [Theory]
        [InlineData("", "dc=eventphone,dc=de", 4)]
        [InlineData("cn=34c3", "dc=eventphone,dc=de")]
        [InlineData("cn=current", "dc=eventphone,dc=de", 2)]
        [InlineData("cn=ümläut", "dc=eventphone,dc=de", 2)]
        [InlineData("cn=future", "dc=eventphone,dc=de")]

        [InlineData("", "ou=current,dc=eventphone,dc=de", 2)]
        [InlineData("cn=34c3", "ou=current,dc=eventphone,dc=de")]
        [InlineData("cn=current", "ou=current,dc=eventphone,dc=de", 2)]
        [InlineData("cn=ümläut", "ou=current,dc=eventphone,dc=de")]
        [InlineData("cn=future", "ou=current,dc=eventphone,dc=de")]

        [InlineData("", "ou=34c3,dc=eventphone,dc=de")]
        [InlineData("cn=34c3", "ou=34c3,dc=eventphone,dc=de")]
        [InlineData("cn=current", "ou=34c3,dc=eventphone,dc=de")]
        [InlineData("cn=ümläut", "ou=34c3,dc=eventphone,dc=de")]
        [InlineData("cn=future", "ou=34c3,dc=eventphone,dc=de")]

        [InlineData("", "ou=future,dc=eventphone,dc=de")]
        [InlineData("cn=34c3", "ou=future,dc=eventphone,dc=de")]
        [InlineData("cn=current", "ou=future,dc=eventphone,dc=de")]
        [InlineData("cn=ümläut", "ou=future,dc=eventphone,dc=de")]
        [InlineData("cn=future", "ou=future,dc=eventphone,dc=de")]

        [InlineData("", "cn=4502,ou=current,dc=eventphone,dc=de")]
        [InlineData("cn=34c3", "cn=4502,ou=current,dc=eventphone,dc=de")]
        [InlineData("cn=current", "cn=4502,ou=current,dc=eventphone,dc=de")]
        [InlineData("cn=ümläut", "cn=4502,ou=current,dc=eventphone,dc=de")]
        [InlineData("cn=future", "cn=4502,ou=current,dc=eventphone,dc=de")]
        public async Task CanSearchMany(string username, string baseDN, int resultCount = 0)
        {
            var results = await Search(username, baseDN, "(l=PoC)", $"{nameof(CanSearchMany)}_{username}_{baseDN}");
            Assert.Equal(resultCount, results.Length);
            if (resultCount > 0)
            {
                Assert.Contains(results, x => x.ObjectName.RDNs[0].Values[0].Value == "2000");
                Assert.Contains(results, x => x.ObjectName.RDNs[0].Values[0].Value == "4502");
            }
        }

        [Theory]
        [InlineData("", "dc=eventphone,dc=de", 1)]
        [InlineData("cn=34c3", "dc=eventphone,dc=de")]
        [InlineData("cn=current", "dc=eventphone,dc=de", 1)]
        [InlineData("cn=ümläut", "dc=eventphone,dc=de")]
        [InlineData("cn=future", "dc=eventphone,dc=de")]

        [InlineData("", "ou=current,dc=eventphone,dc=de", 1)]
        [InlineData("cn=34c3", "ou=current,dc=eventphone,dc=de")]
        [InlineData("cn=current", "ou=current,dc=eventphone,dc=de", 1)]
        [InlineData("cn=ümläut", "ou=current,dc=eventphone,dc=de")]
        [InlineData("cn=future", "ou=current,dc=eventphone,dc=de")]

        [InlineData("", "ou=34c3,dc=eventphone,dc=de")]
        [InlineData("cn=34c3", "ou=34c3,dc=eventphone,dc=de")]
        [InlineData("cn=current", "ou=34c3,dc=eventphone,dc=de")]
        [InlineData("cn=ümläut", "ou=34c3,dc=eventphone,dc=de")]
        [InlineData("cn=future", "ou=34c3,dc=eventphone,dc=de")]

        [InlineData("", "ou=future,dc=eventphone,dc=de")]
        [InlineData("cn=34c3", "ou=future,dc=eventphone,dc=de")]
        [InlineData("cn=current", "ou=future,dc=eventphone,dc=de")]
        [InlineData("cn=ümläut", "ou=future,dc=eventphone,dc=de")]
        [InlineData("cn=future", "ou=future,dc=eventphone,dc=de")]

        [InlineData("", "cn=4502,ou=current,dc=eventphone,dc=de")]
        [InlineData("cn=34c3", "cn=4502,ou=current,dc=eventphone,dc=de")]
        [InlineData("cn=current", "cn=4502,ou=current,dc=eventphone,dc=de")]
        [InlineData("cn=ümläut", "cn=4502,ou=current,dc=eventphone,dc=de")]
        [InlineData("cn=future", "cn=4502,ou=current,dc=eventphone,dc=de")]
        public async Task CanSearchSingle(string username, string baseDN, int resultCount = 0)
        {
            var results = await Search(username, baseDN, "(sn=CERT)", $"{nameof(CanSearchSingle)}_{username}_{baseDN}");
            Assert.Equal(resultCount, results.Length);
            if (resultCount > 0)
            {
                Assert.Contains(results, x => x.ObjectName.RDNs[0].Values[0].Value == "112");
            }
        }
        
        [Theory]
        [InlineData("", "dc=eventphone,dc=de", 2)]
        [InlineData("cn=34c3", "dc=eventphone,dc=de")]
        [InlineData("cn=current", "dc=eventphone,dc=de", 1)]
        [InlineData("cn=ümläut", "dc=eventphone,dc=de", 1)]
        [InlineData("cn=future", "dc=eventphone,dc=de")]

        [InlineData("", "ou=current,dc=eventphone,dc=de", 1)]
        [InlineData("cn=34c3", "ou=current,dc=eventphone,dc=de")]
        [InlineData("cn=current", "ou=current,dc=eventphone,dc=de", 1)]
        [InlineData("cn=ümläut", "ou=current,dc=eventphone,dc=de")]
        [InlineData("cn=future", "ou=current,dc=eventphone,dc=de")]

        [InlineData("", "ou=34c3,dc=eventphone,dc=de")]
        [InlineData("cn=34c3", "ou=34c3,dc=eventphone,dc=de")]
        [InlineData("cn=current", "ou=34c3,dc=eventphone,dc=de")]
        [InlineData("cn=ümläut", "ou=34c3,dc=eventphone,dc=de")]
        [InlineData("cn=future", "ou=34c3,dc=eventphone,dc=de")]

        [InlineData("", "ou=future,dc=eventphone,dc=de")]
        [InlineData("cn=34c3", "ou=future,dc=eventphone,dc=de")]
        [InlineData("cn=current", "ou=future,dc=eventphone,dc=de")]
        [InlineData("cn=ümläut", "ou=future,dc=eventphone,dc=de")]
        [InlineData("cn=future", "ou=future,dc=eventphone,dc=de")]

        [InlineData("", "cn=4502,ou=current,dc=eventphone,dc=de")]
        [InlineData("cn=34c3", "cn=4502,ou=current,dc=eventphone,dc=de")]
        [InlineData("cn=current", "cn=4502,ou=current,dc=eventphone,dc=de")]
        [InlineData("cn=ümläut", "cn=4502,ou=current,dc=eventphone,dc=de")]
        [InlineData("cn=future", "cn=4502,ou=current,dc=eventphone,dc=de")]
        public async Task CanSearchExtension(string username, string baseDN, int resultCount = 0)
        {
            var results = await Search(username, baseDN, "(cn=4502)", $"{nameof(CanSearchExtension)}_{username}_{baseDN}");
            Assert.Equal(resultCount, results.Length);
            if (resultCount > 0)
            {
                Assert.Contains(results, x => x.ObjectName.RDNs[0].Values[0].Value == "4502");
            }
        }
        
        [Theory]
        [InlineData("", "dc=eventphone,dc=de", 1)]
        [InlineData("cn=34c3", "dc=eventphone,dc=de")]
        [InlineData("cn=current", "dc=eventphone,dc=de")]
        [InlineData("cn=ümläut", "dc=eventphone,dc=de", 1)]
        [InlineData("cn=future", "dc=eventphone,dc=de")]

        [InlineData("", "ou=current,dc=eventphone,dc=de")]
        [InlineData("cn=34c3", "ou=current,dc=eventphone,dc=de")]
        [InlineData("cn=current", "ou=current,dc=eventphone,dc=de")]
        [InlineData("cn=ümläut", "ou=current,dc=eventphone,dc=de")]
        [InlineData("cn=future", "ou=current,dc=eventphone,dc=de")]

        [InlineData("", "ou=34c3,dc=eventphone,dc=de")]
        [InlineData("cn=34c3", "ou=34c3,dc=eventphone,dc=de")]
        [InlineData("cn=current", "ou=34c3,dc=eventphone,dc=de")]
        [InlineData("cn=ümläut", "ou=34c3,dc=eventphone,dc=de")]
        [InlineData("cn=future", "ou=34c3,dc=eventphone,dc=de")]

        [InlineData("", "ou=future,dc=eventphone,dc=de")]
        [InlineData("cn=34c3", "ou=future,dc=eventphone,dc=de")]
        [InlineData("cn=current", "ou=future,dc=eventphone,dc=de")]
        [InlineData("cn=ümläut", "ou=future,dc=eventphone,dc=de")]
        [InlineData("cn=future", "ou=future,dc=eventphone,dc=de")]

        [InlineData("", "cn=4502,ou=current,dc=eventphone,dc=de")]
        [InlineData("cn=34c3", "cn=4502,ou=current,dc=eventphone,dc=de")]
        [InlineData("cn=current", "cn=4502,ou=current,dc=eventphone,dc=de")]
        [InlineData("cn=ümläut", "cn=4502,ou=current,dc=eventphone,dc=de")]
        [InlineData("cn=future", "cn=4502,ou=current,dc=eventphone,dc=de")]
        public async Task CanSearchUmlauts(string username, string baseDN, int resultCount = 0)
        {
            var results = await Search(username, baseDN, "(sn=zivilliän)", $"{nameof(CanSearchUmlauts)}_{username}_{baseDN}");
            Assert.Equal(resultCount, results.Length);
            if (resultCount > 0)
            {
                Assert.Contains(results, x => x.ObjectName.RDNs[0].Values[0].Value == "4502");
            }
        }

        [Theory]
        [InlineData("(objectclass=*)", SearchScope.WholeSubtree, 5)]
        [InlineData("(objectclass=*)", SearchScope.SingleLevel, 1)]
        [InlineData("(l=PoC)", SearchScope.WholeSubtree, 3)]
        [InlineData("(|(l=PoC)(ou=*ture)(l=some*))", SearchScope.WholeSubtree, 5)]
        [InlineData("(l=*)", SearchScope.WholeSubtree, 2)]
        public async Task SearchRespectsLimit(string filter, SearchScope scope, int sizeLimit)
        {
            using (var server = GetServer($"{nameof(SearchRespectsLimit)}_{filter}_{scope}"))
            {
                await server.Bind(String.Empty, String.Empty);
                var search = await server.Search("dc=eventphone,dc=de", filter, scope, sizeLimit);
                var results = search.ToArray();
                Assert.True(results.Length <= sizeLimit);
            }
        }

        [Fact]
        public async Task CanSearchNotEvent()
        {
            var search = await Search(String.Empty, "dc=eventphone,dc=de", "(&(!(ou=current))(telephoneNumber=4502))", nameof(CanSearchEvent));
            var result = Assert.Single(search);
            Assert.NotEqual("cn=4502,ou=current,dc=eventphone,dc=de", result.ObjectName.ToString());
        }

        [Fact]
        public async Task CanSearchEventExtension()
        {
            var search = await Search(String.Empty, "dc=eventphone,dc=de", "(&(ou=current)(telephoneNumber=4502))", nameof(CanSearchEvent));
            var result = Assert.Single(search);
            Assert.Equal("cn=4502,ou=current,dc=eventphone,dc=de", result.ObjectName.ToString());
        }

        [Fact]
        public async Task CanSearchEvent()
        {
            var search = await Search(String.Empty, "dc=eventphone,dc=de", "(ou=current)", nameof(CanSearchEvent));
            Assert.Contains(search, x =>x.ObjectName.ToString() == "ou=current,dc=eventphone,dc=de");
        }

        [Fact]
        public async Task CanSearchLocationPresent()
        {
            var search = await Search(String.Empty, "dc=eventphone,dc=de", "(l=*)", nameof(CanSearchLocationPresent));
            var results = search.ToArray();
            Assert.NotEmpty(results);
            foreach (var entry in results)
            {
                var attr = entry.Attributes.Where(x => x.Type.Oid == "l").SelectMany(x => x.Values);
                Assert.Single(attr);
            }
        }

        [Fact]
        public async Task CanSearchGreater()
        {
            var search = await Search(String.Empty, "dc=eventphone,dc=de", "(sn>=0)", nameof(CanSearchGreater));
            var results = search.ToArray();
            Assert.NotEmpty(results);
        }

        private async Task<LdapSearchResultEntry[]> Search(string username, string baseDN, string filter, string testname)
        {
            using (var server = GetServer(testname))
            {
                var bind = await server.Bind(username, String.Empty);
                Assert.Equal(ResultCode.Success, bind);
                var search = await server.Search(baseDN, filter, SearchScope.WholeSubtree);
                return search.OfType<LdapSearchResultEntry>().ToArray();
            }
        }

    }
}