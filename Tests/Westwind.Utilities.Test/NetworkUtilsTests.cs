using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Westwind.Utilities;

namespace Westwind.Utilities.Tests
{
    /// <summary>
    /// Summary description for NetworkUtilsTests
    /// </summary>
    [TestClass]
    public class NetworkUtilsTests
    {
        public NetworkUtilsTests()
        {
            //
            // TODO: Add constructor logic here
            //
        }

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
        public void GetBaseDomain()
        {
            Assert.IsTrue(new Uri("http://www.west-wind.com").GetBaseDomain() == "west-wind.com");
            Assert.IsTrue(new Uri("http://127.0.0.1").GetBaseDomain() == "127.0.0.1");
            Assert.IsTrue(NetworkUtils.GetBaseDomain("localhost") == "localhost");
            Assert.IsTrue(NetworkUtils.GetBaseDomain("classifieds4.gorge.net") == "gorge.net");
            Assert.IsTrue(NetworkUtils.GetBaseDomain("classifieds5.gorge.net") == "gorge.net");
            Assert.IsTrue(NetworkUtils.GetBaseDomain(string.Empty) == string.Empty);
        }
    }
}
