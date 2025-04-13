using Microsoft.EntityFrameworkCore;
using PDV.Domain.Entities;

namespace PDV.Infrastructure.Data.Contexts
{
    public class RemoteDbContext : DbContext
    {
        public RemoteDbContext(DbContextOptions<RemoteDbContext> options) : base(options) { }

        public DbSet<Product> Products { get; set; }
        public DbSet<Employee> Employees { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Employee>()
                .Property(e => e.Role)
                .HasConversion<int>();
        }
    }
}
