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
using Westwind.Utilities.Test;

namespace Westwind.Utilities.Data.Tests
{
    /// <summary>
    /// Summary description for DataUtilsTests
    /// </summary>
    [TestClass]
    public class SqlDataAccessTests
    {
        private const string STR_ConnectionString = "WestwindToolkitSamples";
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
            DatabaseInitializer.InitializeDatabase();

            // warm up data connection
            SqlDataAccess data = new SqlDataAccess(STR_ConnectionString);
            var readr = data.ExecuteReader("select top 1 * from Customers");
            readr.Read();
            readr.Close();

            // warm up DLR load time
            dynamic ddata = data;
            string err = ddata.ErrorMessage;
            
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
            SqlDataAccess data = new SqlDataAccess(STR_ConnectionString);

            IDataReader reader = data.ExecuteReader("select top 1 * from ApplicationLog");

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
            SqlDataAccess data = new SqlDataAccess(STR_ConnectionString);
            
            var swatch = new Stopwatch();
            swatch.Start();
            
            var recs = data.ExecuteReader<WebLogEntry>("select * from ApplicationLog").ToList();
            
            swatch.Stop();

            Assert.IsNotNull(recs,"Null");
            Assert.IsTrue(recs.Count > 0,"Count < 1");
            Assert.IsTrue(recs[0].Entered > DateTime.MinValue);

            Console.WriteLine(swatch.ElapsedMilliseconds);
            Console.WriteLine(recs.Count);
        }

        [TestMethod]
        public void ExecuteDataReaderToIEnumerableTest()
        {
            SqlDataAccess data = new SqlDataAccess(STR_ConnectionString);

            var swatch = new Stopwatch();
            swatch.Start();

            var enumerable = data.ExecuteReader<WebLogEntry>("select * from ApplicationLog");

            var recs = new List<WebLogEntry>();
            foreach (var entry in enumerable)
            {
                recs.Add(entry);
            }

            

            swatch.Stop();

            Assert.IsNotNull(recs, "Null");
            Assert.IsTrue(recs.Count > 0, "Count < 1");
            Assert.IsTrue(recs[0].Entered > DateTime.MinValue);

            Console.WriteLine(swatch.ElapsedMilliseconds);
            Console.WriteLine(recs.Count);
        }


