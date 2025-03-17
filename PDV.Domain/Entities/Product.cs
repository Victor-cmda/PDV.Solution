using PDV.Shared.Entities;

namespace PDV.Domain.Entities
{
    public class Product : BaseEntity
    {
        public string Name { get; set; }
        public string Category { get; set; }
        public decimal Price { get; set; }
        public string Barcode { get; set; }
        public int Stock { get; set; }
        public string Supplier { get; set; }
        public DateTime LastUpdate { get; set; }
    }

}
