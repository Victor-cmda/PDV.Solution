using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PDV.Domain.Entities;
using PDV.Domain.Interfaces;
using PDV.Domain.Interfaces.PDV.Domain.Interfaces;
using PDV.Infrastructure.Data.Contexts;

namespace PDV.Infrastructure.Synchronization
{
    public class SyncService : ISyncService
    {
        private readonly IDbContextFactory<LocalDbContext> _localContextFactory;
        private readonly IDbContextFactory<RemoteDbContext> _remoteContextFactory;
        private readonly IConnectivityService _connectivityService;
        private readonly ISyncNotificationService _syncNotificationService;
        private readonly ILogger<SyncService> _logger;

        public SyncService(
            IDbContextFactory<LocalDbContext> localContextFactory,
            IDbContextFactory<RemoteDbContext> remoteContextFactory,
            IConnectivityService connectivityService,
            ISyncNotificationService syncNotificationService,
            ILogger<SyncService> logger)
        {
            _localContextFactory = localContextFactory;
            _remoteContextFactory = remoteContextFactory;
            _connectivityService = connectivityService;
            _syncNotificationService = syncNotificationService;
            _logger = logger;
        }

        public async Task<bool> HasPendingSyncItemsAsync()
        {
            using var localContext = await _localContextFactory.CreateDbContextAsync();

            var hasPendingProducts = await localContext.Products
                .AnyAsync(p => EF.Property<string>(p, "SyncState") != "Unchanged");

            var hasPendingEmployees = await localContext.Employees
                .AnyAsync(e => EF.Property<string>(e, "SyncState") != "Unchanged");

            return hasPendingProducts || hasPendingEmployees;
        }

        public async Task SynchronizeAsync()
        {
            try
            {
                await _connectivityService.CheckAndUpdateConnectivityAsync();

                if (!_connectivityService.IsOnline())
                {
                    _logger.LogWarning("Não é possível sincronizar, dispositivo está offline");
                    _syncNotificationService.NotifySyncFailed("Dispositivo está offline");
                    return;
                }

                _syncNotificationService.NotifySyncStarted();

                await SynchronizeEntityAsync<Product>("Products");
                await SynchronizeEntityAsync<Employee>("Employees");

                _syncNotificationService.NotifySyncCompleted();
                _logger.LogInformation("Sincronização completa");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro durante a sincronização");
                _syncNotificationService.NotifySyncFailed(ex.Message);
                throw;
            }
        }

        public async Task SynchronizeEntityAsync<T>(string entityName) where T : class
        {
            _logger.LogInformation($"Sincronizando {entityName}");

            using var localContext = await _localContextFactory.CreateDbContextAsync();
            using var remoteContext = await _remoteContextFactory.CreateDbContextAsync();

            // Obter entidades que precisam ser sincronizadas (New, Modified, Deleted)
            var localEntities = await localContext.Set<T>()
                .Where(e => EF.Property<string>(e, "SyncState") != "Unchanged")
                .ToListAsync();

            foreach (var entity in localEntities)
            {
                var syncState = EF.Property<string>(entity, "SyncState");
                var entityId = GetEntityId(entity);

                if (entityId != null)
                {
                    var remoteEntity = await FindEntityByIdAsync(remoteContext.Set<T>(), entityId.Value);

                    switch (syncState)
                    {
                        case "New":
                            if (remoteEntity == null)
                            {
                                await remoteContext.Set<T>().AddAsync(entity);
                            }
                            break;

                        case "Modified":
                            if (remoteEntity != null)
                            {
                                remoteContext.Entry(remoteEntity).CurrentValues.SetValues(entity);
                            }
                            break;

                        case "Deleted":
                            if (remoteEntity != null)
                            {
                                remoteContext.Set<T>().Remove(remoteEntity);
                            }
                            break;
                    }
                }

                // Marcar como sincronizado
                localContext.Entry(entity).Property("SyncState").CurrentValue = "Unchanged";
                localContext.Entry(entity).Property("LastSyncTimestamp").CurrentValue = DateTime.UtcNow;
            }

            // Salvar mudanças no servidor
            await remoteContext.SaveChangesAsync();

            // Buscar entidades novas/modificadas do servidor remoto
            DateTime lastSync = DateTime.MinValue;
            var lastSyncQuery = localContext.Set<T>()
                .Select(e => EF.Property<DateTime>(e, "LastSyncTimestamp"));

            if (await lastSyncQuery.AnyAsync())
            {
                lastSync = await lastSyncQuery.MaxAsync();
            }

            // Determinar quais entidades buscar do servidor (simplificado)
            var remoteEntities = new List<T>();

            if (typeof(T) == typeof(Product))
            {
                remoteEntities = await remoteContext.Set<Product>()
                    .Where(p => p.LastUpdate > lastSync)
                    .Cast<T>()
                    .ToListAsync();
            }
            else if (typeof(T) == typeof(Employee))
            {
                remoteEntities = await remoteContext.Set<Employee>()
                    .Where(e => e.HireDate > lastSync)
                    .Cast<T>()
                    .ToListAsync();
            }

            // Aplicar entidades do servidor ao banco local
            foreach (var remoteEntity in remoteEntities)
            {
                var entityId = GetEntityId(remoteEntity);

                if (entityId != null)
                {
                    var localEntity = await FindEntityByIdAsync(localContext.Set<T>(), entityId.Value);

                    if (localEntity == null)
                    {
                        // Nova entidade do servidor
                        await localContext.Set<T>().AddAsync(remoteEntity);
                        localContext.Entry(remoteEntity).Property("SyncState").CurrentValue = "Unchanged";
                        localContext.Entry(remoteEntity).Property("LastSyncTimestamp").CurrentValue = DateTime.UtcNow;
                    }
                    else
                    {
                        var localSyncState = EF.Property<string>(localEntity, "SyncState");

                        // Só atualiza se não foi modificado localmente
                        if (localSyncState == "Unchanged")
                        {
                            localContext.Entry(localEntity).CurrentValues.SetValues(remoteEntity);
                            localContext.Entry(localEntity).Property("LastSyncTimestamp").CurrentValue = DateTime.UtcNow;
                        }
                    }
                }
            }

            // Salvar mudanças locais
            await localContext.SaveChangesAsync();
        }

        private int? GetEntityId(object entity)
        {
            // Método simples para obter o ID da entidade
            var idProperty = entity.GetType().GetProperty("Id");
            return idProperty?.GetValue(entity) as int?;
        }

        private async Task<T> FindEntityByIdAsync<T>(DbSet<T> dbSet, int id) where T : class
        {
            // Método para encontrar entidade pelo ID
            return await dbSet.FindAsync(id);
        }
    }
}