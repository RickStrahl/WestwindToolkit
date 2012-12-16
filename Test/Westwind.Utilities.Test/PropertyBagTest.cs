using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Westwind.Utilities;
using System.Collections;
using System.Xml.Serialization;
using System.IO;
using System.Runtime.Serialization;
using System.Web.Script.Serialization;


namespace Westwind.Tools.Tests
{
    /// <summary>
    /// Summary description for UnitTest1
    /// </summary>
    [TestClass]
    public class PropertyBagTest
    {
        public PropertyBagTest()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        private TestContext testContextInstance;

        /// <summary>
        /// Gets or sets the test context which provides
        /// information about and functionality for the current test run.
        /// </summary>
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



        [TestMethod]
        public void PropertyBagTwoWayObjectSerializationTest()
        {
            var bag = new PropertyBag();

            bag.Add("key", "Value");
            bag.Add("Key2", 100.10M);
            bag.Add("Key3", Guid.NewGuid());
            bag.Add("Key4", DateTime.Now);
            bag.Add("Key5", true);
            bag.Add("Key7", new byte[3] { 42, 45, 66 } );
            bag.Add("Key8", null);
            bag.Add("Key9", new ComplexObject() { Name = "Rick",
            Entered = DateTime.Now,
            Count = 10 });

            string xml = bag.ToXml();

            TestContext.WriteLine(bag.ToXml());

            bag.Clear();

            bag.FromXml(xml);

            Assert.IsTrue(bag["key"] as string == "Value");
            Assert.IsInstanceOfType( bag["Key3"], typeof(Guid));
            Assert.IsNull(bag["Key8"]);
            //Assert.IsNull(bag["Key10"]);

            Assert.IsInstanceOfType(bag["Key9"], typeof(ComplexObject));
        }

        [TestMethod]
        public void JavaScriptSerializerTest()
        {
            var bag = new Dictionary<string, object>();

            bag.Add("key", "Value");
            bag.Add("Key2", 100.10M);
            bag.Add("Key3", Guid.NewGuid());
            bag.Add("Key4", DateTime.Now);
            bag.Add("Key5", true);
            bag.Add("Key7", new byte[3] { 42, 45, 66 });
            bag.Add("Key8", null);
            bag.Add("Key9", new ComplexObject() { Name = "Rick",
            Entered = DateTime.Now,
            Count = 10 });

            JavaScriptSerializer ser = new JavaScriptSerializer();
            var json = ser.Serialize(bag);

            //TestContext.WriteLine(json);

            var bag2 = ser.Deserialize<Dictionary<string, object>>(json);
            Assert.IsNotNull(bag2);

            // the following two tests should fail as the types don't match
            Assert.IsNotInstanceOfType(bag2["Key3"], typeof(Guid), "Value isn't a GUID"); // string
            Assert.IsNotInstanceOfType(bag2["Key9"], typeof(ComplexObject), "Value isn't a ComplexObject"); // Dictionary<string,object>
        }

        [TestMethod]
        public void PropertyBagInContainerTwoWayObjectSerializationTest()
        {
            var bag = new PropertyBag();

            bag.Add("key", "Value");
            bag.Add("Key2", 100.10M);
            bag.Add("Key3", Guid.NewGuid());
            bag.Add("Key4", DateTime.Now);
            bag.Add("Key5", true);
            bag.Add("Key7", new byte[3] { 42, 45, 66 });
            bag.Add("Key8", null);
            bag.Add("Key9", new ComplexObject() { Name = "Rick",
            Entered = DateTime.Now,
            Count = 10 });

            ContainerObject cont = new ContainerObject();
            cont.Name = "Rick";
            cont.Items = bag;


            string xml = SerializationUtils.SerializeObjectToString(cont);

            TestContext.WriteLine(xml);

            ContainerObject cont2 = SerializationUtils.DeSerializeObject(xml,
            typeof(ContainerObject)) as ContainerObject;

            Assert.IsTrue(cont2.Items["key"] as string == "Value");
            Assert.IsTrue(cont2.Items["Key3"].GetType() == typeof(Guid));

            Assert.IsNull(cont2.Items["Key8"]);

            //Assert.IsNull(bag["Key10"]);

            TestContext.WriteLine(cont.Items["Key3"].ToString());
            TestContext.WriteLine(cont.Items["Key4"].ToString());
        }


        [TestMethod]
        public void PropertyBagTwoWayValueTypeSerializationTest()
        {
            var bag = new PropertyBag<decimal>();

            bag.Add("key", 10M);
            bag.Add("Key1", 100.10M);
            bag.Add("Key2", 200.10M);
            bag.Add("Key3", 300.10M);
            string xml = bag.ToXml();

            TestContext.WriteLine(bag.ToXml());

            bag.Clear();

            bag.FromXml(xml);

            Assert.IsTrue(bag["Key1"] == 100.10M);
            Assert.IsTrue(bag["Key3"] == 300.10M);
        }


        #region StandardSerializerTests

        [TestMethod]
        [ExpectedException(typeof(NotSupportedException))]
        public void DictionaryXmlSerializerTest()
        {
            var bag = new Dictionary<string, object>();

            bag.Add("key", "Value");
            bag.Add("Key2", 100.10M);
            bag.Add("Key3", Guid.NewGuid());
            bag.Add("Key4", DateTime.Now);
            bag.Add("Key5", true);
            bag.Add("Key7", new byte[3] { 42, 45, 66 });

            // this should fail with NotSupported as Dictionaries 
            // can't be serialized
            TestContext.WriteLine(this.ToXml(bag));
        }

        string ToXml(object obj)
        {
            if (obj == null)
                return null;

            StringWriter sw = new StringWriter();
            XmlSerializer ser = new XmlSerializer(obj.GetType());
            ser.Serialize(sw, obj);
            return sw.ToString();
        }


        #endregion
        public class ContainerObject
        {
            public string Name { get; set; }
            public PropertyBag Items { get; set; }
        }

        public class ComplexObject
        {
            public string Name { get; set; }
            public DateTime Entered { get; set; }
            public int Count { get; set; }
        }
    }
}
