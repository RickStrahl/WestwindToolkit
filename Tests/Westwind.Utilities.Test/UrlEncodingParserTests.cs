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
    public class UrlEncodingParserTests
    {
        [TestMethod]
        public void QueryStringTest()
        {
            string str = "http://mysite.com/page1?id=3123&format=json&action=edit&text=It's%20a%20brave%20new%20world!";

            var query = new UrlEncodingParser(str);

            Assert.IsTrue(query["id"] == "3123");
            Assert.IsTrue(query["format"] == "json", "wrong format " + query["format"]);
            Assert.IsTrue(query["action"] == "edit");

            Console.WriteLine(query["text"]);
            // It's a brave new world!

            query["id"] = "4123";
            query["format"] = "xml";
            query["name"] = "<< It's a brave new world!";

            var url = query.Write();

            Console.WriteLine(url);
            //http://mysite.com/page1?id=4123&format=xml&action=edit&
            //text=It's%20a%20brave%20new%20world!&name=%3C%3C%20It's%20a%20brave%20new%20world!
        }

        [TestMethod]
        public void QueryStringMultipleTest()
        {
            string str = "http://mysite.com/page1?id=3123&format=json&format=xml";

            var query = new UrlEncodingParser(str);

            Assert.IsTrue(query["id"] == "3123");
            Assert.IsTrue(query["format"] == "json,xml", "wrong format " + query["format"]);            

            // multiple format strings
            string[] formats = query.GetValues("format");
            Assert.IsTrue(formats.Length == 2);

            query.SetValues("multiple", new[]
    {
        "1",
        "2",
        "3"
    });

            var url = query.Write();

            Console.WriteLine(url);

            Assert.IsTrue(url ==
                            "http://mysite.com/page1?id=3123&format=json&format=xml&multiple=1&multiple=2&multiple=3");

        }

        [TestMethod]
        public void WriteUrlTest()
        {
            // URL only
            string url = "http://test.com/page";

            var query = new UrlEncodingParser(url);
            query["id"] = "321312";
            query["name"] = "rick";

            url = query.Write();            
            Console.WriteLine(url);

            Assert.IsTrue(url.Contains("name="));
            Assert.IsTrue(url.Contains("http://"));

            // URL with ? but no query
            url = "http://test.com/page?";

            query = new UrlEncodingParser(url);
            query["id"] = "321312";
            query["name"] = "rick";

            url = query.Write();
            Console.WriteLine(url);

            Assert.IsTrue(url.Contains("name="));


            // URL with query
            url = "http://test.com/page?q=search";

            query = new UrlEncodingParser(url);
            query["id"] = "321312";
            query["name"] = "rick";

            url = query.Write();
            Console.WriteLine(url);

            Assert.IsTrue(url.Contains("name="));
            Assert.IsTrue(url.Contains("http://"));


            // Raw query data
            url = "q=search&name=james";

            query = new UrlEncodingParser(url);
            query["id"] = "321312";
            query["name"] = "rick";

            url = query.Write();
            Console.WriteLine(url);

            Assert.IsTrue(url.Contains("name="));
            Assert.IsTrue(!url.Contains("http://"));


            // No data at all
            url = null;

            query = new UrlEncodingParser();
            query["id"] = "321312";
            query["name"] = "rick";

            url = query.Write();
            Console.WriteLine(url);

            Assert.IsTrue(url.Contains("name="));
            Assert.IsTrue(!url.Contains("http://"));
        }
    }
}
