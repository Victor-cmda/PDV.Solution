namespace PDV.Infrastructure.Configuration
{
    public class SyncSettings
    {
        public TimeSpan AutoSyncInterval { get; set; }
        public int RetryAttempts { get; set; }
    }
}
