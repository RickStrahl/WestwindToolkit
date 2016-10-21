using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Westwind.Utilities.Test
{
    [TestClass]
    public class FileUtilsTest
    {
        [TestMethod]
        public void SafeFilenameTest()
        {
            string file = "This: iS this file really invalid?";

            string result = FileUtils.SafeFilename(file);

            Assert.AreEqual(result, "This iS this file really invalid");

            Console.WriteLine(result);
        }
    }
}
