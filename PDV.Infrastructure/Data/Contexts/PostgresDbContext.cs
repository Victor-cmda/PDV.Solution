using Microsoft.EntityFrameworkCore;

namespace PDV.Infrastructure.Data.Contexts
{
    public class PostgresDbContext : BaseDbContext
    {
        public PostgresDbContext(DbContextOptions<PostgresDbContext> options)
            : base(options)
        {
        }
    }
}
