using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
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
            var settings = new HttpRequestSettings()
            {
                Url = "http://microsoft.com",
            };

            string html = HttpUtils.HttpRequestString(settings);
            Assert.IsNotNull(html);
            Assert.IsTrue(settings.ResponseStatusCode == System.Net.HttpStatusCode.OK);
        }

        [TestMethod]
        public async Task HttpClientJsonRequestTest()
        {
            var handler = new WebRequestHandler();
            //handler.Proxy = new WebProxy("http://localhost:8888/");
            //handler.Credentials = new NetworkCredential(uid, pwd);            

            var client = new HttpClient(handler);

            var postSnippet = new CodeSnippet()
            {
                UserId = "Bogus",
                Code = "string.Format('Hello World, I will own you!');",
                Comment = "World domination imminent"
            };

            // this isn't actually an API so the POST is ignored
            // but it always returns a JSON response 
            string url = "http://codepaste.net/recent?format=json";

            var response = await client.PostAsync(url, postSnippet,
                                                  new JsonMediaTypeFormatter(), null);

            Assert.IsTrue(response.IsSuccessStatusCode);

            var snippets = await response.Content.ReadAsAsync<List<CodeSnippet>>();
            Assert.IsTrue(snippets.Count > 0);

            foreach (var snippet in snippets)
            {
                if (string.IsNullOrEmpty(snippet.Code))
                    continue;
                Console.WriteLine(snippet.Code.Substring(0, Math.Min(snippet.Code.Length, 200)));
                Console.WriteLine("--");
            }
        }



        [TestMethod]
        public void JsonRequestTest()
        {
            var settings = new HttpRequestSettings()
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
            var settings = new HttpRequestSettings()
            {
                Url = "http://codepaste.net/recent?format=json",

            };

            var snippets = await HttpUtils.JsonRequestAsync<List<CodeSnippet>>(settings);

            Assert.IsNotNull(snippets);
            Assert.IsTrue(settings.ResponseStatusCode == System.Net.HttpStatusCode.OK);
            Assert.IsTrue(snippets.Count > 0);
            Console.WriteLine(snippets.Count);
            Console.WriteLine(settings.CapturedResponseContent);
        }

        [TestMethod]
        public void JsonRequestPostTest()
        {
            var postSnippet = new CodeSnippet()
            {
                UserId = "Bogus",
                Code = "string.Format('Hello World, I will own you!');",
                Comment = "World domination imminent"
            };

            var settings = new HttpRequestSettings()
            {
                Url = "http://codepaste.net/recent?format=json",
                Content = postSnippet,
                HttpVerb = "POST"
            };

            var snippets = HttpUtils.JsonRequest<List<CodeSnippet>>(settings);

            Assert.IsNotNull(snippets);
            Assert.IsTrue(settings.ResponseStatusCode == System.Net.HttpStatusCode.OK);
            Assert.IsTrue(snippets.Count > 0);

            Console.WriteLine(snippets.Count);
            Console.WriteLine(settings.CapturedRequestContent);
            Console.WriteLine();
            Console.WriteLine(settings.CapturedResponseContent);

            foreach (var snippet in snippets)
            {
                if (string.IsNullOrEmpty(snippet.Code))
                    continue;
                Console.WriteLine(snippet.Code.Substring(0, Math.Min(snippet.Code.Length, 200)));
                Console.WriteLine("--");
            }
            
            Console.WriteLine("Status Code: " + settings.Response.StatusCode);

            foreach (var header in settings.Response.Headers)
            {
                Console.WriteLine(header + ": " + settings.Response.Headers[header.ToString()]);
            }
        }

        [TestMethod]
        public async Task JsonRequestPostAsyncTest()
        {
            var postSnippet = new CodeSnippet()
            {
                UserId = "Bogus",
                Code = "string.Format('Hello World, I will own you!');",
                Comment = "World domination imminent"
            };

            var settings = new HttpRequestSettings()
            {
                Url = "http://codepaste.net/recent?format=json",
                Content = postSnippet,
                HttpVerb = "POST"
            };

            var snippets = await HttpUtils.JsonRequestAsync<List<CodeSnippet>>(settings);

            Assert.IsNotNull(snippets);
            Assert.IsTrue(settings.ResponseStatusCode == System.Net.HttpStatusCode.OK);
            Assert.IsTrue(snippets.Count > 0);

            Console.WriteLine(snippets.Count);
            Console.WriteLine(settings.CapturedRequestContent);
            Console.WriteLine();
            Console.WriteLine(settings.CapturedResponseContent);

            foreach (var snippet in snippets)
            {
                if (string.IsNullOrEmpty(snippet.Code))
                    continue;
                Console.WriteLine(snippet.Code.Substring(0, Math.Min(snippet.Code.Length, 200)));
                Console.WriteLine("--");
            }

            // This doesn't work for the async version - Response is never set by the base class
            Console.WriteLine("Status Code: " + settings.Response.StatusCode);

            foreach (var header in settings.Response.Headers)
            {
                Console.WriteLine(header + ": " + settings.Response.Headers[header.ToString()]);
            }
        }
    }

    public class CodeSnippet
    {
        public int CommentCount { get; set; }
        public string Id { get; set; }
        public string UserId { get; set; }
        public string Language { get; set; }
        public int Views { get; set; }
        public string Code { get; set; }
        public string Comment { get; set; }
        public DateTime Entered { get; set; }
    }
}
