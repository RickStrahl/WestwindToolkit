using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace Westwind.Data.Test.Models
{
    public class LineItem
    {
        public int Id { get; set; }
        public int OrderId { get; set; }

        [MaxLength(30)]
        public string Sku { get; set; }
        [MaxLength(200)]
        public string Description { get; set; }
        public decimal Quantity { get; set; }
        public decimal Price { get; set; }

        public decimal Total { get; set; }
    }
}
