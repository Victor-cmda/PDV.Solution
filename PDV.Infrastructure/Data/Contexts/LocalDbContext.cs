using Microsoft.EntityFrameworkCore;
using PDV.Domain.Entities;

namespace PDV.Infrastructure.Data.Contexts
{
    public class LocalDbContext : DbContext
    {
        public LocalDbContext(DbContextOptions<LocalDbContext> options) : base(options) { }

        public DbSet<Product> Products { get; set; }
        public DbSet<Employee> Employees { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Employee>()
                .Property(e => e.Role)
                .HasConversion<int>();

            modelBuilder.Entity<Product>().Property<DateTime>("LastSyncTimestamp")
                .HasDefaultValue(DateTime.MinValue);
            modelBuilder.Entity<Product>().Property<string>("SyncState")
                .HasMaxLength(20)
                .HasDefaultValue("Unchanged");

            modelBuilder.Entity<Employee>().Property<DateTime>("LastSyncTimestamp")
                .HasDefaultValue(DateTime.MinValue);
            modelBuilder.Entity<Employee>().Property<string>("SyncState")
                .HasMaxLength(20)
                .HasDefaultValue("Unchanged");
        }
    }
}
