using System.Net;

namespace Westwind.Utilities
{
    /// <summary>
    /// Customized version of WebClient that provides access
    /// to the Response object so we can read result data 
    /// from the Response.
    /// </summary>
    public class HttpUtilsWebClient : WebClient
    {
        internal HttpWebResponse Response { get; set; }

        protected override WebResponse GetWebResponse(WebRequest request)
        {
            Response = base.GetWebResponse(request) as HttpWebResponse;
            return Response;
        }

        protected override WebResponse GetWebResponse(WebRequest request, System.IAsyncResult result)
        {
            Response = base.GetWebResponse(request, result) as HttpWebResponse;
            return Response;
        }

    }
}