using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Westwind.Utilities;
using System.Dynamic;
using System.Web.Script.Serialization;
using System.Collections;
using System.Diagnostics;
using Microsoft.CSharp.RuntimeBinder;
using Newtonsoft.Json;

namespace ExpandoTests
{

    /// <summary>
    /// Summary description for ExpandoTests
    /// </summary>
    [TestClass]
    public class ExpandoTests
    {
        public ExpandoTests()
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
            get { return testContextInstance; }
            set { testContextInstance = value; }
        }

        #region Additional test attributes

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
        //

        #endregion

        /// <summary>
        /// Summary method that demonstrates some
        /// of the basic behaviors.
        /// 
        /// More specific tests are provided below
        /// </summary>
        [TestMethod]
        public void ExandoBasicTests()
        {
            // Set standard properties
            var ex = new User()
            {
                Name = "Rick",
                Email = "rstrahl@whatsa.com",
                Active = true
            };
            
            // set dynamic properties that don't exist on type
            dynamic exd = ex;
            exd.Entered = DateTime.Now; 
            exd.Company = "West Wind";
            exd.Accesses = 10;

            // set dynamic properties as dictionary
            ex["Address"] = "32 Kaiea";
            ex["Email"] = "rick@west-wind.com";
            ex["TotalOrderAmounts"] = 51233.99M;

            // iterate over all properties dynamic and native
            foreach (var prop in ex.GetProperties(true))
            {
                Console.WriteLine(prop.Key + " " + prop.Value);
            }

            // you can access plain properties both as explicit or dynamic
            Assert.AreEqual(ex.Name, exd.Name, "Name doesn't match");

            // You can access dynamic properties either as dynamic or via IDictionary
            Assert.AreEqual(exd.Company, ex["Company"] as string, "Company doesn't match");
            Assert.AreEqual(exd.Address, ex["Address"] as string, "Name doesn't match");

            // You can access strong type properties via the collection as well (inefficient though)
            Assert.AreEqual(ex.Name, ex["Name"] as string);

            // dynamic can access everything
            Assert.AreEqual(ex.Name, exd.Name); // native property
            Assert.AreEqual(ex["TotalOrderAmounts"], exd.TotalOrderAmounts); // dictionary property
        }


        [TestMethod]
        public void AddAndReadDynamicPropertiesTest()
        {
            // strong typing first
            var ex = new User();
            ex.Name = "Rick";
            ex.Email = "rick@whatsa.com";

            // create dynamic and create new props
            dynamic exd = ex;
            
            string company = "West Wind";
            int count = 10;

            exd.entered = DateTime.Now; 
            exd.Company = company;
            exd.Accesses = count;

            Assert.AreEqual(exd.Company, company);
            Assert.AreEqual(exd.Accesses, count);
        }

        [TestMethod]
        public void AddAndReadDynamicIndexersTest()
        {
            var ex = new ExpandoInstance();
            ex.Name = "Rick";
            ex.Entered = DateTime.Now;

            string address = "32 Kaiea";

            ex["Address"] = address;
            ex["Contacted"] = true;

            dynamic exd = ex;

            Assert.AreEqual(exd.Address, ex["Address"]);
            Assert.AreEqual(exd.Contacted, true);
        }


        [TestMethod]
        public void PropertyAsIndexerTest()
        {
            // Set standard properties
            var ex = new ExpandoInstance();
            ex.Name = "Rick";
            ex.Entered = DateTime.Now;

            Assert.AreEqual(ex.Name, ex["Name"]);
            Assert.AreEqual(ex.Entered, ex["Entered"]);
        }

        [TestMethod]
        public void DynamicAccessToPropertyTest()
        {
            // Set standard properties
            var ex = new ExpandoInstance();
            ex.Name = "Rick";
            ex.Entered = DateTime.Now;

            // turn into dynamic
            dynamic exd = ex;

            // Dynamic can access 
            Assert.AreEqual(ex.Name, exd.Name);
            Assert.AreEqual(ex.Entered, exd.Entered);

        }

