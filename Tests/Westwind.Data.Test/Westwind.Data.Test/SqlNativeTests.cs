using System;
using System.Text;
using System.Collections.Generic;
using System.Data.Common;
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
            
            var t = context.Customers.FirstOrDefault();
            
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

        [TestMethod]
        public void SqlStoredProcedureToEntityList()
        {
            using (var context = new WebStoreContext())
            {
                IEnumerable<Customer> customers = context.Db.ExecuteStoredProcedureReader<Customer>("GetCustomers",
                    // named parameter requires CreateParameter
                    context.Db.CreateParameter("@cCompany","W%"));

                Assert.IsNotNull(customers, "Customers should not be null: " + context.Db.ErrorMessage);
                Assert.IsTrue(customers.Count() > 0, "Customer count should be greater than 0");
            }
        }

        [TestMethod]
        public void SqlStoredProcedureReader()
        {
            using (var context = new WebStoreContext())
            {
                DbDataReader reader = context.Db.ExecuteStoredProcedureReader("GetCustomers",
                    // named parameter requires CreateParameter
                    context.Db.CreateParameter("@cCompany", "W%"));

                Assert.IsNotNull(reader, "Reader should not be null: " + context.Db.ErrorMessage);
                Assert.IsTrue(reader.HasRows, "Reader should have rows");

                while (reader.Read())
                {
                    var company = reader["Company"] as string;
                    var entered = (DateTime) reader["Entered"];
                    Console.WriteLine(company + " " + entered.ToString("d"));
                }
            }
        }

        [TestMethod]
        public void SqlStoredProcedureNonQueryReturnValues()
        {
            using (var context = new WebStoreContext())
            {
                var countParm = context.Db.CreateParameter("@nCount", 0,
                    parameterDirection: System.Data.ParameterDirection.Output);

                var returnValueParm = context.Db.CreateParameter("@returnValue", 0,
                    parameterDirection: System.Data.ParameterDirection.ReturnValue);
                
                int result = context.Db.ExecuteStoredProcedureNonQuery("GetCustomerCount",
                    // named parameter requires CreateParameter
                    context.Db.CreateParameter("@cCompany", "W%"),
                    countParm, returnValueParm);

                Assert.IsFalse(result == -1, "result shouldn't be -1. " + context.Db.ErrorMessage);

                Console.WriteLine("Count value: " + countParm.Value);
                Console.WriteLine("Return Value: " + returnValueParm.Value);
            }
        }


    }
}
