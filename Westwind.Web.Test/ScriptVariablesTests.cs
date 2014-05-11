using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Web;
using System.Collections.Generic;

namespace Westwind.Web.Test
{
    [TestClass]
    public class ScriptVariablesTests
    {
        [TestMethod]
        public void ClientScriptVariablesTest()
        {
            var scriptVars = new ScriptVariables("pageVars");
            scriptVars.Add("name", "Rick & Company");
            scriptVars.Add("date", DateTime.Now);
            scriptVars.Add("item", new Item() { Sku = "wwhelp", Description = "Help Builder  < test >", Price = 299M });

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
        public void ClientScriptVariablesWithItemsTest()
        {
            var scriptVars = new ScriptVariables("pageVars");
            scriptVars.Add("name", "Rick & Company");
            scriptVars.Add("date", DateTime.Now);
            scriptVars.Add("item", new Item() { Sku = "wwhelp", Description = "Help Builder  < test >", Price = 299M });

            scriptVars.UpdateMode = AllowUpdateTypes.ItemsOnly;

            // output as a string
            string output = scriptVars.ToString(true);

            Console.WriteLine(output);

            Assert.IsNotNull(output);
            Assert.IsTrue(output.Contains("Help Builder"));
        }

        [TestMethod]
        public void ClientScriptVariablesDictionaryInitializationTest()
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
        public void ClientScriptVariablesDictionaryWithNumberedIndexesInitializationTest()
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
            
        }
    }

}