        [TestMethod]
        public void IterateOverDynamicPropertiesTest()
        {
            var ex = new ExpandoInstance();
            ex.Name = "Rick";
            ex.Entered = DateTime.Now;

            dynamic exd = ex;
            exd.Company = "West Wind";
            exd.Accesses = 10;

            // Dictionary pseudo implementation
            ex["Count"] = 10;
            ex["Type"] = "NEWAPP";

            // Dictionary Count - 2 dynamic props added
            Assert.IsTrue(ex.Properties.Count == 4);

            // iterate over all properties
            foreach (KeyValuePair<string, object> prop in exd.GetProperties(true))
            {
                Console.WriteLine(prop.Key + " " + prop.Value);
            }
        }

        [TestMethod]
        public void MixInObjectInstanceTest()
        {
            // Create expando an mix-in second objectInstanceTest
            var ex = new ExpandoInstance(new Address());
            ex.Name = "Rick";
            ex.Entered = DateTime.Now;

            // create dynamic
            dynamic exd = ex;

            // values should show Addresses initialized values (not null)
            Console.WriteLine(exd.FullAddress);
            Console.WriteLine(exd.Email);
            Console.WriteLine(exd.Phone);
        }

        [TestMethod]
        public void TwoWayJsonSerializeExpandoTyped()
        {
            // Set standard properties
            var ex = new User()
            {
                Name = "Rick",
                Email = "rstrahl@whatsa.com",
                Password = "Seekrit23",
                Active = true
            };

            // set dynamic properties
            dynamic exd = ex;
            exd.Entered = DateTime.Now;
            exd.Company = "West Wind";
            exd.Accesses = 10;

            // set dynamic properties as dictionary
            ex["Address"] = "32 Kaiea";
            ex["Email"] = "rick@west-wind.com";
            ex["TotalOrderAmounts"] = 51233.99M;

            // *** Should serialize both static properties dynamic properties
            var json = JsonConvert.SerializeObject(ex, Formatting.Indented);
            Console.WriteLine("*** Serialized Native object:");
            Console.WriteLine(json);

            Assert.IsTrue(json.Contains("Name")); // static
            Assert.IsTrue(json.Contains("Company")); // dynamic


            // *** Now deserialize the JSON back into object to 
            // *** check for two-way serialization
            var user2 = JsonConvert.DeserializeObject<User>(json);
            json = JsonConvert.SerializeObject(user2, Formatting.Indented);
            Console.WriteLine("*** De-Serialized User object:");
            Console.WriteLine(json);

            Assert.IsTrue(json.Contains("Name")); // static
            Assert.IsTrue(json.Contains("Company")); // dynamic
        }

#if SupportXmlSerialization
        [TestMethod]
        public void TwoWayXmlSerializeExpandoTyped()
        {
            // Set standard properties
            var ex = new User();
            ex.Name = "Rick";
            ex.Active = true;


            // set dynamic properties
            dynamic exd = ex;
            exd.Entered = DateTime.Now;
            exd.Company = "West Wind";
            exd.Accesses = 10;

            // set dynamic properties as dictionary
            ex["Address"] = "32 Kaiea";
            ex["Email"] = "rick@west-wind.com";
            ex["TotalOrderAmounts"] = 51233.99M;
            
            // Serialize creates both static and dynamic properties
            // dynamic properties are serialized as a 'collection'
            string xml;
            SerializationUtils.SerializeObject(exd, out xml);
            Console.WriteLine("*** Serialized Dynamic object:");
            Console.WriteLine(xml);

            Assert.IsTrue(xml.Contains("Name")); // static
            Assert.IsTrue(xml.Contains("Company")); // dynamic

            // Serialize
            var user2 = SerializationUtils.DeSerializeObject(xml,typeof(User));
            SerializationUtils.SerializeObject(exd, out xml);
            Console.WriteLine(xml);

            Assert.IsTrue(xml.Contains("Rick")); // static
            Assert.IsTrue(xml.Contains("West Wind")); // dynamic
        }
#endif


        [TestMethod]
        public void ExpandoObjectJsonTest()
        {
            dynamic ex = new ExpandoObject();
            ex.Name = "Rick";
            ex.Entered = DateTime.Now;

            string address = "32 Kaiea";

            ex.Address = address;
            ex.Contacted = true;

            ex.Count = 10;
            ex.Completed = DateTime.Now.AddHours(2);
            
            string json = JsonConvert.SerializeObject(ex,Formatting.Indented);
            Console.WriteLine(json);
        }

       

