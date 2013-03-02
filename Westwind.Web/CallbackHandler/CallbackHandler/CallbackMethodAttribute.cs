using System;

namespace Westwind.Web
{
    /// <summary>
    /// Marker Attribute to be used on Callback methods. Signals
    /// parser that the method is allowed to be executed remotely
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class CallbackMethodAttribute : Attribute
    {
        /// <summary>
        /// Allows specification of an ASP.NET style route URL to
        /// a Service Method.        
        /// 
        /// Parameterized route fragments ( {parameter} ) should match
        /// parameter names. 
        /// <example>        
        /// products/{id}
        /// products/add
        /// products/add/{sku}
        /// service/helloworld/{name}/{company}
        /// </example>
        /// <remarks>
        /// Applies only to CallbackHandler based handlers. 
        /// Doesn't have any effect on Page methods called with AjaxMethodCallback
        /// 
        /// Routes need to *uniquely* identify a method in a CallbackHandler.
        /// Make sure you don't have the same route or same partially parameterized
        /// route pointing at multiple methods - in which case you may end up with
        /// routing mismatches. 
        /// stocks/{symbol}
        /// stocks/{symbollist}
        /// </remarks>
        /// </summary>        
        public string RouteUrl
        {
            get { return _RouteUrl; }
            set { _RouteUrl = value; }
        }
        private string _RouteUrl = null;

        /// <summary>
        /// Content Type used for results that are returned as Stream
        /// or raw string values. Same as setting Response.ContentType
        /// but more clear in the attribute
        /// </summary>
        public string ContentType
        {
            get { return _ContentType; }
            set { _ContentType = value; }
        }
        private string _ContentType = string.Empty;


        /// <summary>
        /// Allows specifying of the HTTP Verb that is accepted for the called method.
        /// 
        /// The default is HttpVerbs.All.
        /// 
        /// <remarks>
        /// Requests that fail to access the endpoint with the right HTTP Verb will not
        /// get called and return a 405 error along with an error object (JSON/XML) that 
        /// details the error in the body.
        /// </remarks>
        /// <seealso>Class CallbackMethodAttribute</seealso>
        /// </summary>
        /// <example>
        /// &lt;&lt;code lang=&quot;C#&quot;&gt;&gt;// Allow both GET and POST 
        /// operations
        /// [CallbackMethod(AllowedHttpVerbs=HttpVerbs.GET | HttpVerbs.POST),
        ///                 RouteUrl=&quot;stocks/{symbol}&quot;]
        /// public StockQuote GetStockQuote(string symbol)
        /// { ... }&lt;&lt;/code&gt;&gt;
        /// </example>
        public HttpVerbs AllowedHttpVerbs
        {
            get { return _AllowedHttpVerbs; }
            set { _AllowedHttpVerbs = value; }
        }
        private HttpVerbs _AllowedHttpVerbs = HttpVerbs.All;


        /// <summary>
        /// When set to true indicates that a string result returned to the
        /// client should not be encoded in anw way. This can be more efficient for 
        /// large string results passed back to the client when returning
        /// HTML or other plain text and avoids extra encoding and decoding.
        /// 
        /// The client can also specify format=string on the querystring to
        /// return string values as raw strings.
        /// </summary>
        public bool ReturnAsRawString
        {
            get { return _ReturnAsRawString; }
            set { _ReturnAsRawString = value; }
        }
        private bool _ReturnAsRawString = false;




        /// <summary>
        /// Default Constructor for CallbackMethodAttribute. No functionality added
        /// </summary>
        public CallbackMethodAttribute()
        {
        }

    }

    [Flags]
    public enum HttpVerbs
    {        
        GET = 1,
        POST = 2,
        PUT = 4,
        DELETE = 8,
        All = 15,
        None =0
    }
}
