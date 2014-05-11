using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Web;
using System.Collections.Generic;

namespace Westwind.Web.Test
{
    [TestClass]
    public class JsonVariablesTests
    {
        [TestMethod]
        public void JsonVariablesTest()
        {
            var scriptVars = new JsonVariables("pageVars");
            scriptVars.Add("name", "Rick & Company");
            scriptVars.Add("date", DateTime.Now);
            scriptVars.Add("item", new  { Sku = "wwhelp", Description = "Help Builder < test >\r\n", Price = 299M });

            // output as a string
            string output = scriptVars.ToString();
            
            Assert.IsNotNull(output);
            Assert.IsTrue(output.Contains("Help Builder"));
            Console.WriteLine(output);

            HtmlString htmlOutput = scriptVars.ToHtmlString();

            Assert.IsNotNull(htmlOutput);
            Assert.IsTrue(output.Contains("Help Builder"));
            Console.WriteLine(htmlOutput);
        }

        [TestMethod]
        public void JavaScriptVariablesWithItemsTest()
        {
            var scriptVars = new JsonVariables("pageVars");
            scriptVars.Add("name", "Rick & Company");
            scriptVars.Add("date", DateTime.Now);
            scriptVars.Add("item", new Item() { Sku = "wwhelp", Description = "Help Builder  < test >", Price = 299M });
            

            // output as a string
            string output = scriptVars.ToString(true);

            Console.WriteLine(output);

            Assert.IsNotNull(output);
            Assert.IsTrue(output.Contains("Help Builder"));
        }

        [TestMethod]
        public void JavaScriptVariablesDictionaryInitializationTest()
        {
            var scriptVars = new ScriptVariables("pageVars");
            scriptVars.Add( new Dictionary<string,object>()
            {
                { "name", "Rick & Company" },
                { "date", DateTime.Now },
                { "item", new Item() { Sku = "wwhelp", Description = "Help Builder  < test >", Price = 299M }}
            });
            
            // output as a string
            string output = scriptVars.ToString(true);

            Console.WriteLine(output);
            Assert.IsNotNull(output);
            Assert.IsTrue(output.Contains("Help Builder"));
        }


        [TestMethod]
        public void JavaScriptVariablesDictionaryWithNumberedIndexesInitializationTest()
        {
            var scriptVars = new ScriptVariables("pageVars");
            scriptVars.Add("values",new Dictionary<int, object>
            {
                { 2, "Rick & Company" },
                { 4, DateTime.Now },
                { 6, new Item() { Sku = "wwhelp", Description = "Help Builder  < test >", Price = 299M }}
            });

            // output as a string
            string output = scriptVars.ToString();

            Console.WriteLine(output);
            Assert.IsNotNull(output);
            Assert.IsTrue(output.Contains("Help Builder"));
        }

        [TestMethod]
        public void ReturnNestedVariableTest()
        {
            var scriptVars = new ScriptVariables("app.names");
            scriptVars.Add("values", new 
            {
                name =  "Rick",
                company = "Westwind",
                entered = DateTime.UtcNow
            });            

            // output as a string
            string output = scriptVars.ToString();

            Console.WriteLine(output);
            Assert.IsNotNull(output);
            Assert.IsTrue(output.Contains("Westwind"));            
            
            // . in varname should prevent var statement from generating
            Assert.IsFalse(output.Contains("var"));            
        }

        [TestMethod]
        public void ReturnNoVarTest()
        {
            var scriptVars = new JsonVariables("scriptVars");
            scriptVars.Add("values", new
            {
                name = "Rick",
                company = "Westwind",
                entered = DateTime.UtcNow
            });

            // output as a string - no var statement
            string output = scriptVars.ToString(true);

            Console.WriteLine(output);
            Assert.IsNotNull(output);
            Assert.IsTrue(output.Contains("Westwind"));

            // . in varname should prevent var statement from generating
            Assert.IsFalse(output.Contains("var"));
        }

        [TestMethod]
        public void ToPropertyDictionaryTest()
        {
            var dict = new Dictionary<string, object>
            {
                {
                    "name", "Rick"
                },
                {
                    "company", "West Wind"
                },
                {
                    "entered", DateTime.Now
                }
            };

            string output = JsonVariables.ToPropertyDictionaryString(dict);


            Console.WriteLine(output);
            Assert.IsNotNull(output);
            Assert.IsTrue(output.Contains("West Wind"));            
        }
    }

    public class Item
    {
        public string Sku { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
    }
}
