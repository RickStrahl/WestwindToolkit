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
using System.Web.UI;
using System.IO;
using System.Reflection;
using Westwind.Utilities;

[assembly: WebResource("Westwind.Web.Resources.ww.jquery.js", "application/x-javascript")]

namespace Westwind.Web
{
    /// <summary>
    /// Class is used as to consolidate access to resources
    /// </summary>
    public class WebResources
    {
        /* Embedded Script Resources */
        public const string JQUERY_SCRIPT_RESOURCE = "Westwind.Web.Resources.jquery.js";
        public const string WWJQUERY_SCRIPT_RESOURCE = "Westwind.Web.Resources.ww.jquery.js";

        ///*  Icon Resource Strings */
        //public const string INFO_ICON_RESOURCE = "Westwind.Web.WebForms.Resources.info.gif";
        //public const string WARNING_ICON_RESOURCE = "Westwind.Web.WebForms.Resources.warning.gif";
        //public const string CLOSE_ICON_RESOURCE = "Westwind.Web.WebForms.Resources.close.gif";
        //public const string HELP_ICON_RESOURCE = "Westwind.Web.WebForms.Resources.help.gif";
        //public const string LOADING_ICON_RESOURCE = "Westwind.Web.WebForms.Resources.loading.gif";
        //public const string LOADING_SMALL_ICON_RESOURCE = "Westwind.Web.WebForms.Resources.loading_small.gif";
        //public const string CALENDAR_ICON_RESOURCE = "Westwind.Web.WebForms.Resources.calendar.gif";

        /* Content Types */
        public const string STR_JsonContentType = "application/json";
        public const string STR_JavaScriptContentType = "application/x-javascript";
        public const string STR_UrlEncodedContentType = "application/x-www-form-urlencoded";
        public const string STR_XmlContentType = "text/xml";
        public const string STR_XmlApplicationContentType = "application/xml";

         
        /// <summary>
        /// Returns a string resource from a given assembly.
        /// </summary>
        /// <param name="assembly">Assembly reference (ie. typeof(ControlResources).Assembly) </param>
        /// <param name="ResourceName">Name of the resource to retrieve</param>
        /// <returns></returns>
        public static string GetStringResource(Assembly assembly, string ResourceName)
        {
            Stream st = assembly.GetManifestResourceStream(ResourceName);
            StreamReader sr = new StreamReader(st);
            string content = sr.ReadToEnd();
            st.Close();
            return content;
        }


        /// <summary>
        /// Returns a string resource from the from the ControlResources Assembly
        /// </summary>
        /// <param name="ResourceName"></param>
        /// <returns></returns>
        public static string GetStringResource(string ResourceName)
        {
            return GetStringResource(typeof(WebResources).Assembly, ResourceName);
        }        
    }
}
