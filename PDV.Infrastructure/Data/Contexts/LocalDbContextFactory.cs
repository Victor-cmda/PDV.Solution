using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace PDV.Infrastructure.Data.Contexts
{
    public class LocalDbContextFactory : IDesignTimeDbContextFactory<LocalDbContext>
    {
        public LocalDbContext CreateDbContext(string[] args)
        {
            // Definir o caminho para o banco SQLite
            var basePath = Directory.GetCurrentDirectory();
            var dbPath = Path.Combine(basePath, "pdv.db");
            var connectionString = $"Data Source={dbPath}";

            var optionsBuilder = new DbContextOptionsBuilder<LocalDbContext>();
            optionsBuilder.UseSqlite(connectionString,
                x => x.MigrationsAssembly("PDV.Infrastructure"));

            return new LocalDbContext(optionsBuilder.Options);
        }
    }
}