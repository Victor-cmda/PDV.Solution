using PDV.Shared.Entities;
using System;

namespace PDV.Domain.Entities
{
    /// <summary>
    /// Classe base para pessoas no sistema (funcionários, clientes, fornecedores)
    /// </summary>
    public abstract class Person : BaseEntity
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Document { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string ZipCode { get; set; }
        public DateTime BirthDate { get; set; }
    }
}