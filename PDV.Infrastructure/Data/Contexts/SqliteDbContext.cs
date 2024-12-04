using Microsoft.EntityFrameworkCore;

namespace PDV.Infrastructure.Data.Contexts
{
    public class SqliteDbContext : BaseDbContext
    {
        public SqliteDbContext(DbContextOptions<SqliteDbContext> options)
            : base(options)
        {
        }
    }
}
