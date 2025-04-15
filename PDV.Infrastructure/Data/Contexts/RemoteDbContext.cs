using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.EntityFrameworkCore;
using PDV.Domain.Entities;

public class RemoteDbContext : DbContext
{
    public RemoteDbContext(DbContextOptions<RemoteDbContext> options) : base(options) { }

    public DbSet<Product> Products { get; set; }
    public DbSet<Employee> Employees { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configurar Role como int
        modelBuilder.Entity<Employee>()
            .Property(e => e.Role)
            .HasConversion<int>();

        // Configurar todas as propriedades DateTime para usar conversão de UTC
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var property in entityType.GetProperties())
            {
                if (property.ClrType == typeof(DateTime) || property.ClrType == typeof(DateTime?))
                {
                    // Adicionar conversor para garantir que somente datas UTC sejam enviadas ao PostgreSQL
                    property.SetValueConverter(new UtcValueConverter());
                }
            }
        }
    }
}

// Conversor para garantir que todas as datas sejam UTC
public class UtcValueConverter : ValueConverter<DateTime, DateTime>
{
    public UtcValueConverter()
        : base(
            v => v.Kind == DateTimeKind.Utc ? v : DateTime.SpecifyKind(v, DateTimeKind.Utc),
            v => DateTime.SpecifyKind(v, DateTimeKind.Utc))
    {
    }
}