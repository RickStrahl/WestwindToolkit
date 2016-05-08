using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Westwind.Utilities;

namespace Westwind.Utilities.Tests
{
    /// <summary>
    /// Summary description for StringUtilsTests
    /// </summary>
    [TestClass]
    public class StringUtilsTests
    {
        public StringUtilsTests()
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
        public void ToCamelCaseTest()
        {
            string original = "This is a test";
            string expected = "ThisIsATest";
            string actual = StringUtils.ToCamelCase(original);
            Assert.AreEqual(expected, actual, "Failed Simple Test");

            original = null;
            expected = "";
            actual = StringUtils.ToCamelCase(original);
            Assert.AreEqual(expected, actual, "Failed Null Test");

            original = "Pronto 123";
            expected = "Pronto123";
            actual = StringUtils.ToCamelCase(original);
            Assert.AreEqual(expected, actual, "Failed Embedded Numbers Test");

            original = "None";
            expected = "None";
            actual = StringUtils.ToCamelCase(original);
            Assert.AreEqual(expected, actual, "Failed Null Test");
        }

        [TestMethod]
        public void FromCamelCaseTest()
        {
            string original = "NoProblem";
            string expected = "No Problem";
            string actual = StringUtils.FromCamelCase(original);
            Assert.AreEqual(expected, actual, "Failed Simple Test");

            expected = "not hit";
            original = null;
            actual = expected;

            try
            {
                // null should throw ArgumentException
                actual = StringUtils.FromCamelCase(original);
            }
            catch (Exception ex)
            {
                Assert.IsTrue(ex is ArgumentException, "Failed Null Test");
            }
            Assert.AreEqual(expected, actual, "Failed null test - exception should have been thrown.");

            expected = "Pronto 123";
            original = "Pronto123";
            actual = StringUtils.FromCamelCase(original);
            Assert.AreEqual(expected, actual, "Failed Embedded Numbers Test");

            expected = "None";
            original = "None";
            actual = StringUtils.FromCamelCase(original);
            Assert.AreEqual(expected, actual, "Failed No CamelCase Test");
        }

        [TestMethod]
        public void NormalizeIndentationTest()
        {
            string code = @"
            try
            {
                // null should throw ArgumentException
                actual = StringUtils.FromCamelCase(original);
            }";

            string result = StringUtils.NormalizeIndentation(code).Trim();

            Assert.IsTrue(result.Substring(0, 3) == "try", "Not indented");

            code = @"
		************************************************************************
		FUNCTION IsWinnt
		*****************
		***      Pass: llReturnVersionNumber
		***    Return: .t. or .f.   or Version Number or -1 if not NT
		*************************************************************************
		LPARAMETER llReturnVersionNumber

		loAPI=CREATE(""wwAPI"")
		lcVersion = loAPI.ReadRegistryString(HKEY_LOCAL_MACHINE,;
		           ""SOFTWARE\Microsoft\Windows NT\CurrentVersion"",;
		           ""CurrentVersion"")
		                          
		IF !llReturnVersionNumber
		  IF ISNULL(lcVersion)
		     RETURN .F.
		  ELSE
		     RETURN .T.
		  ENDIF
		ENDIF";

            result = StringUtils.NormalizeIndentation(code).Trim();

            Assert.IsTrue(result.Substring(0, 3) == "***", "Not indented");
        }

