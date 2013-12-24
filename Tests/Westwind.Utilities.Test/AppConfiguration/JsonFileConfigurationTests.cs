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
    public class JsonFileConfigurationTests
    {
        /// <summary>
        /// Note: For Web Apps this should be a complete path.
        /// Here the filename references the current directory
        /// </summary>
        public const string STR_JSONCONFIGFILE = "JsonConfiguration.txt";


        [TestMethod]
        public void DefaultConstructorInstanceTest()
        {
            var config = new JsonFileConfiguration();            
            config.Initialize(configData: STR_JSONCONFIGFILE);
            
            Assert.IsNotNull(config);
            Assert.IsFalse(string.IsNullOrEmpty(config.ApplicationName));

            string text = File.ReadAllText(STR_JSONCONFIGFILE);
            Console.WriteLine(text);
        }

        [TestMethod]
        public void WriteConfigurationTest()
        {
            var config = new JsonFileConfiguration();
            config.Initialize(STR_JSONCONFIGFILE);

            config.MaxDisplayListItems = 12;
            config.DebugMode = DebugModes.DeveloperErrorMessage;
            config.ApplicationName = "Changed";
            config.SendAdminEmailConfirmations = true;

            // secure properties
            config.Password = "seekrit2";
            config.AppConnectionString = "server=.;database=unsecured";

            config.Write();

            string jsonConfig = File.ReadAllText(STR_JSONCONFIGFILE);
            Console.WriteLine(jsonConfig);

            Assert.IsTrue(jsonConfig.Contains(@"""DebugMode"": ""DeveloperErrorMessage"""));
            Assert.IsTrue(jsonConfig.Contains(@"""MaxDisplayListItems"": 12") );
            Assert.IsTrue(jsonConfig.Contains(@"""SendAdminEmailConfirmations"": true"));

            // Password and AppSettings  should be encrypted in config file
            Assert.IsTrue(jsonConfig.Contains(@"""Password"": ""ADoCNO6L1HIm8V7TyI4deg=="""));
            
        }

        [TestMethod]
        public void WriteEncryptedConfigurationTest()
        {
            var config = new JsonFileConfiguration();
            config.Initialize(STR_JSONCONFIGFILE);

            // write secure properties
            config.Password = "seekrit2";
            config.AppConnectionString = "server=.;database=unsecured";

            config.Write();

            string jsonConfig = File.ReadAllText(STR_JSONCONFIGFILE);
            Console.WriteLine(jsonConfig);

            // Password and AppSettings  should be encrypted in config file
            Assert.IsTrue(jsonConfig.Contains(@"""Password"": ""ADoCNO6L1HIm8V7TyI4deg=="""));            

            // now re-read settings into a new object
            var config2 = new JsonFileConfiguration();
            config2.Initialize(STR_JSONCONFIGFILE);

            // check secure properties
            Assert.IsTrue(config.Password == "seekrit2");
            Assert.IsTrue(config.AppConnectionString == "server=.;database=unsecured");
        }
    }
}