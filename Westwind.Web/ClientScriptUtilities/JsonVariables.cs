#region License
/*
 **************************************************************
 *  Author: Rick Strahl 
 *          © West Wind Technologies, 2008 2011
 *          http://www.west-wind.com/
 * 
 * Created: 09/04/2008
 *
 * Permission is hereby granted, free of charge, to any person
 * obtaining a copy of this software and associated documentation
 * files (the "Software"), to deal in the Software without
 * restriction, including without limitation the rights to use,
 * copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the
 * Software is furnished to do so, subject to the following
 * conditions:
 * 
 * The above copyright notice and this permission notice shall be
 * included in all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
 * EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
 * OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
 * NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
 * HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
 * WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
 * FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
 * OTHER DEALINGS IN THE SOFTWARE.
 **************************************************************  
*/
#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Web.UI;
using System.Web;
using Westwind.Utilities;
using Westwind.Web.Properties;
using Westwind.Web.JsonSerializers;
using System.Reflection;
using System.Resources;


namespace Westwind.Web
{
    /// <summary>
    /// JsonVariables provides an easy way to embed server side objects and values
    /// as JSON into client side JavaScript code.  
    /// 
    /// You can use the dynamic Add() method to add properties to an object
    /// with each added value becoming a property on the rendered JSON object.
    /// Pass simple values, or complex objects and lists to easily push server
    /// side static data into client side JavaScript code.
    /// 
    /// The static ToJsonString() and ToJsonHtmlString() methods also provide
    /// easy self contained JSON serialization for any object.
    /// 
    /// This component produces either straight string or HtmlString output when used directly
    /// using the ToString() or HtmlString() methods for use in ASP.NET MVC or Web Pages,
    /// or can be used as WebForms control that automatically handles embedding of
    /// the script and deserialization of return values on the server.
    /// 
    /// This component supports:&lt;&lt;ul&gt;&gt;
    /// &lt;&lt;li&gt;&gt; Creating individual client side variables
    /// &lt;&lt;li&gt;&gt; Dynamic values that are 'evaluated' in OnPreRender to 
    /// pick up a value
    /// &lt;&lt;li&gt;&gt; Creating properties of ClientIDs for a given container
    /// &lt;&lt;li&gt;&gt; Changing the object values and POSTing them back on 
    /// Postback
    /// &lt;&lt;/ul&gt;&gt;
    /// 
    /// You create a script variables instance and add new keys to it:
    /// &lt;&lt;code lang="C#"&gt;&gt;
    /// var scriptVars = new JsonVariables("scriptVars");
    /// 
    /// // Simple value
    /// scriptVars.Add("userToken", UserToken);
    /// 
    /// var book = new AmazonBook();
    /// book.Entered = DateTime.Now;
    /// 
    /// // Complex value marshalled
    /// scriptVars.Add("emptyBook", book);
    /// 
    /// In client code you can then access these variables:
    /// &lt;&lt;code lang="JavaScript"&gt;&gt;$( function() {
    /// 	alert(scriptVars.book.Author);
    /// 	alert(scriptVars.author);
    /// 	alert( $("#" + scriptVars.txtAmazonUrlId).val() );
    /// });&lt;&lt;/code&gt;&gt;
    /// </summary>
    public class JsonVariables
    {
        /// <summary>Edit
        /// Internally holds all script variables declared
        /// </summary>
        private Dictionary<string, object> ScriptVars = new Dictionary<string, object>();

        /// <summary>
        /// The name of the object generated in client script code
        /// </summary>
        public string ClientObjectName
        {
            get { return _ClientObjectName; }
            set { _ClientObjectName = value; }
        }

        private string _ClientObjectName = "serverVars";

        /// <summary>
        /// Internal instance of the Json Serializer used to serialize
        /// the object and deserialize the updateable fields
        /// </summary>
        private JSONSerializer JsonSerializer;

        public JsonVariables() : this("serverVars")
        { }
        
        /// <summary>
        /// Full constructor that receives an instance of any control object
        /// and the client name of the generated script object that contains
        /// the specified properties.
        /// </summary>
        /// <param name="control"></param>
        /// <param name="clientObjectName"></param>
        public JsonVariables(string clientObjectName)
        {
            if (!string.IsNullOrEmpty(clientObjectName))
                ClientObjectName = clientObjectName;

            // we have to use the West Wind parser since dates use new Date() formatting as embedded JSON 'date string'
            JsonSerializer = CreateJsonSerializer();
        }

        /// <summary>
        /// Internal routine to create serializer instance
        /// </summary>
        /// <returns></returns>
        static JSONSerializer CreateJsonSerializer()
        {
            var ser = new JSONSerializer(SupportedJsonParserTypes.WestWindJsonSerializer);
            ser.DateSerializationMode = JsonDateEncodingModes.NewDateExpression;
            ser.FormatJsonOutput = true;
            return ser;
        }


