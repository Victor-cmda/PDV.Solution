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

            _logger.LogInformation($"Encontradas {localEntities.Count} entidades locais para sincronizar");

            foreach (var entity in localEntities)
            {
                var entityEntry = localContext.Entry(entity);
                var syncState = entityEntry.Property<string>("SyncState").CurrentValue;
                var entityId = GetEntityId(entity);

                ConvertAllDateTimesToUtc(entity);

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

            try
            {
                await remoteContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // Registrar a exceção completa, incluindo exceções internas
                var fullMessage = ex.ToString();
                _logger.LogError($"Erro detalhado ao salvar: {fullMessage}");
                throw;
            }

            // Buscar entidades novas/modificadas do servidor remoto
            DateTime lastSync = DateTime.MinValue;

            // Verificar se há algum registro com timestamp não nulo
            var maxTimestampQuery = localContext.Set<T>()
                .Where(e => EF.Property<DateTime?>(e, "LastSyncTimestamp") != null)
                .Select(e => EF.Property<DateTime?>(e, "LastSyncTimestamp"));

            if (await maxTimestampQuery.AnyAsync())
            {
                // Se houver pelo menos um registro com timestamp não nulo, pegamos o máximo
                var maxTimestamp = await maxTimestampQuery.MaxAsync();
                if (maxTimestamp.HasValue)
                {
                    lastSync = maxTimestamp.Value;
                }
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
                        // Corrigido: usar Entry().Property() em vez de EF.Property()
                        var localSyncState = localContext.Entry(localEntity).Property<string>("SyncState").CurrentValue;

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

        private Guid? GetEntityId(object entity)
        {
            // Método para obter o ID da entidade (Guid)
            var idProperty = entity.GetType().GetProperty("Id");
            return idProperty?.GetValue(entity) as Guid?;
        }

        private async Task<T> FindEntityByIdAsync<T>(DbSet<T> dbSet, Guid id) where T : class
        {
            // Usando expressão lambda para encontrar a entidade pelo ID
            return await dbSet.FirstOrDefaultAsync(e => EF.Property<Guid>(e, "Id") == id);
        }

        private void ConvertAllDateTimesToUtc(object entity)
        {
            var type = entity.GetType();
            var properties = type.GetProperties()
                .Where(p => p.PropertyType == typeof(DateTime) || p.PropertyType == typeof(DateTime?));

            foreach (var property in properties)
            {
                if (property.PropertyType == typeof(DateTime))
                {
                    var value = (DateTime)property.GetValue(entity);
                    if (value.Kind != DateTimeKind.Utc)
                    {
                        // Se não for UTC, especificar como UTC sem alterar o valor
                        property.SetValue(entity, DateTime.SpecifyKind(value, DateTimeKind.Utc));
                    }
                }
                else if (property.PropertyType == typeof(DateTime?))
                {
                    var value = (DateTime?)property.GetValue(entity);
                    if (value.HasValue && value.Value.Kind != DateTimeKind.Utc)
                    {
                        // Se não for UTC, especificar como UTC sem alterar o valor
                        property.SetValue(entity, DateTime.SpecifyKind(value.Value, DateTimeKind.Utc));
                    }
                }
            }
        }
    }
}
