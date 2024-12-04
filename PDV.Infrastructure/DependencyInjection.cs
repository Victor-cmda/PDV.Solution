using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PDV.Infrastructure.Configuration;
using PDV.Infrastructure.Data;
using PDV.Infrastructure.Data.Contexts;
using PDV.Infrastructure.Services;

namespace PDV.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        string postgresConnection,
        string sqliteConnection,
        IConfiguration configuration)
        {
            // Configurar PostgreSQL
            services.AddDbContext<PostgresDbContext>(options =>
                options.UseNpgsql(postgresConnection));

            // Configurar SQLite
            services.AddDbContext<SqliteDbContext>(options =>
                options.UseSqlite(sqliteConnection));

            // Configurar SyncSettings
            services.Configure<SyncSettings>(configuration.GetSection("SyncSettings"));

            // Registrar serviços
            services.AddScoped<SyncService>();
            services.AddScoped<DbContextFactory>();
            services.AddScoped<ConnectionManager>();

            return services;
        }
    }
}
