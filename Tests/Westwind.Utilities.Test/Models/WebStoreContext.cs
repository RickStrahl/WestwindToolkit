using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using Westwind.Utilities.Logging;


namespace Westwind.Utilities.Test
{
    public class WebStoreContext : DbContext
    {
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<LineItem> LineItems { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
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
                        FirstName = StringUtils.RandomString(20),
                        LastName = StringUtils.RandomString(20),
                        Company = StringUtils.RandomString(25),
                        Updated = DateTime.Now.AddDays( i * -1)
                    };

                    order = new Order()
                    {
                        Entered = DateTime.Now.AddDays(i * -1),
                        OrderId = StringUtils.RandomString(10)
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

                LogManager manager = LogManager.Create();                

                for (int i = 0; i < 250; i++)
                {
                    var entry = new WebLogEntry()
                    {
                        Entered = DateTime.Now.AddDays(i * -1),
                        ErrorLevel = ErrorLevels.Log,
                        Message = StringUtils.RandomString(50, true),
                        Details = StringUtils.RandomString(60, true),
                        QueryString = StringUtils.RandomString(20, true),
                        ErrorType = (i % 2 == 0 ? "Log" : "Error"),
                        IpAddress = StringUtils.RandomString(12),
                        RequestDuration = i * 1.10M

                    };
                    manager.WriteEntry(entry);
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }                        
        }

    }
}
