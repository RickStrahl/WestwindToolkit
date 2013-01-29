using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Westwind.Utilities;

namespace Westwind.Utilities.Tests
{
    /// <summary>
    /// Summary description for ObjectFactoryTests
    /// </summary>
    [TestClass]
    public class ObjectFactoryTests
    {
        public ObjectFactoryTests()
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
        public void GetUniqueObjectKeyTest()
        {
            var obj1 = ObjectFactory<Information>.GetUniqueObjectKey();
            var obj2 = ObjectFactory<Information>.GetUniqueObjectKey();

            var name = "Rick";
            var obj3 = ObjectFactory<Information>.GetUniqueObjectKey(name);
            var obj4 = ObjectFactory<Information>.GetUniqueObjectKey(name);

            TestContext.WriteLine(obj1 + Environment.NewLine +
            obj2 + Environment.NewLine +
            obj3 + Environment.NewLine +
            obj4 + Environment.NewLine );

            Assert.AreEqual(obj1, obj2);
            Assert.AreNotEqual(obj1, obj3);
            Assert.AreEqual(obj3, obj4);
        }

        [TestMethod]
        public void ThreadScopedObjectTest()
        {
            var inst1 = ObjectFactory<Information>.CreateThreadScopedObject();
            var inst2 = ObjectFactory<Information>.CreateThreadScopedObject();

            var inst3 = ObjectFactory<Information>.CreateThreadScopedObject("rick");
            var inst4 = ObjectFactory<Information>.CreateThreadScopedObject("rick");

            var inst5 = ObjectFactory<Information>.CreateThreadScopedObject("rick5");


            Assert.IsNotNull(inst1);
            Assert.AreEqual(inst1, inst2);

            Assert.IsNotNull(inst3);

            Assert.AreNotEqual(inst1, inst3);
            Assert.AreEqual(inst3, inst4);

            Assert.IsNotNull(inst5);
            Assert.AreNotEqual(inst3, inst5);
        }
    }

    class Information
    {
        public string Name { get; set; }
        public DateTime Entered { get; set; }

        public Information()
        {
        }
        public Information(string name)
        {
            Name = name;
        }
    }
}
