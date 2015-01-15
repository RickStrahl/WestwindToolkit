using System;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoDB.Driver.Builders;
using MongoDB.Driver.Linq;
using Newtonsoft.Json;
using Westwind.Data.MongoDb;
using Westwind.Data.Test.Models;
using Westwind.Utilities;

namespace Westwind.BusinessFramework.Test.MongoDb
{
    [TestClass]
    public class MongoDbBusinessTests
    {
        public MongoDbBusinessBase<Customer, MongoDbContext> CreateBusiness()
        {
            return new MongoDbBusinessBase<Customer, MongoDbContext>(connectionString: "MongoTestContext");
        }


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
            mongoBus.Save(cust);
            
            // add random customers
            for (int i = 0; i < 250; i++)
            {
                cust = new Customer()
                {
                    FirstName = StringUtils.RandomString(20),
                    LastName = StringUtils.RandomString(20),
                    Company = StringUtils.RandomString(25),
                    Updated = DateTime.Now.AddDays(i * -1)
                };                
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


        [TestMethod]
        public void FindFromString()
        {
            var bus = CreateBusiness();
            var result = bus.FindFromString("{ FirstName: /^R.*/i }");

            Assert.IsNotNull(result);
            foreach (var cust in result)
            {
                Console.WriteLine(cust.FirstName);
            }
        }


        [TestMethod]
        public void FindFromStringJson()
        {
            var bus = CreateBusiness();
            var json = bus.FindFromStringJson("{ FirstName: /^R.*/i }");

            Assert.IsNotNull(json);
            Console.WriteLine(json);        
        }

        [TestMethod]
        public void SaveFromString()
        {
            var cust = new Customer()
            {
                FirstName = "Rick (JSON)",
                LastName = "Strahl",
                Company = "West Wind",
                Address = "32 Kaiea Place\r\nPaia, HI",
            };
            var json = JsonConvert.SerializeObject(cust);
            json = json.Replace("\"Id\":", "\"_id\":");

            var bus = CreateBusiness();
            var result = bus.SaveFromJson(json);

            Assert.IsNotNull(result);
            Console.WriteLine(result);
        }


    }
}
