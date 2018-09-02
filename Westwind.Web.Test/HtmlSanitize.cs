using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Westwind.Utilities;

namespace Westwind.Web.Tests
{
    [TestClass]
    public class HtmlSanitizeTests
    {
        [TestMethod]
        public void HtmlSanitizeScriptTags()
        {
            string html = "<div>User input with <ScRipt>alert('Gotcha');</ScRipt></div>";

            var result = WebUtils.SanitizeHtml(html);

            Console.WriteLine(result);
            Assert.IsTrue(!result.Contains("<ScRipt>"));
        }


        [TestMethod]
        public void HtmlSanitizeJavaScriptTags()
        {
            string html = "<div>User input with <a href=\"javascript: alert('Gotcha')\">Don't hurt me!<a/></div>";

            var result = WebUtils.SanitizeHtml(html);

            Console.WriteLine(result);
            Assert.IsTrue(!result.Contains("javascript:"));
        }

        [TestMethod]
        public void HtmlSanitizeJavaScriptTagsSingleQuotes()
        {
            string html = "<div>User input with <a href='javascript: alert(\"Gotcha\");'>Don't hurt me!<a/></div>";

            var result = WebUtils.SanitizeHtml(html);

            Console.WriteLine(result);
            Assert.IsTrue(!result.Contains("javascript:"));
        }


        [TestMethod]
        public void HtmlSanitizeEventAttributes()
        {
            string html = "<div onmouseover=\"alert('Gotcha!')\">User input with " +
                          "<div onclick='alert(\"Gotcha!\");'>Don't hurt me!<div/>" +
                          "</div>";

            var result = WebUtils.SanitizeHtml(html);

            Console.WriteLine(result);
            Assert.IsTrue(!result.Contains("onmouseover:") && !result.Contains("onclick"));
        }
    }
}
