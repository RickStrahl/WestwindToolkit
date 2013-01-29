using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace Westwind.Utilities.Test
{
    public class Customer
    {
        public int Id { get; set; }

        [MaxLength(50)]
        public string FirstName { get; set; }
        
        [MaxLength(50)]
        public string LastName { get; set; }
        
        [MaxLength(50)]
        public string Company { get; set; }
        
        [MaxLength(768)]
        public string Address { get; set; }

        [MaxLength(30)]
        public string Password { get; set; }

        public DateTime? LastOrder { get; set; }

        public DateTime Entered { get; set; }
        public DateTime Updated { get; set; }

        public List<Order> Orders { get; set; }

        public Customer()
        {
            Entered = DateTime.Now;
            Updated = DateTime.Now;
            Orders = new List<Order>();
        }

    }

}