        [TestMethod]
        public void UserExampleTest()
        {
            var user = new User();

            // Set strongly typed properties
            user.Email = "rick@west-wind.com";
            user.Password = "nonya123";
            user.Name = "Rickochet";
            user.Active = true;

            // Now add dynamic properties
            dynamic duser = user;
            duser.Entered = DateTime.Now;
            duser.Accesses = 1;

            // you can also add dynamic props via indexer 
            user["NickName"] = "Wreck";
            duser["WebSite"] = "http://www.west-wind.com/weblog";

            // Access strong type through dynamic ref
            Assert.AreEqual(user.Name, duser.Name);

            // Access strong type through indexer 
            Assert.AreEqual(user.Password, user["Password"]);


            // access dyanmically added value through indexer
            Assert.AreEqual(duser.Entered, user["Entered"]);

            // access index added value through dynamic
            Assert.AreEqual(user["NickName"], duser.NickName);


            // loop through all properties dynamic AND strong type properties (true)
            foreach (var prop in user.GetProperties(true))
            {
                object val = prop.Value;
                if (val == null)
                    val = "null";

                Console.WriteLine(prop.Key + ": " + val.ToString());
            }
        }

        [TestMethod]
        public void ExpandoMixinTest()
        {
            // have Expando work on Addresses
            var user = new User(new Address());

            // cast to dynamicAccessToPropertyTest
            dynamic duser = user;

            // Set strongly typed properties
            duser.Email = "rick@west-wind.com";
            user.Password = "nonya123";

            // Set properties on address object
            duser.Address = "32 Kaiea";
            //duser.Phone = "808-123-2131";

            // set dynamic properties
            duser.NonExistantProperty = "This works too";

            // shows default value Address.Phone value
            Console.WriteLine(duser.Phone);
        }


        public class ObjWithProp : Expando
        {
            public string SomeProp { get; set; }
        }

        [TestMethod]
        public void GivenPropWhenSetWithIndexThenPropsValue()
        {
            //arrange
            ObjWithProp obj = new ObjWithProp();
            //act
            obj.SomeProp = "value1";
            obj["SomeProp"] = "value2";

            //assert
            Assert.AreEqual("value2", obj.SomeProp);

        }

        [TestMethod]
        [ExpectedException(typeof(RuntimeBinderException))]
        public void InvalidAssignmentErrorOnStaticProperty()
        {
            dynamic dynUser = new User();
            dynUser.Name = 100;  // RuntimeBinderException

            // this should never run
            var user = dynUser as User;
            user.Name = "Rick";
            Console.WriteLine(user.Name);                        
            Console.WriteLine(user["Name"]);
            

            Assert.Fail("Invalid Assignment should have thrown exception");
            //>> 100
        }

        [TestMethod]
        public void ExpandoFromDictionary()
        {
            var dict = new Dictionary<string, object>()
            {
                {"Name", "Rick"},
                {"Company", "West Wind"},
                {"Accesses", 2}
            };

            dynamic expando = new Expando(dict);

            Console.WriteLine(expando.Name);
            Console.WriteLine(expando.Company);
            Console.WriteLine(expando.Accesses);

            Assert.AreEqual(dict["Name"], expando.Name);
            Assert.AreEqual(dict["Company"], expando.Company);
            Assert.AreEqual(dict["Accesses"], expando.Accesses);
        }

    }
    
    public class ExpandoInstance : Expando
    {
        public string Name { get; set; }
        public DateTime Entered { get; set; }

        public ExpandoInstance()
        { }

        /// <summary>
        /// Allow passing in of an instance
        /// </summary>
        /// <param name="instance"></param>
        public ExpandoInstance(object instance)
            : base(instance)
        { }
    }

    public class Address
    {
        public string FullAddress { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }

        public Address()
        {
            FullAddress = "32 Kaiea";
            Phone = "808 132-3456";
            Email = "rick@whatsa.com";
        }
    }

    public class User : Expando
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public string Name { get; set; }
        public bool Active { get; set; }
        public DateTime? ExpiresOn { get; set; }

        public User() : base()
        {
        }

        // only required if you want to mix in seperate instance
        public User(object instance)
            : base(instance)
        {
        }
    }

}
