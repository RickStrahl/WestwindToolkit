using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Westwind.Utilities
{
    /// <summary>
    /// Simple HTTP request helper to let you retrieve data from a Web
    /// server and convert it to something useful.
    /// </summary>
    public static class HttpUtils
    {

        /// <summary>
        /// Retrieves and Http request and returns data as a string.
        /// </summary>
        /// <param name="url">A url to call for a GET request without custom headers</param>
        /// <returns>string of HTTP response</returns>
        public static string HttpRequestString(string url)
        {            
            return HttpRequestString(new HttpRequestSettings() { Url = url });
        }

        /// <summary>
        /// Retrieves and Http request and returns data as a string.
        /// </summary>
        /// <param name="settings">Pass HTTP request configuration parameters object to set the URL, Verb, Headers, content and more</param>
        /// <returns>string of HTTP response</returns>
        public static string HttpRequestString(HttpRequestSettings settings)
        {
            var client = new HttpUtilsWebClient();

            if (settings.Credentials != null)
                client.Credentials = settings.Credentials;

            if (settings.Proxy != null)
                client.Proxy = settings.Proxy;

            if (settings.Headers != null)
            {
                foreach (var header in settings.Headers)
                {
                    client.Headers[header.Key] = header.Value;
                }
            }

            if (settings.HttpVerb == "GET")
                settings.CapturedResponseContent = client.DownloadString(settings.Url);
            else
            {
                if (!string.IsNullOrEmpty(settings.ContentType))
                    client.Headers["Content-type"] = settings.ContentType;

                if (settings.Content is string)
                {
                    settings.CapturedRequestContent = settings.Content as string;
                    settings.CapturedResponseContent = client.UploadString(settings.Url, settings.HttpVerb, settings.CapturedRequestContent);
                }
                else if (settings.Content is byte[])
                {
                    settings.ResponseByteData = client.UploadData(settings.Url, settings.Content as byte[]);
                    settings.CapturedResponseContent = Encoding.UTF8.GetString(settings.ResponseByteData);
                }
                else
                    throw new ArgumentException("Data must be either string or byte[].");
            }

            settings.Response = client.Response;

            return settings.CapturedResponseContent;
        }


        /// <summary>
        /// Makes an HTTP with option JSON data serialized from an object
        /// and parses the result from JSON back into an object.
        /// Assumes that the service returns a JSON response
        /// </summary>
        /// <typeparam name="TResultType">The type of the object returned</typeparam>
        /// <param name="settings"><see cref="HttpRequestSettings"/>
        /// Configuration object for the HTTP request made to the server.
        /// </param>
        /// <returns>deserialized value/object from returned JSON data</returns>
        public static TResultType JsonRequest<TResultType>(HttpRequestSettings settings)
        {
            var client = new HttpUtilsWebClient();

            if (settings.Credentials != null)
                client.Credentials = settings.Credentials;

            if (settings.Proxy != null)
                client.Proxy = settings.Proxy;

            client.Headers.Add("Accept", "application/json");

            if (settings.Headers != null)
            {
                foreach (var header in settings.Headers)
                {
                    client.Headers[header.Key] = header.Value;
                }
            }

            string jsonResult;

            if (settings.HttpVerb == "GET")
                jsonResult = client.DownloadString(settings.Url);
            else
            {
                if (!string.IsNullOrEmpty(settings.ContentType))
                    client.Headers["Content-type"] = settings.ContentType;
                else
                    client.Headers["Content-type"] = "application/json";

                if (!settings.IsRawData)
                    settings.CapturedRequestContent = JsonSerializationUtils.Serialize(settings.Content, throwExceptions: true);
                else
                    settings.CapturedRequestContent = settings.Content as string;

                jsonResult = client.UploadString(settings.Url, settings.HttpVerb, settings.CapturedRequestContent);

                if (jsonResult == null)
                    return default(TResultType);
            }

            settings.CapturedResponseContent = jsonResult;
            settings.Response = client.Response;

            return (TResultType)JsonSerializationUtils.Deserialize(jsonResult, typeof(TResultType), true);
        }

#if !NET40
        /// <summary>
        /// Retrieves and Http request and returns data as a string.
        /// </summary>
        /// <param name="url">The Url to access</param>
        /// <returns>string of HTTP response</returns>
        public static async Task<string> HttpRequestStringAsync(string url)
        {
            return await HttpRequestStringAsync(new HttpRequestSettings() { Url = url });
        }


        /// <summary>
        /// Retrieves and Http request and returns data as a string.
        /// </summary>
        /// <param name="settings">Pass HTTP request configuration parameters object to set the URL, Verb, Headers, content and more</param>
        /// <returns>string of HTTP response</returns>
        public static async Task<string> HttpRequestStringAsync(HttpRequestSettings settings)
        {
            var client = new HttpUtilsWebClient();

            if (settings.Credentials != null)
                client.Credentials = settings.Credentials;

            if (settings.Proxy != null)
                client.Proxy = settings.Proxy;

            if (settings.Headers != null)
            {
                foreach (var header in settings.Headers)
                {
                    client.Headers[header.Key] = header.Value;
                }
            }

            if (settings.HttpVerb == "GET")
                settings.CapturedResponseContent = await client.DownloadStringTaskAsync(new Uri(settings.Url));
            else
            {
                if (!string.IsNullOrEmpty(settings.ContentType))
                    client.Headers["Content-type"] = settings.ContentType;

                if (settings.Content is string)
                {
                    settings.CapturedRequestContent = settings.Content as string;
                    settings.CapturedResponseContent = await client.UploadStringTaskAsync(settings.Url, settings.HttpVerb, settings.CapturedRequestContent);
                }
                else if (settings.Content is byte[])
                {
                    settings.ResponseByteData = await client.UploadDataTaskAsync(settings.Url, settings.Content as byte[]);
                    settings.CapturedResponseContent = Encoding.UTF8.GetString(settings.ResponseByteData);
                }
                else
                    throw new ArgumentException("Data must be either string or byte[].");
            }

            settings.Response = client.Response;

            return settings.CapturedResponseContent;
        }

        /// <summary>
        /// Makes an HTTP with option JSON data serialized from an object
        /// and parses the result from JSON back into an object.
        /// Assumes that the service returns a JSON response and that
        /// any data sent is json.
        /// </summary>
        /// <typeparam name="TResultType">The type of the object returned</typeparam>
        /// <param name="settings"><see cref="HttpRequestSettings"/>
        /// Configuration object for the HTTP request made to the server.
        /// </param>
        /// <returns>deserialized value/object from returned JSON data</returns>
        public static async Task<TResultType> JsonRequestAsync<TResultType>(HttpRequestSettings settings)
        {
            var client = new HttpUtilsWebClient();

            if (settings.Credentials != null)
                client.Credentials = settings.Credentials;

            if (settings.Proxy != null)
                client.Proxy = settings.Proxy;

            client.Headers.Add("Accept", "application/json");

            if (settings.Headers != null)
            {
                foreach (var header in settings.Headers)
                {
                    client.Headers[header.Key] = header.Value;
                }
            }

            string jsonResult;
            if (settings.HttpVerb == "GET")
                jsonResult = await client.DownloadStringTaskAsync(settings.Url);
            else
            {
                if (!string.IsNullOrEmpty(settings.ContentType))
                    client.Headers["Content-type"] = settings.ContentType;
                else
                    client.Headers["Content-type"] = "application/json";

                if (!settings.IsRawData)
                    settings.CapturedRequestContent = JsonSerializationUtils.Serialize(settings.Content, throwExceptions: true);
                else
                    settings.CapturedRequestContent = settings.Content as string;

                jsonResult = await client.UploadStringTaskAsync(settings.Url, settings.HttpVerb, settings.CapturedRequestContent);

                if (jsonResult == null)
                    return default(TResultType);
            }

            settings.CapturedResponseContent = jsonResult;
            settings.Response = client.Response;

            return (TResultType)JsonSerializationUtils.Deserialize(jsonResult, typeof(TResultType), true);
        }
#endif

    }


    /// <summary>
    /// Configuration object for Http Requests used by the HttpUtils
    /// methods. Allows you to set the URL, verb, headers proxy and
    /// credentials that are then passed to the HTTP client.
    /// </summary>
    public class HttpRequestSettings
    {
        /// <summary>
        /// The URL to send the request to
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// The HTTP verb to use when sending the request
        /// </summary>
        public string HttpVerb { get; set; }

        /// <summary>
        /// The Request content to send to the server.
        /// Data can be either string or byte[] type
        /// </summary>
        public object Content { get; set; }

        /// <summary>
        /// When true data is not translated. For example
        /// when using JSON Request if you want to send 
        /// raw POST data rather than a serialized object.
        /// </summary>
        public bool IsRawData { get; set; }

        /// <summary>
        /// The content type of any request data sent to the server
        /// in the Data property.
        /// </summary>
        public string ContentType { get; set; }

        /// <summary>
        /// Any Http request headers you want to set for this request
        /// </summary>
        public Dictionary<string, string> Headers { get; set; }

        /// <summary>
        /// Authentication information for this request
        /// </summary>
        public NetworkCredential Credentials { get; set; }

        /// <summary>
        /// An optional proxy to set for this request
        /// </summary>
        public WebProxy Proxy { get; set; }

        /// <summary>
        /// Capture request string data that was actually sent to the server.
        /// </summary>
        public string CapturedRequestContent { get; set; }

        /// <summary>
        /// Captured string Response Data from the server
        /// </summary>
        public string CapturedResponseContent { get; set; }

        /// <summary>
        /// Capture binary Response data from the server when 
        /// using the Data methods rather than string methods.
        /// </summary>
        public byte[] ResponseByteData { get; set; }

        /// <summary>
        /// The HTTP Status code of the HTTP response
        /// </summary>
        public HttpStatusCode ResponseStatusCode
        {
            get
            {
                if (Response != null)
                    return Response.StatusCode;

                return HttpStatusCode.OK;
            }
        }

        /// <summary>
        /// Instance of the full HttpResponse object that gives access
        /// to the full HttpWebResponse object to provide things
        /// like Response headers, status etc.
        /// </summary>
        public HttpWebResponse Response { get; set; }


        public HttpRequestSettings()
        {
            HttpVerb = "GET";
            Headers = new Dictionary<string, string>();
        }
    }
}
