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
    public class DatabaseConfigurationTests
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
        [TestMethod]
        public void DefaultConstructorInstanceTest()
        {
            // this should create the database table and add default
            // data into it if it doesn't exist - otherwise
            // the values are read
            var config = new DatabaseConfiguration();
            config.Initialize("LocalDatabaseConnection","ConfigurationData");

            Assert.IsNotNull(config);
            Assert.IsFalse(string.IsNullOrEmpty(config.ApplicationName));                       
        }

        [TestMethod]
        public void WriteConfigurationTest()
        {
            var config = new DatabaseConfiguration();
            // connection string and table are provided in OnInitialize()
            config.Initialize();
            
            config.MaxDisplayListItems = 12;
            config.DebugMode = DebugModes.DeveloperErrorMessage;
            config.ApplicationName = "Changed";
            config.SendAdminEmailConfirmations = true;

            // encrypted properties
            config.Password = "seekrit2";
            config.AppConnectionString = "server=.;database=HRDatabase";

            Assert.IsTrue(config.Write(),"Write Failed: " + config.ErrorMessage);

            // create a new instance and read the values from the database
            var config2 = new DatabaseConfiguration();
            config2.Initialize(); 
            
            Assert.IsNotNull(config2);
            Assert.AreEqual(config.MaxDisplayListItems, config2.MaxDisplayListItems);
            Assert.AreEqual(config.DebugMode, config2.DebugMode);

            // Encrypted values
            Assert.AreEqual(config.Password, config2.Password);
            Assert.AreEqual(config.AppConnectionString, config2.AppConnectionString);

            // reset to default val
            config.MaxDisplayListItems = 15;
            config.Write();
        }
    }
}