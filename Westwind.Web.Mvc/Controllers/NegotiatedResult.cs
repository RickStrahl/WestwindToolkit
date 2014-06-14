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

        static NegotiatedResult()
        {
            FormatOutput = HttpContext.Current.IsDebuggingEnabled;
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

            HttpResponseBase response = context.HttpContext.Response;
            HttpRequestBase request = context.HttpContext.Request;


            // Look for specific content types            
            if (request.AcceptTypes.Contains("text/html"))
            {
                response.ContentType = "text/html";

                if (!string.IsNullOrEmpty(ViewName))
                {
                    var viewData = context.Controller.ViewData;
                    viewData.Model = Data;

                    var viewResult = new ViewResult
                    {
                        ViewName = ViewName,
                        MasterName = null,
                        ViewData = viewData,
                        TempData = context.Controller.TempData,
                        ViewEngineCollection = ((Controller)context.Controller).ViewEngineCollection
                    };
                    viewResult.ExecuteResult(context.Controller.ControllerContext);
                }
                else
                    response.Write(Data);
            }
            else if (request.AcceptTypes.Contains("application/json"))
            {
                response.ContentType = "application/json";

                using (JsonTextWriter writer = new JsonTextWriter(response.Output)
                {
                    Formatting = Newtonsoft.Json.Formatting.Indented
                })
                {
                    JsonSerializer serializer = JsonSerializer.Create();
                    serializer.Serialize(writer, Data);
                    writer.Flush();
                }
            }
            else if (request.AcceptTypes.Contains("text/xml"))
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
            }
            else if (request.AcceptTypes.Contains("text/plain"))
            {
                response.ContentType = "text/plain";
                response.Write(Data);
            }
            else
            {
                // just write data as a plain string
                response.Write(Data);
            }

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
