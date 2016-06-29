using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Westwind.Data.Test.Models;
using Westwind.Data.EfCodeFirst;
using System.Data.Entity;
using Westwind.Utilities;
using System.Linq;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace Westwind.Data.Test
{
    [TestClass]
    public class SingleEntityEfCodeFirstTests
    {

        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
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
        // public void MyTestCleanup() { }

        #region Initialization
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
        [ClassInitialize()]
        public static void Initialize(TestContext testContext)
        {
            DatabaseInitializer.InitializeDatabase();
        }
        #endregion

        [TestMethod]
        public void LoadEntityTest()
        {
            using (var custBus = new busCustomer())
            {
                Customer cust = null;
                try
                {
                    cust = custBus.Load(1);
                }
                catch (Exception ex)
                {

                    Console.WriteLine(ex.Message);
                }
                Assert.IsNotNull(cust,custBus.ErrorMessage);
                Console.WriteLine(cust.Company);
                Assert.IsTrue(cust.Company.StartsWith("West Wind"));
            }
        }


        [TestMethod]
        public void UpdateEntityTest()
        {
            string newCompanyName = "West Wind " + DateTime.Now;

            // load and update first
            using (var custBus = new busCustomer())
            {
                var cust = custBus.Load(1);
                Assert.IsNotNull(cust);

                cust.Company = newCompanyName;
                Assert.IsTrue(custBus.Save(), custBus.ErrorMessage);
            }

            // reload from disk via new context and check for validity
            using (var custBus = new busCustomer())
            {
                // reload entity
                var cust = custBus.Load(1);
                Assert.IsNotNull(cust);
                Assert.AreEqual(newCompanyName, cust.Company);
            }
        }

        [TestMethod]
        public void NewEntity1Test()
        {
            int custId;
            Customer cust = null;
            using (var custBus = new busCustomer())
            {
                cust = custBus.NewEntity();

                cust.FirstName = "Oscar";
                cust.LastName = "Grouch";
                cust.Company = "Groucho Inc.";
                cust.Entered = DateTime.Now;

                Assert.IsTrue(custBus.Save(), custBus.ErrorMessage);

                // cust.Id is updated after save operation
                custId = cust.Id;
            }

            // New bus object to ensure new context is used
            // and data is loaded from disk - normally not required.
            using (var custBus = new busCustomer())
            {
                // check to ensure record was created
                var cust2 = custBus.Load(custId);
                Assert.AreEqual(cust2.Company, cust.Company);

                // Remove the new record    
                Assert.IsTrue(custBus.Delete(custId), custBus.ErrorMessage);
            }
        }

        [TestMethod]
        public void NewEntityAlternateTest()
        {
            int custId;
            using (var custBus = new busCustomer())
            {
                var cust = new Customer()
                {
                    FirstName = "John",
                    LastName = "Farrow",
                    Company = "Faraway Travel",
                    LastOrder = DateTime.Now
                };

                // Attach cust to Context and fire
                // NewEntity hooks
                custBus.NewEntity(cust);

                Assert.IsTrue(custBus.Save(), custBus.ErrorMessage);

                // cust.Id is updated after save operation
                custId = cust.Id;

                // Use a new bus object/context to force
                // reload from disk - existing context loads from memory
                // which doesn't test properly
                using (var custBus2 = new busCustomer())
                {
                    // load and compare
                    var cust2 = custBus2.Load(custId);
                    Assert.AreEqual(cust2.Company, cust.Company);
                    // Remove the new record    
                    Assert.IsTrue(custBus2.Delete(custId), custBus2.ErrorMessage);
                }
                
            }
        }

        [TestMethod]
        public void NewEntityWithTransactionTest()
        {
            int custId;
            using (var custBus = new busCustomer())
            {
                var cust = new Customer()
                {
                    FirstName = "John",
                    LastName = "Farrow 2",
                    Company = "Faraway2 Travel",
                    LastOrder = DateTime.Now,
                    Address = "111 adsasdasdasd"
                };

                // Attach cust to Context and fire
                // NewEntity hooks
                custBus.NewEntity(cust);

                Assert.IsTrue(custBus.Save(useTransactionScope: true), custBus.ErrorMessage);

                // cust.Id is updated after save operation
                custId = cust.Id;

                // Use a new bus object/context to force
                // reload from disk - existing context loads from memory
                // which doesn't test properly
                using (var custBus2 = new busCustomer())
                {
                    // load and compare
                    var cust2 = custBus2.Load(custId);
                    Assert.AreEqual(cust2.Company, cust.Company);
                    // Remove the new record    
                    Assert.IsTrue(custBus2.Delete(custId), custBus2.ErrorMessage);
                }

            }
        }

        [TestMethod]
        public void AttachNewTest()
        {
            var cust = new Customer()
            {
                Company = "East Wind Technologies",
                FirstName = "Jimmy",
                LastName = "Ropain",
            };

            using (var custBus = new busCustomer())
            {
                custBus.Attach(cust, true);
                Assert.IsTrue(custBus.Save(), custBus.ErrorMessage);
            }

            int custId = cust.Id;

            // load new bus/context to force load from disk
            // otherwise load loads from cached context
            using (var custBus = new busCustomer())
            {
                Customer cust2 = custBus.Load(custId);
                Assert.IsNotNull(cust2, custBus.ErrorMessage);

                Assert.AreEqual(cust.Company, cust2.Company);

                Assert.IsTrue(custBus.Delete(custId), custBus.ErrorMessage);
            }

            Console.WriteLine(custId);

        }

        [TestMethod]
        public void AttachExistingTest()
        {            
            int custId = 0;
            Customer cust; 
            DateTime time = DateTime.Now;


            using (var custBo = new busCustomer())
            {
                // grab existing customer id we can update
                custId = custBo.Context.Customers
                        .Where(c => c.LastName == "Strahl")
                        .Select( c=> c.Id)
                        .First();

                cust = new Customer()
                {
                    Id = custId,
                    FirstName = "Ricky",
                    LastName = "Strahl",
                    Company = "West Wind",
                    Entered = time,
                    Address = "31 Kaiea Place"                    
                };

                custBo.Attach(cust);
                Assert.IsTrue(custBo.Save(), custBo.ErrorMessage);              
            }

            using (var custBo2 = new busCustomer())
            {
            
                // load new bus/context to force load from disk
                // otherwise load loads from cached context                        
                Customer cust2 = custBo2.Load(custId);
                Assert.IsNotNull(cust2, custBo2.ErrorMessage);

                Assert.IsTrue(cust2.Entered.ToString() == cust.Entered.ToString());                
            }
        }

        [TestMethod]
        public void TestExecuteNonQuery()
        {
            var custBo = new busCustomer(); 

            int count = custBo.ExecuteNonQuery("update customers set updated = getDate() where Id={0}", 3);

            Console.WriteLine(count);
            Console.WriteLine(custBo.ErrorMessage);
            
            Assert.IsTrue(count > -1);
        }

        [TestMethod]
        public void TestContextExecuteNonQuery()
        {
            var custBo = new busCustomer();

            int count = custBo.Context.Db.ExecuteNonQuery("update customers set updated = getDate() where Id=@0", 3);

            Console.WriteLine(count);
            Console.WriteLine(custBo.Context.Db.ErrorMessage);

            Assert.IsTrue(count > -1);
        }

        [TestMethod]
        public void AttachWithChildExistingTest()
        {
            var orderBo = new busOrder();
            var custBo = new busCustomer(orderBo); // share context

            // same as this to share context
            //custBo.Context = orderBo.Context;

            var order = orderBo.NewEntity();
            order.OrderId = StringUtils.NewStringId();

            // this is easiest and most efficient - if you have a PK
            //order.CustomerPk = 1;

            // manually load customer instance from context
            var cust = custBo.Load(1);
            cust.Updated = DateTime.Now; // make a change  
          
            // assign the customer
            order.Customer = cust;

            // add a new line item
            order.LineItems.Add(new LineItem()
                                {
                                    Description = "Cool new Item",
                                    Price = 40M,
                                    Sku = "COOLNEW",
                                    Quantity = 1,
                                    Total = 40m
                                });

            Assert.IsTrue(orderBo.Save(), orderBo.ErrorMessage);
        }

        [TestMethod]
        public void AttachWithChildExistingAndCustIdTest()
        {
            var orderBo = new busOrder();

            var order = orderBo.NewEntity();
            order.OrderId = StringUtils.NewStringId();

            // this is easiest and most efficient
            order.CustomerPk = 1;

            // add a new line item
            order.LineItems.Add(new LineItem()
            {
                Description = "Cool new Item",
                Price = 40M,
                Sku = "COOLNEW",
                Quantity = 1,
                Total = 40m
            });

            Assert.IsTrue(orderBo.Save(), orderBo.ErrorMessage);
        }

        [TestMethod]
        public void FailedValidationTest()
        {
            var customerBus = new busCustomer();

            var custExisting = customerBus.Load(1);

            var cust = new Customer()
            {
                // create dupe values which should fail validation
                FirstName = custExisting.FirstName,
                LastName = custExisting.LastName,
                Company = custExisting.Company
            };
            cust = customerBus.NewEntity(cust);

            Assert.IsFalse(customerBus.Validate());
            Assert.IsFalse(string.IsNullOrEmpty(customerBus.ErrorMessage));
            Console.WriteLine("Validation Failed (test passed): " + customerBus.ErrorMessage);
        }

        [TestMethod]
        public void FailedValidationAutoValidateTest()
        {
            var customerBus = new busCustomer()
            {
                // Validates on Save automatically
                AutoValidate = true
            };
           

            var custExisting = customerBus.Load(1);

            var cust = new Customer()
            {
                // create dupe values which should fail validation
                FirstName = custExisting.FirstName,
                LastName = custExisting.LastName,
                Company = custExisting.Company
            };
            cust = customerBus.NewEntity(cust);

            Assert.IsFalse(customerBus.Save());
            Assert.IsFalse(string.IsNullOrEmpty(customerBus.ErrorMessage));
            Console.WriteLine("Validation Failed (test passed): " + customerBus.ErrorMessage);
        }

        
        [TestMethod]
        public void ExceptionHandling()
        {
            var customerBus = new busCustomer()
            {
                // Validates on Save automatically
                ErrorHandlingMode = ErrorHandlingModes.ThrowExecptions
            };

            try
            {
                var custList = customerBus.ExecuteList<Customer>("select * from customerss where company like '%Wind%'");
            }
            catch (SqlException ex)
            {
                Console.WriteLine("Error correctly thrown: " + customerBus.ErrorMessage);                       
                return;
            }

            Assert.IsTrue(false, "Exception should have fired and code should not get here.");
        }

    }

}