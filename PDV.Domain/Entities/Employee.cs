using System.ComponentModel.DataAnnotations;

namespace PDV.Domain.Entities
{
    public class Employee
    {
        public string Name { get; set; }
        public string Role { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public DateTime HireDate { get; set; }
    }
}
