using Microsoft.EntityFrameworkCore;
using PDV.Infrastructure.Data.Contexts;

namespace PDV.Infrastructure.Data
{
    public class DbContextFactory
    {
        private readonly DbContextOptions<PostgresDbContext> _postgresOptions;
        private readonly DbContextOptions<SqliteDbContext> _sqliteOptions;

        public DbContextFactory(
            DbContextOptions<PostgresDbContext> postgresOptions,
            DbContextOptions<SqliteDbContext> sqliteOptions)
        {
            _postgresOptions = postgresOptions;
            _sqliteOptions = sqliteOptions;
        }

        public PostgresDbContext CreatePostgresContext()
            => new PostgresDbContext(_postgresOptions);

        public SqliteDbContext CreateSqliteContext()
            => new SqliteDbContext(_sqliteOptions);
    }
}
