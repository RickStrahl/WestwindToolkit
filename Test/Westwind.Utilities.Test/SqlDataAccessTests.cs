using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Westwind.Utilities.Data;
using System.Data;
using Westwind.Utilities;
using Westwind.Utilities.Logging;
using System.Diagnostics;

namespace Westwind.UtilitiesTests
{
    /// <summary>
    /// Summary description for DataUtilsTests
    /// </summary>
    [TestClass]
    public class SqlDataAccessTests
    {
        public SqlDataAccessTests()
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
        [ClassInitialize()]
        public static void Initialize(TestContext testContext) 
        {
            // warm up data connection
            SqlDataAccess data = new SqlDataAccess("TestData");
            var readr = data.ExecuteReader("select top 1 * from TestLogFile");
            readr.Read();
            readr.Close();
            
        }
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
        public void DataReaderToObjectTest()
        {
            SqlDataAccess data = new SqlDataAccess("TestData");

            IDataReader reader = data.ExecuteReader("select top 1 * from TestLogFile");

            Assert.IsNotNull(reader, "Couldn't access Data reader. " + data.ErrorMessage);

            Assert.IsTrue(reader.Read(), "Couldn't read from Data Reader");

            WebLogEntry entry = new WebLogEntry();

            DataUtils.DataReaderToObject(reader, entry, null);

            Assert.IsNotNull(entry.Message, "Entry Message should not be null");
            Assert.IsTrue(entry.ErrorLevel != ErrorLevels.None, "Entry Error level should not be None (error)");
        }

        [TestMethod]
        public void ExecuteDataReaderToListTest()
        {
            SqlDataAccess data = new SqlDataAccess("TestData");
            
            var swatch = new Stopwatch();
            swatch.Start();
            
            var recs = data.ExecuteReader<WebLogEntry>("select * from TestLogFile");
            
            swatch.Stop();

            Assert.IsNotNull(recs,"Null");
            Assert.IsTrue(recs.Count > 0,"Count < 1");
            Assert.IsTrue(recs[0].Entered > DateTime.MinValue);

            Console.WriteLine(swatch.ElapsedMilliseconds);
            Console.WriteLine(recs.Count);
        }

        [TestMethod]
        public void ExecuteDataReaderToListManualTest()
        {
            SqlDataAccess data = new SqlDataAccess("TestData");
        
            var swatch = new Stopwatch();
            swatch.Start();

            var entries = new List<WebLogEntry>();
            var reader = data.ExecuteReader("select * from TestLogFile");
            
            while (reader.Read())
            {
                WebLogEntry entry = new WebLogEntry();
                entry.Details = reader["Details"] as string;
                entry.Entered = (DateTime)reader["Entered"];
                entry.ErrorLevel = (ErrorLevels) reader["ErrorLevel"];                
                entry.Id = (int)reader["id"];
                entry.IpAddress = reader["IpAddress"] as string;
                entry.Message = reader["Message"] as string;
                entry.PostData = reader["PostData"] as string;
                entry.QueryString = reader["QueryString"] as string;
                entry.Referrer = reader["Referrer"] as string;
                entry.RequestDuration = (decimal)reader["RequestDuration"];                
                entry.Url = reader["Url"] as string;
                entry.UserAgent = reader["UserAgent"] as string;

                entries.Add(entry);                
            }
            reader.Close();

            swatch.Stop();

            Console.WriteLine(swatch.ElapsedMilliseconds);
            Console.WriteLine(entries.Count);            
        }

     
    }
}
