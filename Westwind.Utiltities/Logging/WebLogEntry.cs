#region License
/*
 **************************************************************
 *  Author: Rick Strahl 
 *          © West Wind Technologies, 2009
 *          http://www.west-wind.com/
 * 
 * Created: 09/12/2009
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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Net;

namespace Westwind.Utilities.Logging
{
    /// <summary>
    /// A Web specific Log entry that includes information about the current Web Request
    /// </summary>
    public class WebLogEntry : LogEntry 
    {
        public WebLogEntry() : base() { } 
        public WebLogEntry(Exception ex) : base(ex) {}
        public WebLogEntry(Exception ex, HttpContext context) : base(ex) 
        {
            UpdateFromRequest(context);
        }

        /// <summary>
        /// The Url without the query string for the current request
        /// </summary>
        public string Url
        {
            get { return _Url; }
            set { _Url = value; }
        }
        private string _Url = "";

        
        /// <summary>
        /// The query string of the current request
        /// </summary>
        public string QueryString
        {
            get { return _QueryString; }
            set { _QueryString = value; }
        }
        private string _QueryString = "";

        /// <summary>
        /// The IP Address of the client that called this URL
        /// </summary>
        public string IpAddress
        {
            get { return _IpAddress; }
            set { _IpAddress = value; }
        }
        private string _IpAddress = "";


        /// <summary>
        /// The POST data if available
        /// </summary>
        public string PostData
        {
            get { return _PostData; }
            set { _PostData = value; }
        }
        private string _PostData = "";

        /// <summary>
        /// The Referring url
        /// </summary>
        public string Referrer
        {
            get { return _Referrer; }
            set { _Referrer = value; }
        }
        private string _Referrer = "";

        
        public string UserAgent
        {
            get { return _UserAgent; }
            set { _UserAgent = value; }
        }
        private string _UserAgent = "";


        /// <summary>
        /// Optional duration of the current request
        /// </summary>
        public decimal RequestDuration
        {
            get { return _RequestDuration; }
            set { _RequestDuration = value; }
        }
        private decimal _RequestDuration = 0M;


        public bool UpdateFromRequest()
        {
            return UpdateFromRequest(HttpContext.Current);            
        }
        /// <summary>
        /// Updates the Web specific properties of this entry from the 
        /// supplied HttpContext object.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public bool UpdateFromRequest(HttpContext context)
        {
            if (context == null)
                context = HttpContext.Current;

            if (context == null)
                return false;

            HttpRequest request = context.Request;

            IpAddress = request.UserHostAddress;
            Url = request.FilePath;
            QueryString = request.QueryString.ToString();
            
            if (request.UrlReferrer != null)
                Referrer = request.UrlReferrer.ToString();
            UserAgent = request.UserAgent;

            if (request.TotalBytes > 0 && request.TotalBytes < 2048)
            {
                PostData = Encoding.Default.GetString(request.BinaryRead(request.TotalBytes));                
            }
            else if (request.TotalBytes > 2048)  // strip the result
            {
                PostData = Encoding.Default.GetString(request.BinaryRead(2040)) + "...";                
            }

            return true;
        }
    }    
}
