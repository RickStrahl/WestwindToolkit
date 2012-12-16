using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace Westwind.Utilities.Configuration.Tests
{
    /// <summary>
    /// Tests default config file implementation that uses
    /// only base constructor behavior - (config file and section config only)    
    /// </summary>
    [TestClass]
    public class XmlConfigurationTests
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
        /// Note: For Web Apps this should be a complete path.
        /// Here the filename references the current directory
        /// </summary>
        public const string STR_XMLCONFIGFILE = "XmlConfiguration.xml";

        [TestMethod]
        public void DefaultConstructorInstanceTest()
        {
            var config = new XmlFileConfiguration(STR_XMLCONFIGFILE);

            Assert.IsNotNull(config);
            Assert.IsFalse(string.IsNullOrEmpty(config.ApplicationName));
            
            string text = File.ReadAllText(STR_XMLCONFIGFILE);
            Console.WriteLine(text);          
        }

        [TestMethod]
        public void WriteConfigurationTest()
        {
            var config = new XmlFileConfiguration(STR_XMLCONFIGFILE);
            
            config.MaxDisplayListItems = 12;
            config.DebugMode = DebugModes.DeveloperErrorMessage;
            config.ApplicationName = "Changed";
            config.SendAdminEmailConfirmations = true;

            // secure properties
            config.Password = "seekrit2";
            config.AppConnectionString = "server=.;database=unsecured";

            config.Write();
            
            string xmlConfig = File.ReadAllText(STR_XMLCONFIGFILE);
            Console.WriteLine(xmlConfig);

            Assert.IsTrue(xmlConfig.Contains(@"<DebugMode>DeveloperErrorMessage</DebugMode>"));
            Assert.IsTrue(xmlConfig.Contains(@"<MaxDisplayListItems>12</MaxDisplayListItems>"));
            Assert.IsTrue(xmlConfig.Contains(@"<SendAdminEmailConfirmations>true</SendAdminEmailConfirmations>"));

            // Password and AppSettings  should be encrypted in config file
            Assert.IsTrue(xmlConfig.Contains(@"<Password>ADoCNO6L1HIm8V7TyI4deg==</Password>"));
            Assert.IsTrue(xmlConfig.Contains(@"<AppConnectionString>z6+T5mzXbtJBEgWqpQNYbBss0csbtw2b/qdge7PUixE=</AppConnectionString>"));
        }

        [TestMethod]
        public void WriteEncryptedConfigurationTest()
        {
            var config = new XmlFileConfiguration(STR_XMLCONFIGFILE);

            // write secure properties
            config.Password = "seekrit2";
            config.AppConnectionString = "server=.;database=unsecured";

            config.Write();
            
            string xmlConfig = File.ReadAllText(STR_XMLCONFIGFILE);
            Console.WriteLine(xmlConfig);

            // Password and AppSettings  should be encrypted in config file
            Assert.IsTrue(xmlConfig.Contains(@"<Password>ADoCNO6L1HIm8V7TyI4deg==</Password>"));
            Assert.IsTrue(xmlConfig.Contains(@"<AppConnectionString>z6+T5mzXbtJBEgWqpQNYbBss0csbtw2b/qdge7PUixE=</AppConnectionString>"));
            
            // now re-read settings into a new object
            var config2 = new XmlFileConfiguration(STR_XMLCONFIGFILE);
            
            // check secure properties
            Assert.IsTrue(config.Password == "seekrit2");
            Assert.IsTrue(config.AppConnectionString == "server=.;database=unsecured");
        }
    }
}