        /// <summary>
        /// Adds a property and value to the client side object to be rendered into 
        /// JavaScript code. VariableName becomes a property on the object and the 
        /// value will be properly converted into JavaScript Compatible JSON text.
        /// <seealso>Class ScriptVariables</seealso>
        /// </summary>
        /// <param name="variableName">
        /// The name of the property created on the client object.
        /// </param>
        /// <param name="value">
        /// The value that is to be assigned. Can be any simple type and most complex 
        /// objects that can be serialized into JSON.
        /// </param>
        /// <example>
        /// &amp;lt;&amp;lt;code 
        /// lang=&amp;quot;C#&amp;quot;&amp;gt;&amp;gt;ScriptVariables scriptVars = new
        ///  ScriptVariables(this,&amp;quot;serverVars&amp;quot;);
        /// 
        /// // Add simple values
        /// scriptVars.Add(&amp;quot;name&amp;quot;,&amp;quot;Rick&amp;quot;);
        /// scriptVars.Add(&amp;quot;pageLoadTime&amp;quot;,DateTime.Now);
        /// 
        /// // Add objects
        /// AmazonBook amazon = new AmazonBook();
        /// bookEntity book = amazon.New();
        /// 
        /// scripVars.Add(&amp;quot;book&amp;quot;,book);
        /// &amp;lt;&amp;lt;/code&amp;gt;&amp;gt;
        /// </example>
        public void Add(string variableName, object value)
        {
            ScriptVars[variableName] = value;
        }

        /// <summary>
        /// Adds an entire dictionary of values
        /// </summary>
        /// <param name="values"></param>
        public void Add(IDictionary<object, object> values)
        {
            foreach (var item in values)
            {
                ScriptVars[item.Key.ToString()] = item.Value;
            }
        }

        /// <summary>
        /// Serialization helper that can be used to easily embed
        /// JSON values into ASP.NET pages.
        /// For Razor page embedding use ToJsonHtmlString instead.
        /// </summary>
        /// <param name="val">value or object to serialize</param>
        /// <returns>JSON string</returns>
        public static string ToJsonString(object val)
        {
            var ser = CreateJsonSerializer();
            return ser.Serialize(val);
        }

        /// <summary>
        /// Serialization helper that can be used to easily
        /// embed JSON values into Razor pages. Returns
        /// an HTML string that doesn't require @Html.Raw()
        /// </summary>
        /// <param name="val">value or object to serialize</param>
        /// <returns>JSON string that is not reencoded by Razor</returns>
        public static HtmlString ToJsonHtmlString(object val)
        {
            return new HtmlString(ToJsonString(val));
        }

        /// <summary>
        /// Creates a JSON dictionary where the top level properties
        /// are created from the dictionary's keys.                 
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="val"></param>
        /// <returns></returns>
        public static string ToPropertyDictionaryString<TKey, TValue>(Dictionary<TKey, TValue> val)
        {
            var ser = CreateJsonSerializer();
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("{");
            foreach (var kv in val)
            {
                sb.Append("\t\"" + kv.Key + "\": ");
                sb.AppendLine (ser.Serialize(kv.Value) + ",");
            }

            // strip off trailing comma and crlf
            if (sb.Length > 0)
            {
                sb.Length = sb.Length - 3;
                sb.AppendLine();
            }

            sb.AppendLine("}");

            return sb.ToString();
        }

        public static HtmlString ToPropertyDictionaryHtmlString<TKey, TValue>(Dictionary<TKey, TValue> val)
        {
            return new HtmlString( ToPropertyDictionaryString(val) );
        }
        /// <summary>
        /// Returns the rendered JavaScript for the generated object and name. 
        /// Note this method returns only the generated object, not the 
        /// related code to save updates.
        /// 
        /// You can use this method with MVC Views to embedd generated JavaScript
        /// into the the View page.
        /// </summary>
        public string ToString(bool noVarStatement = false)
        {
            StringBuilder sb = new StringBuilder();

            // If the name includes a . assignment is made to an existing
            // object or namespaced reference - don't create var instance.
            if (!noVarStatement && !ClientObjectName.Contains("."))
                sb.Append("var ");

            sb.Append(ClientObjectName + " =  " + ToJson() + ";");
            
            return sb.ToString();
        }


        /// <summary>
        /// Returns the script as an HTML string. Use this version
        /// with AsP.NET MVC to force raw unencoded output in Razor:
        /// 
        /// @scriptVars.ToHtmlString()
        /// </summary>
        /// <param name="addScriptTags"></param>
        /// <returns></returns>
        public HtmlString ToHtmlString(bool noVarStatement = false)
        {
            return new HtmlString(ToString(noVarStatement));
        }

        /// <summary>
        /// Outputs the variable data as a raw JSON object
        /// </summary>
        /// <returns></returns>
        public string ToJson()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("{");

            foreach (KeyValuePair<string, object> entry in ScriptVars)
            {
                if (string.IsNullOrEmpty(entry.Key))
                {
                    ClientObjectName += " = " + sb.AppendLine(JsonSerializer.Serialize(entry.Value) + ";");
                }
                if (entry.Key.StartsWith("."))
                {
                    // It's a dynamic key
                    string[] tokens = entry.Key.Split(new char[1] { '.' }, StringSplitOptions.RemoveEmptyEntries);
                    string varName = tokens[0];
                    string property = tokens[1];


                    object propertyValue = null;
                    if (entry.Value != null)
                        propertyValue = ReflectionUtils.GetPropertyEx(entry.Value, property);

                    sb.AppendLine("\t\"" + varName + "\": " + JsonSerializer.Serialize(propertyValue) + ",");
                }
                else
                    sb.AppendLine("\t\"" + entry.Key + "\": " + JsonSerializer.Serialize(entry.Value) + ",");
            }

            // Strip off last comma plus CRLF
            if (sb.Length > 0)
                sb.Length -= 3;

            sb.AppendLine("\r\n}");

            return sb.ToString();
        }

        /// <summary>
        /// Outputs the the dictionary as a JSON string for MVC
        /// </summary>
        /// <returns></returns>
        public HtmlString ToJsonHtmlString()
        {
            return new HtmlString(ToJson());
        }

    }
}
