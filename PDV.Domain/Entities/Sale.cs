namespace PDV.Domain.Entities
{
    public class Sale : BaseEntity
    {
        public string Number { get; set; } = string.Empty;
        public DateTime SaleDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;

        public virtual ICollection<SaleItem> Items { get; set; } = new List<SaleItem>();
    }
}
