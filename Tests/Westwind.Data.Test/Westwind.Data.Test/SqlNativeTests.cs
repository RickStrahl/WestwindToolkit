using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Westwind.Data.Test.Models;
using System.Diagnostics;
using System.Data.Entity;

namespace Westwind.Data.Test
{
    /// <summary>
    /// Summary description for UnitTest2
    /// </summary>
    [TestClass]
    public class SqlNativeTests
    {
        public SqlNativeTests()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        [ClassInitialize()]
        public static void MyClassInitialize(TestContext testContext) 
        {
            DatabaseInitializer.InitializeDatabase();

            // warm up connection            
            object id = new WebStoreContext()
                            .Db.ExecuteScalar("select id from customer where id=@0", 1);

        }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void6 MyTestCleanup() { }
        //
        #endregion

        [TestMethod]
        public void Find()
        {
            using (var context = new WebStoreContext())
            {
                var cust = context.Db.Find<Customer>(1, "Customers", "Id");
                Assert.IsNotNull(cust,context.Db.ErrorMessage);
                Assert.AreEqual(cust.LastName, "Strahl");
            }
        }

        [TestMethod]
        public void FindTest()
        {
            using (var context = new WebStoreContext())
            {
                var t = context.Db.Find<Customer>("select id,firstname,lastname from Customers where id = @0", 1);
                Console.WriteLine(t.Company);
                var watch = Stopwatch.StartNew();
                var custs = context.Db.Query<Customer>("select * from Customers where entered > @0", new DateTime(2000, 1, 1)).ToList();
                watch.Stop();
                Assert.IsNotNull(custs);
                Console.WriteLine(custs.Count);
                Console.WriteLine(watch.ElapsedMilliseconds);
            }
        }

        [TestMethod]
        public void ExecuteStoreQuery()
        {
            using (var context = new WebStoreContext())
            {
                var t = context.Database.SqlQuery<Customer>("select * from Customers where id = {0}", 1).FirstOrDefault();
                var watch = Stopwatch.StartNew();
                var custs = context.Database.SqlQuery<Customer>("select * from Customers where entered > {0}", new DateTime(2000, 1, 1));
                var custList = custs.ToList();
                watch.Stop();
                Assert.IsNotNull(custList);
                Console.WriteLine(custList.Count);
                Console.WriteLine(watch.ElapsedMilliseconds);
            }
        }

        [TestMethod]
        public void EntityLinqQuery()
        {
            var context = new WebStoreContext();

            DateTime newDateTime = new DateTime(1900, 1, 1);
            
            var t = context.Customers.Where(cust=> cust.Id == 1).FirstOrDefault();
            
            context.Dispose();

            context = new WebStoreContext();

            var watch = Stopwatch.StartNew();
            
            newDateTime = new DateTime(2000, 1, 1);
            
            var custList = context.Customers.Where(cust => cust.Entered > newDateTime).ToList();            

            watch.Stop();

            Assert.IsNotNull(custList);
            Console.WriteLine(custList.Count);
            Console.WriteLine(watch.ElapsedMilliseconds);
        }

    }
}
