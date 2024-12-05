using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDV.Domain.Entities
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Category { get; set; }
        public decimal Price { get; set; }
        public string Barcode { get; set; }
        public int Stock { get; set; }
        public string Supplier { get; set; }
        public DateTime LastUpdate { get; set; }
    }

}
