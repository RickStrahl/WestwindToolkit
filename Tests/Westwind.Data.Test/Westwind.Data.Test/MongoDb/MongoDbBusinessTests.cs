using System;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoDB.Driver.Builders;
using MongoDB.Driver.Linq;
using Westwind.Data.MongoDb;
using Westwind.Data.Test.Models;

namespace Westwind.BusinessFramework.Test.MongoDb
{
    [TestClass]
    public class MongoDbBusinessTests
    {
        [TestMethod]
        public void Seed()
        {
            var mongoBus = CreateBusiness();

            mongoBus.Collection.RemoveAll();
            
            var cust = new Customer()
            {                
                FirstName = "Rick",
                LastName = "Strahl",
                Company = "West Wind",
                Address = "32 Kaiea Place\r\nPaia, HI",
            };
            cust.Id = cust.GetHashCode();
            mongoBus.Save(cust);

            
            // add 100 random customers
            for (int i = 0; i < 250; i++)
            {
                cust = new Customer()
                {
                    FirstName = RandomString(20),
                    LastName = RandomString(20),
                    Company = RandomString(25),
                    Updated = DateTime.Now.AddDays(i * -1)
                };
                cust.Id = cust.GetHashCode();
                Assert.IsTrue(mongoBus.Save(cust), mongoBus.ErrorMessage);
            }
        }

        [TestMethod]
        public void LoadByIdTest()
        {
            var bus = CreateBusiness();

            // find id (verbose)
            var cust = bus.Collection.AsQueryable().FirstOrDefault();
            int custId = cust.Id;
            cust = null;
           

            // actual test
            cust = bus.Load( custId );

            Assert.IsNotNull(cust);
            Assert.AreEqual(cust.Id, custId);
        }



        public MongoDbBusinessBase<Customer, MongoDbContext> CreateBusiness()
        {
            return new MongoDbBusinessBase<Customer, MongoDbContext>(connectionString: "MongoTestContext");
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
