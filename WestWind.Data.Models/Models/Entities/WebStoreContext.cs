using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using Westwind.Data.EfCodeFirst;
using Westwind.Data.EfCodeFirst;

namespace Westwind.Data.Test.Models
{
    public class WebStoreContext : EfCodeFirstContext  // use to get support for DbNative interface
    {
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<LineItem> LineItems { get; set; }        
    }

    public class WebStoreContextInitializer : DropCreateDatabaseIfModelChanges<WebStoreContext>
    {
        protected override void Seed(WebStoreContext context)
        {            
            try
            {
                var cust = new Customer()
                {
                    FirstName = "Rick",
                    LastName = "Strahl",
                    Company = "West Wind",
                    Address = "32 Kaiea Place\r\nPaia, HI",
                };
                context.Customers.Add(cust);

                int res = context.SaveChanges();

                var order = new Order()
                {
                    Customer = cust,
                    OrderId = "Order1",
                };

                var lItem = new LineItem()
                {
                    Sku = "WCONNECT",
                    Description = "West wind Web Connection",
                    Quantity = 1,
                    Price = 399.99M,
                    OrderId = order.Id
                };
                lItem.Total = lItem.Quantity * lItem.Price;
                order.LineItems.Add(lItem);

                lItem = new LineItem()
                {
                    Sku = "WWHELP",
                    Description = "West  Wind Html Help Builder",
                    Quantity = 1,
                    Price = 299.99M,
                    OrderId = order.Id
                };
                lItem.Total = lItem.Quantity * lItem.Price;
                order.LineItems.Add(lItem);

                context.Orders.Add(order);

                res = context.SaveChanges();

                // add 100 random customers
                for (int i = 0; i < 550; i++)
                {
                    cust = new Customer()
                    {
                        FirstName = RandomString(20),
                        LastName = RandomString(20),
                        Company = RandomString(25),
                        Updated = DateTime.Now.AddDays( i * -1)
                    };

                    order = new Order()
                    {
                        Entered = DateTime.Now.AddDays(i * -1),
                        OrderId = RandomString(10)
                    };
                    cust.Orders.Add(order);

                     lItem = new LineItem()
                    {
                        Sku = "WCONNECT",
                        Description = "West wind Web Connection",
                        Quantity = 1,
                        Price = 399.99M,
                        OrderId = order.Id
                    };
                    lItem.Total = lItem.Quantity * lItem.Price;
                    order.LineItems.Add(lItem);

                    lItem = new LineItem()
                    {
                        Sku = "WWHELP",
                        Description = "West  Wind Html Help Builder",
                        Quantity = 1,
                        Price = 299.99M,
                        OrderId = order.Id
                    };
                    lItem.Total = lItem.Quantity * lItem.Price;
                    order.LineItems.Add(lItem);

                    context.Customers.Add(cust);

                    context.SaveChanges();
                }




            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }            
            //base.Seed(context);
        }

        private static Random random = new Random((int)DateTime.Now.Ticks);//thanks to McAden

        private string RandomString(int size)
        {
            StringBuilder builder = new StringBuilder();
            char ch;
            for (int i = 0; i < size; i++)
            {
                ch = Convert.ToChar(Convert.ToInt32(Math.Floor(26 * random.NextDouble() + 65)));
                builder.Append(ch);
            }

            return builder.ToString();
        }
    }
}