        [TestMethod]
        public void Base36EncodeTest()
        {
            // positive number
            long inputNumber = 512344131333132;
            long result = 0;
            string base36 = "";

            base36 = StringUtils.Base36Encode(inputNumber);
            Assert.IsTrue(!string.IsNullOrEmpty(base36), "Base36 number resulted in empty or null");
            result = StringUtils.Base36Decode(base36);
            Assert.AreEqual(inputNumber, result, "Base36 conversion failed.");

            // negative number
            inputNumber = -512344131333132;

            base36 = StringUtils.Base36Encode(inputNumber);
            Assert.IsTrue(!string.IsNullOrEmpty(base36), "Base36 number resulted in empty or null");
            result = StringUtils.Base36Decode(base36);
            Assert.AreEqual(inputNumber, result, "Base36 conversion failed.");

            inputNumber = 0;

            base36 = StringUtils.Base36Encode(inputNumber);
            Assert.IsTrue(!string.IsNullOrEmpty(base36), "Base36 number resulted in empty or null");
            result = StringUtils.Base36Decode(base36);
            Assert.AreEqual(inputNumber, result, "Base36 conversion failed.");
        }

        [TestMethod]
        public void GenerateUniqueId()
        {
            List<string> list = new List<string>();
            for (int i = 0; i < 100; i++)
            {
                list.Add(DataUtils.GenerateUniqueId());
            }

            int maxLength = list.Max(str => str.Length);
            int minLength = list.Min(str => str.Length);
            int negVals = list.Where(str => str.StartsWith("-")).Count();
            this.TestContext.WriteLine("Min Length: {0}, Max: {1}, Neg: {2}", minLength, maxLength, negVals);

            Assert.IsTrue(list.Distinct().Count() == 100, "Didn't create 100 unique entries");
        }

        [TestMethod]
        public void RandomStringTest()
        {
            for (int i = 0; i < 20; i++)
            {
                string random = StringUtils.RandomString(20);
                foreach (var ch in random)
                    Assert.IsTrue(char.IsLetter(ch));
            }
        }

        [TestMethod]
        public void RandomStringWithNumbersTest()
        {
            for (int i = 0; i < 20; i++)
            {
                string random = StringUtils.RandomString(20,true);
                foreach (var ch in random)
                    Assert.IsTrue(char.IsLetterOrDigit(ch));
            }
        }

        [TestMethod]
        public void ExtractStringTest()
        {
            string source = "Hello: <rant />";
            string extract = StringUtils.ExtractString(source, "<", "/>", false, false, true);

            Console.WriteLine(extract);
            Assert.AreEqual(extract, "<rant />");
        }

        [TestMethod]
        public void ExtractStringWithDelimitersTest()
        {
            string source = @"
# Another Test Blog Post

So this is a new test blog post. I can read this and can do some cool stuff with this.



<!-- Post Configuration -->
---
```xml
<abstract>
This is the abstract ofr this blog post.
</abstract>
<categories>
</categories>
<postid>1420322</postid>
<keywords>
</keywords>
<weblog>
Rick Strahl's Weblog
</weblog>
```
<!-- End Post Configuration -->
";

            string extract = StringUtils.ExtractString(source, "<!-- Post Configuration -->", "<!-- End Post Configuration -->", false,  true, true );

            Console.WriteLine(extract);
            Assert.IsTrue(extract.Contains("<!-- Post Configuration -->"));
            Assert.IsTrue(extract.Contains("<!-- End Post Configuration -->"));
        }


        [TestMethod]
        public void GetLinesTest()
        {
            string s = 
@"this is test
with
multiple lines";

            var strings = StringUtils.GetLines(s);
            Assert.IsNotNull(strings);
            Assert.IsTrue(strings.Length == 3);


            s = string.Empty;
            strings = StringUtils.GetLines(s);
            Assert.IsNotNull(strings);
            Assert.IsTrue(strings.Length == 1);

            s = null;
            strings = StringUtils.GetLines(s);
            Assert.IsNull(strings);            
        }

        [TestMethod]
        public void CountLinesTest()
        {
            string s = 
"this is test\r\n" + 
"with\n" +
"multiple lines";

            int count = StringUtils.CountLines(s);            
            Assert.IsTrue(count == 3);

            s = string.Empty;
            count = StringUtils.CountLines(s);
            Assert.IsTrue(count == 0);

            s = null;
            count = StringUtils.CountLines(s);
            Assert.IsTrue(count == 0);
        }

    }
}
