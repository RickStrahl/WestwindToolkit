using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace System.Web
{
    /// <summary>
    /// HttpResponse Extension methods to facilitate various output tasks
    /// </summary>
    public static class HttpResponseExtensions
    {

        /// <summary>
        /// Writes output as an HTML 'line' by appending a &lt;br /&gt; and linefeed at the end
        /// </summary>
        /// <param name="response"></param>
        /// <param name="output">String to output</param>
        public static void WriteLine(this HttpResponse response, object output)
        {
            response.Write(output + " <br />\r\n");            
        }

        /// <summary>
        /// Writes formatted output as an HTML 'line' by appending a &lt;br /&gt; and linefeed at the end
        /// </summary>
        /// <param name="response">HttpResponse object</param>
        /// <param name="format">format string</param>
        /// <param name="args">format string arguments</param>
        public static void WriteLine(this HttpResponse response, string format, params object[] args)
        {
            response.Write( string.Format(format, args) + " <br />\r\n");
        }

        /// <summary>
        /// Writes formatted output into the Response
        /// </summary>
        /// <param name="response">HttpResponse object</param>
        /// <param name="format">format string</param>
        /// <param name="args">format string arguments</param>
        public static void Write(this HttpResponse response, string format, params object[] args)
        {
            response.Write(string.Format(format, args) );
        }
    }
}
