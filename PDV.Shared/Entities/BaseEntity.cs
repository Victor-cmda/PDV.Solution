namespace PDV.Shared.Entities
{
    public class BaseEntity
    {
        public Guid Id { get; set; }
        public DateTime? LastSyncTimestamp { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
