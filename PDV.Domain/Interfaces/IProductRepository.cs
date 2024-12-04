using PDV.Domain.Entities;

namespace PDV.Domain.Interfaces
{
    public interface IProductRepository : IRepository<Product>
    {
        Task<Product?> GetByCodeAsync(string code);
        Task<Product?> GetByBarCodeAsync(string barCode);
    }
}
