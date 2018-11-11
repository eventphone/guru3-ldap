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
        [InlineData("", "dc=eventphone,dc=de", 6+1+1)]
        [InlineData("cn=34c3", "dc=eventphone,dc=de")]
        [InlineData("cn=current", "dc=eventphone,dc=de", 3+1)]
        [InlineData("cn=ümläut", "dc=eventphone,dc=de", 3+1)]
        [InlineData("cn=future", "dc=eventphone,dc=de")]

        [InlineData("", "ou=current,dc=eventphone,dc=de", 3)]
        [InlineData("cn=34c3", "ou=current,dc=eventphone,dc=de")]
        [InlineData("cn=current", "ou=current,dc=eventphone,dc=de", 3)]
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

        private async Task<LdapSearchResultEntry[]> Search(string username, string baseDN, string filter, string testname)
        {
            using (var server = GetServer(testname))
            {
                var bind = await server.Bind(username, String.Empty);
                Assert.Equal(ResultCode.Success, bind.ResultCode);
                var search = await server.Search(baseDN, filter, SearchScope.WholeSubtree);
                return search.OfType<LdapSearchResultEntry>().ToArray();
            }
        }

    }
}