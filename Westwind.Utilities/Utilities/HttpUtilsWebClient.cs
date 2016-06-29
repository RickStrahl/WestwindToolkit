using System;
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
        public HttpUtilsWebClient(HttpRequestSettings settings = null)
        {
            Settings = settings;
        }

        internal HttpRequestSettings Settings { get; set; }
        internal HttpWebResponse Response { get; set; }
        internal HttpWebRequest Request { get; set; }

        protected override WebRequest GetWebRequest(Uri address)
        {
            Request = base.GetWebRequest(address) as HttpWebRequest;

            if (Settings != null)
            {
                if (Settings.Timeout > 0)
                {
                    Request.Timeout = Settings.Timeout;
                    Request.ReadWriteTimeout = Settings.Timeout;
                }
            }

            return Request;
        }

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