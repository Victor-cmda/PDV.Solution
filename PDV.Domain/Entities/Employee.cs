using PDV.Shared.Entities;

namespace PDV.Domain.Entities
{
    public class Employee : BaseEntity
    {
        public string Name { get; set; }
        public string Role { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public DateTime HireDate { get; set; }
    }
}