        [TestMethod]
        public void ExecuteDataReaderToListManualTest()
        {
            SqlDataAccess data = new SqlDataAccess(STR_ConnectionString);
        
            var swatch = new Stopwatch();
            swatch.Start();

            var entries = new List<WebLogEntry>();
            var reader = data.ExecuteReader("select * from ApplicationLog");
            
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

        [TestMethod]
        public void NewParametersReaderTest()
        {
            var data = new SqlDataAccess(STR_ConnectionString);


            var swatch = Stopwatch.StartNew();

            var reader = data.ExecuteReader( "select * from ApplicationLog where entered > @0 and entered < @1 order by Entered", DateTime.Now.AddYears(-115), DateTime.Now.AddYears(-1));

            Assert.IsNotNull(reader, data.ErrorMessage);

            int readerCount = 0;
            while(reader.Read())             
            {
                string Message = reader["Message"] as string;
                string Details = reader["Details"] as string;

                Console.WriteLine(((DateTime)reader["Entered"]));
                readerCount++;
            }
            swatch.Stop();
            Console.WriteLine(readerCount);
            Console.WriteLine(swatch.ElapsedMilliseconds + "ms");

        }

        [TestMethod]
        public void NewParametersTableTest()
        {
            var data = new SqlDataAccess(STR_ConnectionString);

            // warmup
            data.ExecuteScalar("select top1 id from ApplicationLog");

            //var cmd = data.CreateCommand("select * from ApplicationLog where entered > @0 and entered > @1",CommandType.Text, DateTime.Now.AddYears(-10), DateTime.Now.AddYears(-));
            //var table = data.ExecuteTable("TLogs", cmd);

            var swatch = Stopwatch.StartNew();

            var table = data.ExecuteTable("TLogs","select * from ApplicationLog where entered > @0 and entered < @1 order by Entered", DateTime.Now.AddYears(-115), DateTime.Now.AddYears(-1));

            Assert.IsNotNull(table, data.ErrorMessage);

            Console.WriteLine(table.Rows.Count);
            foreach (DataRow row in table.Rows)
            {
                Console.WriteLine( ((DateTime) row["Entered"]) );
            }
            swatch.Stop();
            Console.WriteLine(swatch.ElapsedMilliseconds + "ms");

        }

        [TestMethod]
        public void NewParametersExecuteEntityTest()
        {
            using (var data = new SqlDataAccess(STR_ConnectionString))
            {
                //var cmd = data.CreateCommand("select * from ApplicationLog where entered > @0 and entered > @1",CommandType.Text, DateTime.Now.AddYears(-10), DateTime.Now.AddYears(-));
                //var table = data.ExecuteTable("TLogs", cmd);
                var swatch = Stopwatch.StartNew();
                var entries = data.ExecuteReader<WebLogEntry>("select * from ApplicationLog where entered > @0 and entered < @1 order by Entered", DateTime.Now.AddYears(-115), DateTime.Now.AddYears(-1));
                var logEntries = entries.ToList();
                Assert.IsNotNull(logEntries, data.ErrorMessage);
                Console.WriteLine(logEntries.Count);
                foreach (var logEntry in logEntries)
                {
                    Console.WriteLine(logEntry.Entered);
                }
                swatch.Stop();
                Console.WriteLine(swatch.ElapsedMilliseconds + "ms");
            }
        }

        [TestMethod]
        public void NewParametersxecuteDynamicTest()
        {
            using (var data = new SqlDataAccess(STR_ConnectionString))
            {
                var swatch = Stopwatch.StartNew();
                var reader = data.ExecuteReader("select * from ApplicationLog where entered > @0 and entered < @1 order by Entered", DateTime.Now.AddYears(-115), DateTime.Now.AddYears(-1));
                dynamic dreader = new DynamicDataReader(reader);
                Assert.IsNotNull(reader, data.ErrorMessage);
                int readerCount = 0;
                while (reader.Read())
                {
                    DateTime date = (DateTime)dreader.Entered; // reader.Entered;
                    Console.WriteLine(date);
                    readerCount++;
                }
                swatch.Stop();
                Console.WriteLine(readerCount);
                Console.WriteLine(swatch.ElapsedMilliseconds + "ms");
            }
        }

        [TestMethod]
        public void FindByIdTest()
        {
            using (var data = new SqlDataAccess(STR_ConnectionString))
            {
                var entry = data.Find<WebLogEntry>(1, "ApplicationLog", "Id");
                Assert.IsNotNull(entry, data.ErrorMessage);
                Console.WriteLine(entry.Entered + " " + entry.Message);
                var entry2 = new WebLogEntry();
                data.GetEntity(entry2, "ApplicationLog", "Id", 1);
                Assert.IsNotNull(entry2);
                Assert.AreEqual(entry2.Message, entry.Message);
                Console.WriteLine(entry2.Entered + " " + entry2.Message);
            }    
        }


        [TestMethod]
        public void FindBySqlTest()
        {
            using (var data = new SqlDataAccess(STR_ConnectionString))
            {
                var entry = data.Find<WebLogEntry>("select * from ApplicationLog where id=@0", 1);
                Assert.IsNotNull(entry, data.ErrorMessage);
                Console.WriteLine(entry.Entered + " " + entry.Message);
            }
            
            
        }

        [TestMethod]
        public void QueryTest()
        {
            using (var data = new SqlDataAccess(STR_ConnectionString))
            {
                var swatch = Stopwatch.StartNew();
                var logEntries = data.Query<WebLogEntry>("select * from ApplicationLog where entered > @0 and entered < @1 order by Entered", DateTime.Now.AddYears(-115), DateTime.Now.AddYears(-1)).ToList();
                Assert.IsNotNull(logEntries, data.ErrorMessage);
                Console.WriteLine(logEntries.Count);
                foreach (var logEntry in logEntries)
                {
                    Console.WriteLine(logEntry.Entered);
                }
                swatch.Stop();
                Console.WriteLine(swatch.ElapsedMilliseconds + "ms");
            }
        }

    }
}
