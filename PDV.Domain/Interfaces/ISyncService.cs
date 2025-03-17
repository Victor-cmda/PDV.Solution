namespace PDV.Domain.Interfaces
{
    public interface ISyncService
    {
        Task SynchronizeAsync();
        Task<bool> HasPendingSyncItemsAsync();
        Task SynchronizeEntityAsync<T>(string entityName) where T : class;
    }
}
