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

        public Guru3Context(DbContextOptions<Guru3Context> options)
            : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseNpgsql(_connectionString);
            }
        }

        public DbSet<Event> Events { get; set; }
        
        public DbSet<Extension> Extensions { get; set; }

        public DbSet<Location> Locations { get; set; }
    }
}
