using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace eventphone.guru3.ldap.DAL
{
    public class Guru3Context:DbContext
    {
        private readonly string _connectionString;

        public Guru3Context(string connectionString)
        {
            _connectionString = connectionString;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseMySql(_connectionString)
                .ConfigureWarnings(warnings => warnings.Throw(RelationalEventId.QueryClientEvaluationWarning));;
        }

        public DbSet<Event> Events { get; set; }
        
        public DbSet<Extension> Extensions { get; set; }
    }
}
