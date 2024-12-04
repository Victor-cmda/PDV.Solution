using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PDV.Infrastructure.Data.Contexts;

namespace PDV.Infrastructure.Services
{
    public class SyncService
    {
        private readonly PostgresDbContext _remoteDb;
        private readonly SqliteDbContext _localDb;
        private readonly ILogger<SyncService> _logger;

        public SyncService(
            PostgresDbContext remoteDb,
            SqliteDbContext localDb,
            ILogger<SyncService> logger)
        {
            _remoteDb = remoteDb;
            _localDb = localDb;
            _logger = logger;
        }

        public async Task SynchronizeAsync()
        {
            try
            {
                // Sincronizar produtos
                await SyncProducts();

                // Sincronizar vendas
                await SyncSales();

                // Adicione outras sincronizações conforme necessário
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro durante a sincronização");
                throw;
            }
        }

        private async Task SyncProducts()
        {
            var localProducts = await _localDb.Products
                .Where(p => !p.IsSynchronized)
                .ToListAsync();

            foreach (var product in localProducts)
            {
                var remoteProduct = await _remoteDb.Products
                    .FirstOrDefaultAsync(p => p.Id == product.Id);

                if (remoteProduct == null)
                {
                    _remoteDb.Products.Add(product);
                }
                else
                {
                    // Atualizar apenas se o local for mais recente
                    if (product.LastModified > remoteProduct.LastModified)
                    {
                        _remoteDb.Entry(remoteProduct).CurrentValues.SetValues(product);
                    }
                }

                product.IsSynchronized = true;
                product.SyncStatus = "Synchronized";
            }

            await _remoteDb.SaveChangesAsync();
            await _localDb.SaveChangesAsync();
        }

        private async Task SyncSales()
        {
            // Implementação similar para vendas
        }
    }
}
