using System;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Xml;
using System.Xml.Serialization;
using Newtonsoft.Json;

namespace Westwind.Web.Mvc
{
    /// <summary>
    /// Returns a content negotiated result based on the Accept header.
    /// Minimal implementation that works with JSON and XML content,
    /// can also optionally return a view with HTML.    
    /// </summary>
    /// <example>
    /// // model data only
    /// public ActionResult GetCustomers()
    /// {
    ///      return new NegotiatedResult(repo.Customers.OrderBy( c=> c.Company) )
    /// }
    /// // optional view for HTML
    /// public ActionResult GetCustomers()
    /// {
    ///      return new NegotiatedResult("List", repo.Customers.OrderBy( c=> c.Company) )
    /// }
    /// </example>
    public class NegotiatedResult : ActionResult
    {
        /// <summary>
        /// Data stored to be 'serialized'. Public
        /// so it's potentially accessible in filters.
        /// </summary>
        public object Data { get; set; }

        /// <summary>
        /// Optional name of the HTML view to be rendered
        /// for HTML responses
        /// </summary>
        public string ViewName { get; set; }

        /// <summary>
        /// Global static flag that allows you to set whether 
        /// output is formatted. By default Debug settings
        /// of project are used - debug is formatted
        /// </summary>
        public static bool FormatOutput { get; set; }

        public static string DefaultContentType { get; set; }

        static NegotiatedResult()
        {
            FormatOutput = HttpContext.Current.IsDebuggingEnabled;
            DefaultContentType = "application/json";
        }

        /// <summary>
        /// Pass in data to serialize
        /// </summary>
        /// <param name="data">Data to serialize</param>        
        public NegotiatedResult(object data)
        {
            Data = data;
        }

        /// <summary>
        /// Pass in data and an optional view for HTML views
        /// </summary>
        /// <param name="data"></param>
        /// <param name="viewName"></param>
        public NegotiatedResult(string viewName, object data)
        {
            Data = data;
            ViewName = viewName;
        }


        public override void ExecuteResult(ControllerContext context)
        {
            if (context == null)
                throw new ArgumentNullException("context");

            var request = context.HttpContext.Request;

            if (request.AcceptTypes != null)
            {

                for (int i = 0; i < request.AcceptTypes.Length; i++)
                {
                    string acceptType = request.AcceptTypes[i];

                    if (string.IsNullOrEmpty(acceptType))
                        continue;

                    if (TryApplyContentType(acceptType, context))
                        return;
                }
            }

            // no content type or nothing matched - try the default
            TryApplyContentType(DefaultContentType,context);
        }


        private bool TryApplyContentType(string acceptType, ControllerContext context)
        {
            var response = context.HttpContext.Response;

            int semi = acceptType.IndexOf(';');
            if (semi > 0)
                acceptType = acceptType.Substring(0, semi);

            acceptType = acceptType.ToLower();

            // Look for specific content types            
            if (acceptType == "application/json")
            {
                response.ContentType = "application/json";

                using (JsonTextWriter writer = new JsonTextWriter(response.Output)
                {
                    Formatting =
                        FormatOutput ? Newtonsoft.Json.Formatting.Indented : Newtonsoft.Json.Formatting.None
                })
                {
                    JsonSerializer serializer = JsonSerializer.Create();
                    serializer.Serialize(writer, Data);
                    writer.Flush();
                }
                return true;
            }
            if (acceptType == "text/xml" || acceptType == "application/xml")
            {
                response.ContentType = "text/xml";
                if (Data != null)
                {
                    using (var writer = new XmlTextWriter(response.OutputStream, new UTF8Encoding()))
                    {
                        if (FormatOutput)
                            writer.Formatting = System.Xml.Formatting.Indented;

                        XmlSerializer serializer = new XmlSerializer(Data.GetType());

                        serializer.Serialize(writer, Data);
                        writer.Flush();
                    }
                }
                return true;
            }
            if (!string.IsNullOrEmpty(ViewName) && acceptType == "text/html")
            {
                response.ContentType = "text/html";

                var viewData = context.Controller.ViewData;
                viewData.Model = Data;

                var viewResult = new ViewResult
                {
                    ViewName = ViewName,
                    MasterName = null,
                    ViewData = viewData,
                    TempData = context.Controller.TempData,
                    ViewEngineCollection = ((Controller) context.Controller).ViewEngineCollection
                };
                viewResult.ExecuteResult(context.Controller.ControllerContext);

                return true;
            }
            if (acceptType == "text/plain")
            {
                response.ContentType = "text/plain";
                response.Write(Data);
                return true;
            }
            return false;
        }
    }

    /// <summary>
    /// Extends Controller with Negotiated() ActionResult that does
    /// basic content negotiation based on the Accept header.
    /// </summary>
    public static class NegotiatedResultExtensions
    {
        /// <summary>
        /// Return content-negotiated content of the data based on Accept header.
        /// Supports:
        ///    application/json  - using JSON.NET
        ///    text/xml   - Xml as XmlSerializer XML
        ///    text/html  - as text, or an optional View
        ///    text/plain - as text
        /// </summary>        
        /// <param name="controller"></param>
        /// <param name="data">Data to return</param>
        /// <returns>serialized data</returns>
        /// <example>
        /// public ActionResult GetCustomers()
        /// {
        ///      return this.Negotiated( repo.Customers.OrderBy( c=> c.Company) )
        /// }
        /// </example>
        public static NegotiatedResult Negotiated(this Controller controller, object data)
        {
            return new NegotiatedResult(data);
        }

        /// <summary>
        /// Return content-negotiated content of the data based on Accept header.
        /// Supports:
        ///    application/json  - using JSON.NET
        ///    text/xml   - Xml as XmlSerializer XML
        ///    text/html  - as text, or an optional View
        ///    text/plain - as text
        /// </summary>        
        /// <param name="controller"></param>
        /// <param name="viewName">Name of the View to when Accept is text/html</param>
        /// /// <param name="data">Data to return</param>        
        /// <returns>serialized data</returns>
        /// <example>
        /// public ActionResult GetCustomers()
        /// {
        ///      return this.Negotiated("List", repo.Customers.OrderBy( c=> c.Company) )
        /// }
        /// </example>
        public static NegotiatedResult Negotiated(this Controller controller, string viewName, object data)
        {
            return new NegotiatedResult(viewName, data);
        }
    }
}
