using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace Westwind.Utilities.Configuration.Tests
{
    /// <summary>
    /// Tests implementation of the string provider.
    /// 
    /// String providers make it easy to read and write
    /// configuration values and store them in any source
    /// that can store strings. For example you can store
    /// the xml generated in a database field of your choice.
    /// (there is a database field provider that does that
    ///  however).
    /// 
    /// Use App.Configuration.Read(xmlString)
    /// and App.Configuration.WriteAsString() to read and
    /// write the XML configuration data for storage in 
    /// any non-existing format you like.
    /// 
    /// Light weight alternative to creating your own
    /// configuration provider (although it's easy to 
    /// create one).
    /// </summary>
    [TestClass]
    public class StringConfigurationTests
    {

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

        /// <summary>
        /// Write out configuration settings to string
        /// </summary>
        [TestMethod]
        public void WriteConfigurationTest()
        {
            var config = new StringConfiguration(null);

            config.MaxDisplayListItems = 12;
            config.DebugMode = DebugModes.DeveloperErrorMessage;
            config.ApplicationName = "Changed";
            config.SendAdminEmailConfirmations = true;

            // secure properties
            config.Password = "seekrit2";
            config.AppConnectionString = "server=.;database=unsecured";

            string xmlConfig = config.WriteAsString();

            Console.WriteLine(xmlConfig);

            Assert.IsTrue(xmlConfig.Contains(@"<DebugMode>DeveloperErrorMessage</DebugMode>"));
            Assert.IsTrue(xmlConfig.Contains(@"<MaxDisplayListItems>12</MaxDisplayListItems>"));
            Assert.IsTrue(xmlConfig.Contains(@"<SendAdminEmailConfirmations>true</SendAdminEmailConfirmations>"));

            // Password and AppSettings  should be encrypted in config file
            Assert.IsTrue(xmlConfig.Contains(@"<Password>ADoCNO6L1HIm8V7TyI4deg==</Password>"));
            Assert.IsTrue(xmlConfig.Contains(@"<AppConnectionString>z6+T5mzXbtJBEgWqpQNYbBss0csbtw2b/qdge7PUixE=</AppConnectionString>"));
        }

        /// <summary>
        /// Unlike other providers the string provider has 
        /// no 'automatic' read mode - it requires a string 
        /// as input to work.
        /// </summary>
        [TestMethod]
        public void ReadConfigurationFromStringTest()
        {
            string xmlConfig = @"<?xml version=""1.0"" encoding=""utf-8""?>
<StringConfiguration xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"">
   <ApplicationName>Changed</ApplicationName>
   <DebugMode>DeveloperErrorMessage</DebugMode>
   <MaxDisplayListItems>12</MaxDisplayListItems>
   <SendAdminEmailConfirmations>true</SendAdminEmailConfirmations>
   <Password>ADoCNO6L1HIm8V7TyI4deg==</Password>
   <AppConnectionString>z6+T5mzXbtJBEgWqpQNYbBss0csbtw2b/qdge7PUixE=</AppConnectionString>
</StringConfiguration>
";
            var config = new StringConfiguration(xmlConfig);
            //config.Read(xmlConfig);

            Assert.IsNotNull(config);
            Assert.IsFalse(string.IsNullOrEmpty(config.ApplicationName));
            Assert.IsTrue(config.MaxDisplayListItems == 12);
            Assert.IsTrue(config.Password == "seekrit2");                        
        }

        
        [TestMethod]
        public void WriteEncryptedConfigurationTest()
        {
            var config = new StringConfiguration(null);

            // write secure properties
            config.Password = "seekrit2";
            config.AppConnectionString = "server=.;database=unsecured";

            var xml = config.WriteAsString(); 
                        
            Console.WriteLine(xml);

            // Password and AppSettings  should be encrypted in config file
            Assert.IsTrue(xml.Contains(@"<Password>ADoCNO6L1HIm8V7TyI4deg==</Password>"));
            Assert.IsTrue(xml.Contains(@"<AppConnectionString>z6+T5mzXbtJBEgWqpQNYbBss0csbtw2b/qdge7PUixE=</AppConnectionString>"));

            // now re-read settings into a new object
            var config2 = new StringConfiguration(xml);
            //config2.Read(xml);  // you can also explicitly read
            
            // check secure properties
            Assert.IsTrue(config.Password == "seekrit2");
            Assert.IsTrue(config.AppConnectionString == "server=.;database=unsecured");
        }
    }
}