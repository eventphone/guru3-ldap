using System;
using System.Threading.Tasks;
using Xunit;
using zivillian.ldap;

namespace guru3_ldap.test
{
    public class BindTests : LdapDBServerTests
    {
        [Fact]
        public async Task CanBindAnon()
        {
            using (var server = GetServer(nameof(CanBindAnon)))
            {
                var result = await server.Bind(String.Empty, String.Empty);
                Assert.Equal(ResultCode.Success, result.ResultCode);
                Assert.False(server.HasEvent);
            }
        }

        [Fact]
        public async Task CanBindEvent()
        {
            using (var server = GetServer(nameof(CanBindEvent)))
            {
                var result = await server.Bind("ou=current", Guid.NewGuid().ToString());
                Assert.Equal(ResultCode.Success, result.ResultCode);
                Assert.True(server.HasEvent);
            }
        }

        [Fact]
        public async Task CannotBindInvalidEvent()
        {
            using (var server = GetServer(nameof(CannotBindInvalidEvent)))
            {
                var result = await server.Bind("cn=" + Guid.NewGuid().ToString(), Guid.NewGuid().ToString());
                Assert.Equal(ResultCode.InvalidCredentials, result.ResultCode);
                Assert.False(server.HasEvent);
            }
        }

        [Fact]
        public async Task CanBindPastEvent()
        {
            using (var server = GetServer(nameof(CanBindPastEvent)))
            {
                var result = await server.Bind("cn=34c3", Guid.NewGuid().ToString());
                Assert.Equal(ResultCode.Success, result.ResultCode);
                Assert.True(server.HasEvent);
            }
        }

        [Fact]
        public async Task CanBindFutureEvent()
        {
            using (var server = GetServer(nameof(CanBindFutureEvent)))
            {
                var result = await server.Bind("cn=future", Guid.NewGuid().ToString());
                Assert.Equal(ResultCode.Success, result.ResultCode);
                Assert.True(server.HasEvent);
            }
        }

        [Fact]
        public async Task CanBindUmlautEvent()
        {
            using (var server = GetServer(nameof(CanBindUmlautEvent)))
            {
                var result = await server.Bind("cn=ümläut", Guid.NewGuid().ToString());
                Assert.Equal(ResultCode.Success, result.ResultCode);
                Assert.True(server.HasEvent);
            }
        }
    }
}