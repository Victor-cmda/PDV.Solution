using PDV.Domain.Entities;

namespace PDV.Domain.Interfaces
{
    public interface ISaleRepository : IRepository<Sale>
    {
        Task<Sale?> GetByNumberAsync(string number);
        Task<IEnumerable<Sale>> GetByDateRangeAsync(DateTime start, DateTime end);
    }
}
