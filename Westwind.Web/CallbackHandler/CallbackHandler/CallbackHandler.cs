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
using System.Web;
using System.Text;
using System.Reflection;
using Westwind.Utilities;
using System.Web.Routing;
using System.Linq;

namespace Westwind.Web
{
    /// <summary>
    /// CallbackHandler is an Http Handler base class that allows you to create a
    ///  class with methods marked up with a [CallbackMethod] attribute which are 
    /// then exposed for remote execution. The handler routes the to the methods 
    /// and executes the call and returns the results - or any errors - as JSON 
    /// strings.
    /// 
    /// To use this service you need to create an HttpHandler (either .ASHX or a 
    /// class registered in web.config's httpHandler section) and then add methods 
    /// with the [CallbackMethod] attribute and that's it. The service accepts 
    /// inputs via query string and POST data.
    /// 
    /// If you use the AjaxMethodCallbackControl the process of calling service
    /// methods is fully automated including automatic client proxy creation and
    /// you can call methods with individual parameters mapped from client to server.
    /// 
    /// Alternately you can also use plain REST calls that pass either no parameters
    /// and purely access POST data, or pass a single JSON object that can act as 
    /// a single input parameter.
    /// 
    /// The service can be accessed with:
    /// 
    /// MyHandler.ashx?CallbackMethod=MethodToCall
    /// 
    /// POST data can then be passed in to act as parameters:
    /// 
    /// &lt;&lt;ul&gt;&gt;
    /// &lt;&lt;li&gt;&gt; &lt;&lt;b&gt;&gt;Raw Post Buffer&lt;&lt;/b&gt;&gt;
    /// You simply pass raw POST data that you can access with Request.Form in the 
    /// handler
    /// 
    /// &lt;&lt;li&gt;&gt; &lt;&lt;b&gt;&gt;JSON value or object 
    /// string&lt;&lt;/b&gt;&gt;
    /// Alternately you can set the content type to application/json and pass a 
    /// JSON string of a value or object which calls the server method with a 
    /// single parameter of matching type.
    /// &lt;&lt;/ul&gt;&gt;
    /// 
    /// For more information on how to call these handlers see 
    /// <see cref="_24I0VDWUR">Using CallbackHandler with REST Calls</see>.
    /// </summary>
    public class CallbackHandler : IHttpHandler
    {

        /// <summary>
        /// Shortcut to the ASP.NET Response object.
        /// 
        /// Allows easy access to the ASP.NET Response object to allow formatting of 
        /// the response. Note that CallbackHandler will set default content types and 
        /// encoding for a request which can be overridden in your code if necessary.
        /// 
        /// &lt;&lt;code lang="C#"&gt;&gt;[CallbackMethod(RouteUrl="stocks/{symbol}")]
        /// public StockQuote GetStockQuote(string symbol)
        /// {
        ///     Response.Cache.SetExpires(DateTime.UtcNow.Add(new TimeSpan(0, 2, 0)));
        /// 
        ///     StockServer server = new StockServer();
        ///     return server.GetStockQuote(symbol);
        /// }&lt;&lt;/code&gt;&gt;
        /// <seealso>Class CallbackHandler</seealso>
        /// </summary>
        public HttpResponse Response { get; set; }

        /// <summary>
        /// Shortcut reference to active ASP.NET Request object
        /// </summary>
        public HttpRequest Request { get; set; }

        /// <summary>
        /// Shortcut to the current HttpContext instance
        /// </summary>
        public HttpContext Context { get; set; }

        /// <summary>
        /// This handler is not thread-safe
        /// </summary>
        public bool IsReusable
        {
            get { return false; }
        }

        /// <summary>
        /// Optionally store RouteData on this handler
        /// so we can access it internally
        /// </summary>
        public RouteData RouteData {get; set; }

        /// <summary>
        /// Handle the actual callback by deferring to JsonCallbackMethodProcessor()
        /// </summary>
        /// <param name="context"></param>
        public virtual void ProcessRequest(HttpContext context)
        {
            var pathInfo = Request.PathInfo;
            // handle WCF/ASMX style type wrappers for handler implementation
            // returns a separate dynamic link with the JavaScript Service Proxy
            if (pathInfo == "/jsdebug" ||
                pathInfo == "/js")
            {
                GenerateClassWrapperForCallbackMethods();
                return;
            }

            Request = context.Request;
            Response = context.Response;
            Context = context;

            // Pass off to the worker Callback Processor

            ICallbackMethodProcessor processor;

            // default format is JSON - check for others
            string format = (context.Request.Params["format"] ?? "").ToLower();

            // check accept types
            if (string.IsNullOrEmpty(format))
            {                
                if (context.Request.AcceptTypes.Where(str => str.Contains(WebResources.STR_JsonContentType) || str.Contains(WebResources.STR_JavaScriptContentType)).Count() > 0)
                    format = "json";
                else if (context.Request.AcceptTypes.Where(str => str.Contains(WebResources.STR_XmlContentType)).Count() > 0)
                    format = "xml";
            }
            
            if (format == "xml")
                processor = new XmlCallbackMethodProcessor();
            else
                processor = new JsonCallbackMethodProcessor();

            // Process the inbound request and execute it on this 
            // Http Handler's methods 
            processor.ProcessCallbackMethodCall(this);
            
            Request = null;
            Response = null;
            Context = null;
        }


        void GenerateClassWrapperForCallbackMethods()
        {
            Type objectType = GetType();

            StringBuilder sb = new StringBuilder(2048);
            string nameSpace = GetType().Namespace;
            string typeId = GetType().Name;

            if (!string.IsNullOrEmpty(nameSpace))
            {
                if (true) //ClientScriptProxy.IsMsAjax())
                    sb.AppendLine("registerNamespace(\"" + nameSpace + "\");");

                sb.AppendLine(nameSpace + "." + typeId + " = { ");
            }
            else
                sb.AppendLine(typeId + " = { ");

            MethodInfo[] Methods = objectType.GetMethods(BindingFlags.Instance | BindingFlags.Public);
            foreach (MethodInfo Method in Methods)
            {
                if (Method.GetCustomAttributes(typeof(CallbackMethodAttribute), false).Length > 0)
                {
                    sb.Append("    " + Method.Name + ": function " + "(");

                    string ParameterList = "";
                    foreach (ParameterInfo Parm in Method.GetParameters())
                    {
                        ParameterList += Parm.Name + ",";
                    }
                    sb.Append(ParameterList + "completed,errorHandler)");

                    sb.AppendFormat(
@"
    {{
        var _cb = {0}_GetProxy();
        _cb.callMethod(""{1}"",[{2}],completed,errorHandler);
        return _cb;           
    }},
", typeId, Method.Name, ParameterList.TrimEnd(','));

                }
            }

            if (sb.Length > 2)
                sb.Length -= 3; // strip trailing ,\r\n                                    

            // End of class
            sb.Append("\r\n}\r\n");


            string Url = Request.Path.ToLower().Replace(Request.PathInfo, "");

            sb.Append(
"function " + typeId + @"_GetProxy() {
    var _cb = new AjaxMethodCallback('" + typeId + "','" + Url + @"');
    _cb.serverUrl = '" + Url + @"';
    _cb.postbackMode = 'PostMethodParametersOnly';    
    return _cb;
}    
");
            WebUtils.GZipEncodePage();

            Response.ContentType = WebResources.STR_JavaScriptContentType;
            HttpContext.Current.Response.Write(sb.ToString());
        }


    }
}
