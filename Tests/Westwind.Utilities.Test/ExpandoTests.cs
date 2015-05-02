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
using Newtonsoft.Json;


namespace Westwind.Utilities.Tests
{
    /// <summary>
    /// Summary description for UnitTest1
    /// </summary>
    [TestClass]
    public class ExpandoTests
    {
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
        public void ExpandoToAndFromXml()
        {            
            dynamic properties = new Expando();            
            properties.Item1 = "Item 1 text";
            properties.Item2 = "Item 2 text";
            properties.Number3 = 10.22;


            string xml = properties.Properties.ToXml();

            Assert.IsNotNull(xml);
            Console.WriteLine(xml);

            dynamic properties2 = new Expando();
            Expando expando = properties2 as Expando;

            properties2.Properties.FromXml(xml);
            
            Assert.IsTrue(properties2.Item1 == properties.Item1);
            Assert.IsTrue(properties2.Number3 == properties.Number3);

            Console.WriteLine(properties2.Item1 + " " + 
                              properties2.Number3 + " " + 
                              properties["Item2"]);
            
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

        public User()
            : base()
        {
        }

        // only required if you want to mix in seperate instance
        public User(object instance)
            : base(instance)
        {
        }
    }

}
