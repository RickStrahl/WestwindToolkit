using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text.RegularExpressions;
using Westwind.Utilities;

namespace Westwind.Tools.Tests
{
    /// <summary>
    /// Summary description for StrExtractTest
    /// </summary>
    [TestClass]
    public class StrExtractTest
    {
        public StrExtractTest()
        {
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
        public void ExtractStringTest()
        {
            string sourceString = "<root><data>value</data></root>";
            string expected = "value";

            // Case sensitive
            string res = StringUtils.ExtractString(sourceString, "<data>", "</data>", true, false);
            Assert.AreEqual(expected, res, "Failed to extract string properly");

            // Case Insensitive
            res = StringUtils.ExtractString(sourceString, "<Data>", "</Data>", false, false);
            Assert.AreEqual(expected, res, "Failed to extract string properly with case insensitive values");

            // Missing end delimiter - should read until end of the string
            res = StringUtils.ExtractString(sourceString, "</Data>", "<Data>", false, true);
            expected = "</root>";
            Assert.AreEqual(expected, res, "Failed to extract string with missing end parameter");
        }

        [TestMethod]
        public void RegExExtractionTest()
        {
            string sourceString = "ScriptCompression.ahx?r=ww.jquery.js";
            Match match = Regex.Match(sourceString, @"r=(.*)(&|$)", RegexOptions.IgnoreCase);
            string res = match.Groups[1].Value;

            string expected = "ww.jquery.js";
            Assert.AreEqual(expected, res, "Failed to extract string with missing end parameter");
        }

        [TestMethod]
        public void ReplaceStringTest()
        {
        }
    }
}













