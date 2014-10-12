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
            File.Delete(TestHelpers.GetTestConfigFilePath());

            var config = new CustomConfigFileConfiguration();
            config.Initialize();
            
            config.MaxDisplayListItems = 12;
            config.DebugMode = DebugModes.DeveloperErrorMessage;
            config.ApplicationName = "Changed";
            config.SendAdminEmailConfirmations = true;

            // secure properties
            config.Password = "seekrit2";
            config.AppConnectionString = "server=.;database=unsecured";

            // Complex Types
            config.License.Company = "Updated Company";
            config.ServerList[0] = "UpdatedServerName";

            config.License.Name = "Rick";
            config.License.Company = "West Wind 2";
            config.License.LicenseKey = "RickWestWind2-51231223";

            config.Write();

            config = null;
            config = new CustomConfigFileConfiguration();
            config.Initialize();

            Console.WriteLine(config.License.LicenseKey);
            Assert.IsTrue(config.License.LicenseKey == "RickWestWind2-51231223");
            
            
            string text = File.ReadAllText(TestHelpers.GetTestConfigFilePath());
            Console.WriteLine(text);

            Assert.IsTrue(text.Contains(@"<add key=""DebugMode"" value=""DeveloperErrorMessage"" />"));
            Assert.IsTrue(text.Contains(@"<add key=""MaxDisplayListItems"" value=""12"" />"));
            Assert.IsTrue(text.Contains(@"<add key=""SendAdminEmailConfirmations"" value=""True"" />"));

            // Password and AppSettings  should be encrypted in config file
            Assert.IsTrue(text.Contains(@"<add key=""Password"" value=""ADoCNO6L1HIm8V7TyI4deg=="" />"));
            Assert.IsTrue(text.Contains(@"<add key=""AppConnectionString"" value=""z6+T5mzXbtJBEgWqpQNYbBss0csbtw2b/qdge7PUixE="" />"));

            // Complex Value
            Assert.IsTrue(text.Contains(@"West Wind 2"));

            // List values
            Assert.IsTrue(text.Contains(@"<add key=""ServerList1"""));
            Assert.IsTrue(text.Contains(@"UpdatedServerName"));



        }

        [TestMethod]
        public void WriteAndReadConfigurationTest()
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

            config.License.Company = "Updated Company";
            config.ServerList[0] = "UpdatedServerName";

            config.Write();

            string text = File.ReadAllText(TestHelpers.GetTestConfigFilePath());
            Console.WriteLine(text);

            Assert.IsTrue(text.Contains(@"<add key=""DebugMode"" value=""DeveloperErrorMessage"" />"));
            Assert.IsTrue(text.Contains(@"<add key=""MaxDisplayListItems"" value=""12"" />"));
            Assert.IsTrue(text.Contains(@"<add key=""SendAdminEmailConfirmations"" value=""True"" />"));

            // Password and AppSettings  should be encrypted in config file
            Assert.IsTrue(text.Contains(@"<add key=""Password"" value=""ADoCNO6L1HIm8V7TyI4deg=="" />"));
            Assert.IsTrue(text.Contains(@"<add key=""AppConnectionString"" value=""z6+T5mzXbtJBEgWqpQNYbBss0csbtw2b/qdge7PUixE="" />"));

            // Complex Value
            Assert.IsTrue(text.Contains(@"Updated Company"));

            // List values
            Assert.IsTrue(text.Contains(@"<add key=""ServerList1"""));
            Assert.IsTrue(text.Contains(@"UpdatedServerName"));

            
            var config2 = new CustomConfigFileConfiguration();
            config2.Initialize();
            config2.Read();

            Assert.IsTrue(config2.License.Company == "Updated Company");
            Assert.IsTrue(config2.ServerList[0] == "UpdatedServerName");

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