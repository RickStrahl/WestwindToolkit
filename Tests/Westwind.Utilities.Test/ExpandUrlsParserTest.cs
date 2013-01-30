using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Westwind.Utilities;

namespace Westwind.Utilities.Tests
{
    /// <summary>
    /// Summary description for UnitTest1
    /// </summary>
    [TestClass]
    public class ExpandUrlsParserTest
    {
        public ExpandUrlsParserTest()
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
        public void ParseExpandedLinks()
        {
            string text = "This is a test with an [embedded link|www.west-wind.com] that is formatted and one to www.west-wind.com that is not.";

            string parsed = UrlParser.ExpandUrls(text, null, true);

            TestContext.WriteLine("{0}", parsed);

            Assert.IsTrue(parsed.Contains("<a href='http://www.west-wind.com'>embedded link</a>"), "link parsing failed.");
            Assert.IsTrue(parsed.Contains("<a href='http://www.west-wind.com'>www.west-wind.com</a>"), "link parsing failed.");
        }

        [TestMethod]
        public void ParseOddExpandedLinks()
        {
            string text = "This is a test with an [embedded link|jquery.ui.com] that is formatted and one to http://skodia.name.com that is not.";

            string parsed = UrlParser.ExpandUrls(text, null, true);

            TestContext.WriteLine("{0}", parsed);

            Assert.IsTrue(parsed.Contains("<a href='http://jquery.ui.com'>embedded link</a>"), "link parsing failed.");
            Assert.IsTrue(parsed.Contains("<a href='http://skodia.name.com'>http://skodia.name.com</a>"), "link parsing failed.");
        }
    }
}
