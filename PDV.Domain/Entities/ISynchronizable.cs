namespace PDV.Domain.Entities
{
    public interface ISynchronizable
    {
        DateTime LastModified { get; set; }
        bool IsSynchronized { get; set; }
        string SyncStatus { get; set; }
    }
}
