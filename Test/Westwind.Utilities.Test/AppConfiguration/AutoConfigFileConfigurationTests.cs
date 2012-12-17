using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Configuration;

namespace Westwind.Utilities.Configuration.Tests
{
    /// <summary>
    /// Tests default config file implementation that uses
    /// only base constructor behavior - (config file and section config only)    
    /// </summary>
    [TestClass]
    public class AutoConfigFileConfigurationTests
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
            var config = new AutoConfigFileConfiguration();
            config.Initialize();

            Assert.IsNotNull(config);
            Assert.IsFalse(string.IsNullOrEmpty(config.ApplicationName));
            Assert.AreEqual(config.MaxDisplayListItems, 15);

            string text = File.ReadAllText(TestHelpers.GetTestConfigFilePath());
            Console.WriteLine(text);
        }

        [TestMethod]
        public void AutoConfigWriteConfigurationTest()
        {            
            var config = new AutoConfigFileConfiguration();
            config.Initialize();

            Assert.IsNotNull(config);
            Assert.IsFalse(string.IsNullOrEmpty(config.ApplicationName));
            Assert.AreEqual(config.MaxDisplayListItems, 15);

            config.MaxDisplayListItems = 17;
            config.Write();

            var config2 = new AutoConfigFileConfiguration();
            config2.Initialize();

            Assert.AreEqual(config2.MaxDisplayListItems, 17);

            // reset to default val
            config2.MaxDisplayListItems = 15;
            config2.Write();
        }

        [TestMethod]
        public void WriteConfigurationTest()
        {

            var config = new AutoConfigFileConfiguration();
            config.Initialize();

            config.MaxDisplayListItems = 12;
            config.DebugMode = DebugModes.DeveloperErrorMessage;
            config.ApplicationName = "Changed";
            config.SendAdminEmailConfirmations = true;
            config.Write();

            string text = File.ReadAllText(TestHelpers.GetTestConfigFilePath());
            Console.WriteLine(text);

            Assert.IsTrue(text.Contains(@"<add key=""DebugMode"" value=""DeveloperErrorMessage"" />"));
            Assert.IsTrue(text.Contains(@"<add key=""MaxDisplayListItems"" value=""12"" />"));
            Assert.IsTrue(text.Contains(@"<add key=""SendAdminEmailConfirmations"" value=""True"" />"));

            var config2 = new AutoConfigFileConfiguration();
            config2.Initialize();

            Assert.AreEqual(config2.MaxDisplayListItems, 12);
            Assert.AreEqual(config2.ApplicationName, "Changed");

            // reset to default val
            config2.MaxDisplayListItems = 15;
            config2.Write();
        }


        /// <summary>
        /// Test without explicit constructor parameter 
        /// </summary>
        [TestMethod]
        public void DefaultConstructor2InstanceTest()
        {
            var config = new AutoConfigFile2Configuration();
            
            // Not required since custom constructor calls this
            //config.Initialize();

            Assert.IsNotNull(config);
            Assert.IsFalse(string.IsNullOrEmpty(config.ApplicationName));
            Assert.AreEqual(config.MaxDisplayListItems, 15);

            string text = File.ReadAllText(TestHelpers.GetTestConfigFilePath());
            Console.WriteLine(text);
        }

        /// <summary>
        /// Write test without explicit constructor
        /// </summary>
        [TestMethod]
        public void WriteConfiguration2Test()
        {
            var config = new AutoConfigFile2Configuration();
            
            //config.Initialize();

            config.MaxDisplayListItems = 12;
            config.DebugMode = DebugModes.DeveloperErrorMessage;
            config.ApplicationName = "Changed";
            config.SendAdminEmailConfirmations = true;
            config.Write();

            string text = File.ReadAllText(TestHelpers.GetTestConfigFilePath());
            Console.WriteLine(text);

            Assert.IsTrue(text.Contains(@"<add key=""DebugMode"" value=""DeveloperErrorMessage"" />"));
            Assert.IsTrue(text.Contains(@"<add key=""MaxDisplayListItems"" value=""12"" />"));
            Assert.IsTrue(text.Contains(@"<add key=""SendAdminEmailConfirmations"" value=""True"" />"));

            // reset to default val
            config.MaxDisplayListItems = 15;
            config.Write();
        }
    }
}