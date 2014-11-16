using System;
using System.Collections.Generic;
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

        public static string HttpRequestString(HttpHelperRequestSettings settings)
        {
            var client = new WebClient();

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
                settings.ResponseData = client.DownloadString(settings.Url);
            else
            {
                if (!string.IsNullOrEmpty(settings.ContentType))
                    client.Headers["Content-type"] = settings.ContentType;

                if (settings.Data is string)
                {
                    settings.RequestData = settings.Data as string;
                    settings.ResponseData = client.UploadString(settings.Url, settings.HttpVerb, settings.RequestData);
                }
                else if(settings.Data is byte[])
                {                    
                    settings.ResponseByteData = client.UploadData(settings.Url, settings.Data as byte[]);
                    settings.ResponseData = Encoding.UTF8.GetString(settings.ResponseByteData);
                }
                else
                    throw new ArgumentException("Data must be either string or byte[].");
            }

            return settings.ResponseData;
        }

        public static async Task<string> WebRequestStringAsync(HttpHelperRequestSettings settings)
        {

        }


        /// <summary>
        /// Makes an HTTP with option JSON data serialized from an object
        /// and parses the result from JSON back into an object.
        /// Assumes that the service returns a JSON response
        /// </summary>
        /// <typeparam name="TResultType">The type of the object returned</typeparam>
        /// <param name="settings"><see cref="Westwind.Utilities.HttpHelperRequestSettings"/>
        /// Configuration object for the HTTP request made to the server.
        /// </param>
        /// <returns>deserialized value/object from returned JSON data</returns>
        public static TResultType JsonRequest<TResultType>(HttpHelperRequestSettings settings)
        {
            var client = new WebClient();

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

                settings.RequestData = JsonSerializationUtils.Serialize(settings.Data, throwExceptions: true);
                jsonResult = client.UploadString(settings.Url, settings.HttpVerb, settings.RequestData);

                if (jsonResult == null)
                    return default(TResultType);
            }

            settings.ResponseData = jsonResult;
            return (TResultType) JsonSerializationUtils.Deserialize(jsonResult, typeof (TResultType), true);
        }

        /// <summary>
        /// Makes an HTTP with option JSON data serialized from an object
        /// and parses the result from JSON back into an object.
        /// Assumes that the service returns a JSON response and that
        /// any data sent is json.
        /// </summary>
        /// <typeparam name="TResultType">The type of the object returned</typeparam>
        /// <param name="settings"><see cref="Westwind.Utilities.HttpHelperRequestSettings"/>
        /// Configuration object for the HTTP request made to the server.
        /// </param>
        /// <returns>deserialized value/object from returned JSON data</returns>
        public static async Task<TResultType> JsonRequestAsync<TResultType>(HttpHelperRequestSettings settings)
        {
            var client = new WebClient();

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

                settings.RequestData = JsonSerializationUtils.Serialize(settings.Data, throwExceptions: true);
                jsonResult = await client.UploadStringTaskAsync(settings.Url, settings.HttpVerb, settings.RequestData);

                if (jsonResult == null)
                    return default(TResultType);
            }

            settings.ResponseData = jsonResult;
            return (TResultType) JsonSerializationUtils.Deserialize(jsonResult, typeof (TResultType), true);
        }
    }


    /// <summary>
    /// Configuration object for Http Requests used by the HttpUtils
    /// methods. Allows you to set the URL, verb, headers proxy and
    /// credentials that are then passed to the HTTP client.
    /// </summary>
    public class HttpHelperRequestSettings
    {
        public string Url { get; set; }
        public object Data { get; set; }
        public string HttpVerb { get; set; }
        public string ContentType { get; set; }
        public Dictionary<string, string> Headers { get; set; }
        public NetworkCredential Credentials { get; set; }
        public WebProxy Proxy { get; set; }
        public string RequestData { get; set; }
        public string ResponseData { get; set; }
        public byte[] ResponseByteData { get; set; }

        public HttpHelperRequestSettings()
        {
            HttpVerb = "GET";
            Headers = new Dictionary<string, string>();
        }
    }
}
