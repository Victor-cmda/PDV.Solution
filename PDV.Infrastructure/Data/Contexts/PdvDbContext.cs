using Microsoft.EntityFrameworkCore;
using PDV.Domain.Entities;

namespace PDV.Infrastructure.Data.Contexts
{
    public class PdvDbContext : DbContext
    {
        public PdvDbContext(DbContextOptions<PdvDbContext> options) : base(options) { }

        public DbSet<Product> Products { get; set; }
        public DbSet<Employee> Employees { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Product>().Property<DateTime>("LastSyncTimestamp");
            modelBuilder.Entity<Employee>().Property<DateTime>("LastSyncTimestamp");
        }
    }
}
