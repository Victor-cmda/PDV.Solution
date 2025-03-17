using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace PDV.Infrastructure.Data.Contexts
{
    public class RemoteDbContextFactory : IDesignTimeDbContextFactory<RemoteDbContext>
    {
        public RemoteDbContext CreateDbContext(string[] args)
        {
            // Obter configuração de um arquivo appsettings.json ou definir diretamente
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false)
                .Build();

            var connectionString = configuration.GetConnectionString("PostgresConnection");
            // Ou defina diretamente se preferir
            // var connectionString = "Host=your-server;Database=pdv;Username=your-user;Password=your-password";

            var optionsBuilder = new DbContextOptionsBuilder<RemoteDbContext>();
            optionsBuilder.UseNpgsql(connectionString,
                x => x.MigrationsAssembly("PDV.Infrastructure"));

            return new RemoteDbContext(optionsBuilder.Options);
        }
    }
}
