using System;
using System.Linq;
using System.Threading.Tasks;
using eventphone.guru3.ldap.DAL;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace guru3_ldap.test
{
    public class DatabaseTest
    {
        private readonly string _connectionString = "<private>";
        [Fact]
        public void CanConnect()
        {
            using (var context = new Guru3Context(_connectionString))
            {
                context.Database.ExecuteSqlRaw("SELECT NOW();");
            }
        }

        [Fact]
        public async Task CanGetEvent()
        {
            using (var context = new Guru3Context(_connectionString))
            {
                var froscon = await context.Events.FirstOrDefaultAsync(x => x.Id == 66);
                Assert.NotNull(froscon);
                Assert.Equal(66, froscon.Id);
                Assert.Equal("FrOSCon 2018", froscon.Name);
                Assert.Equal("FH Sankt Augustin", froscon.Location);
                Assert.Equal(new DateTime(2018,06,01), froscon.RegistrationStart);
                Assert.Equal(new DateTime(2018,08,26), froscon.End);
                Assert.Equal("de", froscon.DescriptionDe);
            }
        }

        [Fact]
        public async Task CanGetExtension()
        {
            using (var context = new Guru3Context(_connectionString))
            {
                var ext = await context.Extensions.Where(x => x.EventId == 66).FirstOrDefaultAsync(x=>x.Name.Contains("ziv"));
                Assert.NotNull(ext);
                Assert.Equal(45348, ext.Id);
                Assert.Equal("zivillian", ext.Name);
                Assert.Equal("PoC", ext.Location);
                Assert.True(ext.InPhonebook);
                Assert.Equal("4502", ext.Number);
            }
        }
    }
}
