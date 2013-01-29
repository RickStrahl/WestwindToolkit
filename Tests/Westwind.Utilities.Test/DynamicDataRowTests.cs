using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Data;
using Westwind.Utilities;
using Microsoft.CSharp.RuntimeBinder;
using Westwind.Utilities.Data;

namespace Westwind.Utilities.Data.Tests
{
    /// <summary>
    /// Summary description for DynamicDataRowTests
    /// </summary>
    [TestClass]
    public class DynamicDataRowTests
    {
        public DynamicDataRowTests()
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
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
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
        [ExpectedException(typeof(RuntimeBinderException))]
        public void BasicDataRowTests()
        {
            DataTable table = new DataTable("table");
            table.Columns.Add( new DataColumn() { ColumnName = "Name", DataType = typeof(string) });
            table.Columns.Add( new DataColumn() { ColumnName = "Entered", DataType = typeof(DateTime) });
            table.Columns.Add(new DataColumn() { ColumnName = "NullValue", DataType = typeof(string) });

            DataRow row = table.NewRow();

            DateTime now = DateTime.Now;

            row["Name"] = "Rick";
            row["Entered"] = now;
            row["NullValue"] = null; // converted in DbNull

            dynamic drow = new DynamicDataRow(row);

            string name = drow.Name;
            DateTime entered = drow.Entered;
            string nulled = drow.NullValue;

            Assert.AreEqual(name, "Rick");
            Assert.AreEqual(entered, now);
            Assert.IsNull(nulled);
            // this should throw a RuntimeBinderException
            Assert.AreEqual(entered, drow.enteredd);
        }

        [TestMethod]
        public void DynamicDataTableTest()
        {
            DataTable table = new DataTable("table");
            table.Columns.Add(new DataColumn() { ColumnName = "Name", DataType = typeof(string) });
            table.Columns.Add(new DataColumn() { ColumnName = "Entered", DataType = typeof(DateTime) });
            table.Columns.Add(new DataColumn() { ColumnName = "NullValue", DataType = typeof(string) });

            DataRow row = table.NewRow();

            DateTime now = DateTime.Now;

            row["Name"] = "Rick";
            row["Entered"] = now;
            row["NullValue"] = null; // converted in DbNull

            table.Rows.Add(row);

            row = table.NewRow();
            row["Name"] = "Don";
            row["Entered"] = now.AddDays(1);
            row["NullValue"] = "Not Null"; // converted in DbNull
            table.Rows.Add(row);

            foreach (dynamic drow in table.DynamicRows())
            {
                Console.WriteLine(drow.Name + " " + drow.Entered.ToString("d") + " " + drow.NullValue);
            }

            dynamic drow2 = table.DynamicRows()[1];
            Console.WriteLine(drow2.Name + " " + drow2.Entered.ToString("d") + " " + drow2.NullValue);
            
        }
    }
}
