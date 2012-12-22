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
    public class CustomConfigurationTests
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

        [TestMethod]
        public void DefaultConstructorInstanceTest()
        {
            var config = new CustomConfigFileConfiguration();
            config.Initialize();

            Assert.IsNotNull(config);
            Assert.IsFalse(string.IsNullOrEmpty(config.ApplicationName));
            
            string text = File.ReadAllText(TestHelpers.GetTestConfigFilePath());
            Assert.IsTrue(text.Contains(@"<add key=""MaxDisplayListItems"" value=""15"" />"));
            Console.WriteLine(text);          
        }

        [TestMethod]
        public void WriteConfigurationTest()
        {
            var config = new CustomConfigFileConfiguration();
            config.Initialize();
            
            config.MaxDisplayListItems = 12;
            config.DebugMode = DebugModes.DeveloperErrorMessage;
            config.ApplicationName = "Changed";
            config.SendAdminEmailConfirmations = true;

            // secure properties
            config.Password = "seekrit2";
            config.AppConnectionString = "server=.;database=unsecured";

            config.Write();
            
            string text = File.ReadAllText(TestHelpers.GetTestConfigFilePath());
            Console.WriteLine(text);

            Assert.IsTrue(text.Contains(@"<add key=""DebugMode"" value=""DeveloperErrorMessage"" />"));
            Assert.IsTrue(text.Contains(@"<add key=""MaxDisplayListItems"" value=""12"" />"));
            Assert.IsTrue(text.Contains(@"<add key=""SendAdminEmailConfirmations"" value=""True"" />"));

            // Password and AppSettings  should be encrypted in config file
            Assert.IsTrue(text.Contains(@"<add key=""Password"" value=""ADoCNO6L1HIm8V7TyI4deg=="" />"));
            Assert.IsTrue(text.Contains(@"<add key=""AppConnectionString"" value=""z6+T5mzXbtJBEgWqpQNYbBss0csbtw2b/qdge7PUixE="" />"));            
        }

        [TestMethod]
        public void WriteEncryptedConfigurationTest()
        {
            var config = new CustomConfigFileConfiguration();
            config.Initialize();

            // write secure properties
            config.Password = "seekrit2";
            config.AppConnectionString = "server=.;database=unsecured";

            config.Write();
            
            string text = File.ReadAllText(TestHelpers.GetTestConfigFilePath());
            Console.WriteLine(text);
            
            // Password and AppSettings  should be encrypted in config file
            Assert.IsTrue(text.Contains(@"<add key=""Password"" value=""ADoCNO6L1HIm8V7TyI4deg=="" />"));
            Assert.IsTrue(text.Contains(@"<add key=""AppConnectionString"" value=""z6+T5mzXbtJBEgWqpQNYbBss0csbtw2b/qdge7PUixE="" />"));
            
            // now re-read settings into a new object
            var config2 = new CustomConfigFileConfiguration();
            config2.Initialize();
            
            // check secure properties
            Assert.IsTrue(config.Password == "seekrit2");
            Assert.IsTrue(config.AppConnectionString == "server=.;database=unsecured");
        }
    }
}