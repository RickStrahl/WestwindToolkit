using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;
using Westwind.Web.Mvc;
using System.Collections.Specialized;

namespace Westwind.Web.Mvc.Test
{
    [TestClass]
    public class MvcHtmlUtilsTests
    {
        [TestMethod]
        public void SelectListItemsFromDictionaryTest()
        {
            Dictionary<string, string> dict = new Dictionary<string, string>();
            dict.Add("Name", "Rick");
            dict.Add("Company", "Westwind");

            var listItems = MvcHtmlUtils.SelectListItemsFromDictionary(dict);

            Assert.IsNotNull(listItems);
            Assert.IsTrue(listItems.Count() == 2);

            foreach (var li in listItems)
            {
                Console.WriteLine("{0}: {1}", li.Value, li.Text);
            }        
        }
    }
}
