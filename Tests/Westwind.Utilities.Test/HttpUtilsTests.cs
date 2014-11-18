using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Westwind.Data.Test.Models;

namespace Westwind.Utilities.Test
{
    [TestClass]
    public class HttpUtilsTests
    {
        [TestMethod]
        public void HttpRequestStringWithUrlTest()
        {
            string html = HttpUtils.HttpRequestString("http://microsoft.com");
            Assert.IsNotNull(html);            
        }

        [TestMethod]
        public async Task HttpRequestStringWithUrlAsyncTest()
        {
            string html = await HttpUtils.HttpRequestStringAsync("http://microsoft.com");
            Assert.IsNotNull(html);
        }

        [TestMethod]
        public void HttpRequestStringWithSettingsTest()
        {
            var settings = new HttpHelperRequestSettings()
            {
                Url = "http://microsoft.com",
                 
            };

            string html = HttpUtils.HttpRequestString(settings);
            Assert.IsNotNull(html);
            Assert.IsTrue(settings.ResponseStatusCode == System.Net.HttpStatusCode.OK);
        }

        
        [TestMethod]
        public void JsonRequestTest()
        {
            var settings = new HttpHelperRequestSettings()
            {
                Url = "http://codepaste.net/recent?format=json",
                 
            };

            var snippets = HttpUtils.JsonRequest<List<CodeSnippet>>(settings);

            Assert.IsNotNull(snippets);
            Assert.IsTrue(settings.ResponseStatusCode == System.Net.HttpStatusCode.OK);
            Assert.IsTrue(snippets.Count > 0);
            Console.WriteLine(snippets.Count);
        }

        [TestMethod]
        public async Task JsonRequestAsyncTest()
        {
            var settings = new HttpHelperRequestSettings()
            {
                Url = "http://codepaste.net/recent?format=json",

            };

            var snippets = await HttpUtils.JsonRequestAsync<List<CodeSnippet>>(settings);

            Assert.IsNotNull(snippets);
            Assert.IsTrue(settings.ResponseStatusCode == System.Net.HttpStatusCode.OK);
            Assert.IsTrue(snippets.Count > 0);
            Console.WriteLine(snippets.Count);
            Console.WriteLine(settings.ResponseData);
        }

        [TestMethod]
        public async Task JsonRequestPostAsyncTest()
        {
            var data = new Customer()
            {
                FirstName = "Joe",
                LastName = "Blow",
                Entered = DateTime.Now,
                Orders = new List<Order>()
            };


            var settings = new HttpHelperRequestSettings()
            {
                Url = "http://codepaste.net/recent?format=json",
                Data = data,
                HttpVerb = "POST"
            };
            
            var snippets = await HttpUtils.JsonRequestAsync<List<CodeSnippet>>(settings);

            Assert.IsNotNull(snippets);
            Assert.IsTrue(settings.ResponseStatusCode == System.Net.HttpStatusCode.OK);
            Assert.IsTrue(snippets.Count > 0);
            Console.WriteLine(snippets.Count);
            Console.WriteLine(settings.RequestData);
            Console.WriteLine();
            Console.WriteLine(settings.ResponseData);
        }

    }

    public class CodeSnippet
    {
        public int CommentCount { get; set; }
        public string Id { get; set; }
        public string UserId { get; set; }
        public string Language { get; set; }
        public int Views { get; set; }
        public DateTime Entered { get; set; }
    }
}
