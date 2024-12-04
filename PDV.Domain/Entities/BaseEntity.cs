namespace PDV.Domain.Entities
{
    public abstract class BaseEntity : ISynchronizable
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime LastModified { get; set; }
        public bool IsSynchronized { get; set; }
        public string SyncStatus { get; set; }

        protected BaseEntity()
        {
            CreatedAt = DateTime.Now;
            LastModified = DateTime.Now;
            IsSynchronized = false;
            SyncStatus = "Pending";
        }
    }
}
