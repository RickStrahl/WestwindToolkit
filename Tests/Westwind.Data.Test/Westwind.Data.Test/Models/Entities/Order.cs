using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;

namespace Westwind.Data.Test.Models
{
    public class Order
    {
        public int Id { get; set; }
        
        [ForeignKey("Customer")]
        public int CustomerPk { get; set; }

        public string OrderId { get; set; }
        public DateTime Entered { get; set; }
        public DateTime? Shipped { get; set; }

        public List<LineItem> LineItems { get; set; }
        public Customer Customer { get; set; }

        public Order()
        {
            Entered = DateTime.Now;
            Shipped = null;
            LineItems = new List<LineItem>();
        }

    }
}
