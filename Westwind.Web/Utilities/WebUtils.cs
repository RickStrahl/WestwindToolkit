#region License
/*
 **************************************************************
 *  Author: Rick Strahl 
 *          © West Wind Technologies, 2008 - 2011
 *          http://www.west-wind.com/
 * 
 * Created: Date
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
using System.Data;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using Westwind.Web;
using Westwind.Web.Properties;
using Westwind.Web.JsonSerializers;

namespace Westwind.Utilities
{
    /// <summary>
    /// Summary description for wwWebUtils.
    /// </summary>
    public static class WebUtils
    {

        static DateTime DAT_JAVASCRIPT_BASEDATE = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);


        #region Url Management
        /// <summary>
        /// Returns a site relative HTTP path from a partial path starting out with a ~.
        /// Same syntax that ASP.Net internally supports but this method can be used
        /// outside of the Page framework.
        /// 
        /// Works like Control.ResolveUrl including support for ~ syntax
        /// but returns an absolute URL.
        /// </summary>
        /// <param name="originalUrl">Any Url including those starting with ~ for virtual base path replacement</param>
        /// <returns>relative url</returns>
        /// <remarks>
        /// Returns the path as relative of current location (ie. ./link.htm) if 
        /// HttpContext is not available. Note that this may result in some scenarios where
        /// an invalid URL is returned if HttpContext is not present, but it allows for test
        /// scenarios.
        /// </remarks>
        public static string ResolveUrl(string originalUrl)
        {
            if (originalUrl == null)
                return null;

            // Fix up image path for ~ root app dir directory
            if (originalUrl.StartsWith("~"))
            {
                // This is unreliable for some URLs and doesn't work if HttpContext is not available
                //return VirtualPathUtility.ToAbsolute(originalUrl);

                string newUrl = "";
                if (HttpContext.Current != null)
                    newUrl = HttpContext.Current.Request.ApplicationPath +
                          originalUrl.Substring(1);
                else
                    // no context - return assume ~ is off current
                    // this will fail in some instances but hopefully work in most
                    newUrl = "./" + originalUrl.Substring(1);

                // No Context - throw
                //throw new ArgumentException(Resources.InvalidURLRelativeURLNotAllowed);              

                // make sure that we didn't add a double // at the beginning
                newUrl = newUrl.Replace("//", "/");

                return newUrl;
            }

            return originalUrl;
        }


        /// <summary>
        /// This method returns a fully qualified absolute server Url which includes
        /// the protocol, server, port in addition to the server relative Url.
        /// 
        /// Works like Control.ResolveUrl including support for ~ syntax
        /// but returns an absolute URL.
        /// </summary>
        /// <param name="ServerUrl">Any Url, either App relative (~/default.aspx) 
        /// or fully qualified</param>
        /// <param name="forceHttps">if true forces the url to use https</param>
        /// <returns></returns>
        public static string ResolveServerUrl(string serverUrl, bool forceHttps = false)
        {
            // Is it already an absolute Url?
            if (serverUrl.IndexOf("://") < 0)
            {
                // Start by fixing up the Url an Application relative Url
                string relPath = ResolveUrl(serverUrl);

                serverUrl = HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Authority);
                serverUrl += relPath;
            }

            if (forceHttps)
                serverUrl = serverUrl.Replace("http://", "https://");

            return serverUrl;
        }

        /// <summary>
        /// This method returns a fully qualified absolute server Url which includes
        /// the protocol, server, port in addition to the server relative Url.
        /// 
        /// It work like Page.ResolveUrl, but adds these to the beginning.
        /// This method is useful for generating Urls for AJAX methods
        /// </summary>
        /// <param name="ServerUrl">Any Url, either App relative or fully qualified</param>
        /// <returns></returns>
        public static string ResolveServerUrl(string serverUrl)
        {
            return ResolveServerUrl(serverUrl, false);
        }

        /// <summary>
        /// Returns the Application Path as a full Url with scheme 
        /// </summary>
        /// <returns></returns>
        public static string GetFullApplicationPath()
        {
            var url = HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Authority);
            return url + HttpContext.Current.Request.ApplicationPath.TrimEnd('/');
        }


        /// <summary>
        /// Returns the executing ASPX, ASCX, MASTER page for a control instance.
        /// Path is returned app relative without a leading slash
        /// </summary>
        /// <param name="Ctl"></param>
        /// <returns></returns>
        public static string GetControlAppRelativePath(Control Ctl)
        {
            return Ctl.TemplateControl.AppRelativeVirtualPath.Replace("~/", "");
        }

        /// <summary>
        /// Returns just the Path of a full Url. Strips off the filename and querystring
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static string GetUrlPath(string url)
        {
            int lnAt = url.LastIndexOf("/");
            if (lnAt > 0)
            {
                return url.Substring(0, lnAt + 1);
            }
            return "/";
        }

        /// <summary>
        /// Translates an ASP.NET path like /myapp/subdir/page.aspx 
        /// into an application relative path: subdir/page.aspx. The
        /// path returned is based of the application base and 
        /// starts either with a subdirectory or page name (ie. no ~)
        /// 
        /// The path is turned into all lower case.
        /// </summary>
        /// <param name="logicalPath">A logical, server root relative path (ie. /myapp/subdir/page.aspx)</param>
        /// <returns>Application relative path (ie. subdir/page.aspx)</returns>
        public static string GetAppRelativePath(string logicalPath)
        {
            logicalPath = logicalPath.ToLower();

            string appPath = string.Empty;

            if (HttpContext.Current != null)
            {
                appPath = HttpContext.Current.Request.ApplicationPath.ToLower();
                if (appPath != "/")
                    appPath += "/";
                else
                    // Root web relative path is empty - strip off leading slash
                    return logicalPath.TrimStart('/');
            }
            else
            {
                // design time compiler for stock web projects will treat as root web
                return logicalPath.TrimStart('/');
            }

            return logicalPath.Replace(appPath, "");
        }

        /// <summary>
        /// Translates the current ASP.NET path  
        /// into an application relative path: subdir/page.aspx. The
        /// path returned is based of the application base and 
        /// starts either with a subdirectory or page name (ie. no ~)
        /// 
        /// This version uses the current ASP.NET path of the request
        /// that is active and internally uses AppRelativeCurrentExecutionFilePath
        /// </summary>
        /// <returns></returns>
        public static string GetAppRelativePath()
        {
            return HttpContext.Current.Request.AppRelativeCurrentExecutionFilePath.Replace("~/", "");
        }

        /// <summary>
        /// Inserts a &lt;script&gt; source link into the page.
        /// Includes carriage return at end.
        /// </summary>
        /// <param name="url">The url to the script to load. Url is resolved</param>
        /// <returns>full script tag</returns>
        public static string ScriptLink(string url)
        {
            return "<script type=\"text/javascript\" src=\"" + ResolveUrl(url) + "></script>\r\n";
        }

        ///// <summary>
        ///// Injects a full jQuery script link and CDN fallback (if using CDN) into the page.
        ///// Version information and CDN Urls are based on the static settings in the <see cref="System.Web.ControlResources"/> class.
        ///// </summary>
        ///// <param name="mode">Optional: Determines where jQuery is loaded from (CDN, WebResources, Script)</param>        
        ///// <param name="url">Optional url from where to load jQuery</param>
        //public static string jQueryLink( jQueryLoadModes jQueryLoadMode = jQueryLoadModes.Default, string url = null )
        //{
        //    return ScriptLoader.jQueryLink(jQueryLoadMode,url);
        //}

        /// <summary>
        /// Inserts a CSS &lt;link&gt; tag into the page.
        /// Includes carriage return at end.
        /// </summary>
        /// <param name="url">The url to the CSS file to load. Url is resolved</param>
        /// <returns>full CSS link tag</returns>

        public static string CssLink(string url)
        {
            return "<link href=\"" + ResolveUrl(url) + " rel=\"stylesheet\" type=\"text/css\" />\r\n";
        }

        /// <summary>
        /// Creates the headers required to force the current request to not go into 
        /// the client side cache, forcing a reload of the page.
        /// 
        /// This method can be called anywhere as part of the Response processing to 
        /// modify the headers. Use this for any non POST pages that should never be 
        /// cached.
        /// <seealso>Class WebUtils</seealso>
        /// </summary>
        /// <param name="Response"></param>
        /// <returns>Void</returns>
        public static void ForceReload()
        {
            HttpResponse Response = HttpContext.Current.Response;
            Response.Expires = 0;
            //Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.AppendHeader("Pragma", "no-cache");
            Response.AppendHeader("Cache-Control", "no-cache, mustrevalidate");
        }

        #endregion


        #region Form Variables
        /// <summary>
        /// Checks to see if a form variable exists in the 
        /// HttpContext. Only works in System.Web based applications
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static bool IsFormVar(string key)
        {
            if (HttpContext.Current == null)
                return false;

            string val = HttpContext.Current.Request.Form[key];
            return !string.IsNullOrEmpty(val);
        }

        /// <summary>
        /// Routine that can be used read Form Variables into an object if the 
        /// object name and form variable names match either exactly or with a specific
        /// prefix.
        /// 
        /// The method loops through all *public* members of the object and tries to 
        /// find a matching form variable by the same name in Request.Form.
        /// 
        /// The routine returns false if any value failed to parse (ie. invalid
        /// formatting etc.). However parsing is not aborted on errors so all
        /// other convertable values are set on the object.
        /// 
        /// You can pass in a Dictionary<string,string> for the Errors parameter
        /// to optionally retrieve unbinding errors. The dictionary key holds the
        /// simple form varname for the field (ie. txtName), the value the actual
        /// exception error message
        /// </summary>
        /// <remarks>
        /// This method can have unexpected side-effects if multiple naming
        /// containers share common variable names. This routine is not recommended
        /// for those types of pages.
        /// </remarks>
        /// <param name="Target"></param>
        /// <param name="FormVarPrefix">empty or one or more prefixes spearated by |</param>
        /// <param name="errors">Allows passing in a string dictionary that receives error messages. returns key as field name, value as error message</param>
        /// <returns>true or false if an unbinding error occurs</returns>
        public static bool FormVarsToObject(object target, string formvarPrefixes = null, Dictionary<string, string> errors = null)
        {
            bool isError = false;
            List<string> ErrorList = new List<string>();

            if (formvarPrefixes == null)
                formvarPrefixes = "";

            if (HttpContext.Current == null)
                throw new InvalidOperationException("FormVarsToObject can only be called from a Web Request");

            HttpRequest Request = HttpContext.Current.Request;

            // try to get a generic reference to a page for recursive find control
            // This value will be null if not dealing with a page (ie. in JSON Web Service)
            Page page = HttpContext.Current.CurrentHandler as Page;

            MemberInfo[] miT = target.GetType().FindMembers(
                MemberTypes.Field | MemberTypes.Property,
                BindingFlags.Public | BindingFlags.Instance,
                null, null);

            // Look through all prefixes separated by |
            string[] prefixes = formvarPrefixes.Split('|');

            foreach (string prefix in prefixes)
            {

                // Loop through all members of the Object
                foreach (MemberInfo Field in miT)
                {
                    string Name = Field.Name;

                    FieldInfo fi = null;
                    PropertyInfo pi = null;
                    Type FieldType = null;

                    if (Field.MemberType == MemberTypes.Field)
                    {
                        fi = (FieldInfo)Field;
                        FieldType = fi.FieldType;
                    }
                    else
                    {
                        pi = (PropertyInfo)Field;
                        FieldType = pi.PropertyType;
                    }

                    // Lookup key will be field plus the prefix
                    string formvarKey = prefix + Name;

                    // Try a simple lookup at the root first
                    var strValue = Request.Form[formvarKey];

                    // if not found try to find the control and then
                    // use its UniqueID for lookup instead
                    if (strValue == null && page != null)
                    {
                        Control ctl = WebUtils.FindControlRecursive(page, formvarKey);
                        if (ctl != null)
                            strValue = Request.Form[ctl.UniqueID];
                    }

                    // Bool values and checkboxes might require special handling
                    if (strValue == null)
                    {
                        // Must handle checkboxes/radios
                        if (FieldType == typeof(bool))
                            strValue = "false";
                        // other values that are null are not updated
                        else
                            continue;
                    }

                    try
                    {
                        // Convert the value to it target type
                        object Value = ReflectionUtils.StringToTypedValue(strValue, FieldType);

                        // Assign it to the object property/field
                        if (Field.MemberType == MemberTypes.Field)
                            fi.SetValue(target, Value);
                        else
                            pi.SetValue(target, Value, null);
                    }
                    catch (Exception ex)
                    {
                        isError = true;
                        if (errors != null)
                            errors.Add(Field.Name, ex.Message);
                    }
                }
            }

            return !isError;
        }


        /// <summary>
        /// Routine that retrieves form variables for each row in a dataset that match
        /// the fieldname or the field name with a prefix.
        /// The routine returns false if any value failed to parse (ie. invalid
        /// formatting etc.). However parsing is not aborted on errors so all
        /// other convertable values are set on the object.
        /// 
        /// You can pass in a Dictionary<string,string> for the Errors parameter
        /// to optionally retrieve unbinding errors. The dictionary key holds the
        /// simple form varname for the field (ie. txtName), the value the actual
        /// exception error message
        /// <seealso>Class wwWebUtils</seealso>
        /// </summary>
        /// <param name="loRow">
        /// A DataRow object to load up with values from the Request.Form[] collection.
        /// </param>
        /// <param name="Prefix">
        /// Optional prefix of form vars. For example, "txtCompany" has a "txt" prefix 
        /// to map to the "Company" field. Specify multiple prefixes and separate with |
        /// Leave blank or null for no prefix.
        /// </param>
        /// <param name="errors">
        /// An optional Dictionary that returns an error list. Dictionary is
        /// has a string key that is the name of the field and a value that describes the error.
        /// Errors are binding errors only.
        /// </param>
        public static bool FormVarsToDataRow(DataRow dataRow, string formvarPrefixes, Dictionary<string, string> errors)
        {
            bool isError = false;

            if (HttpContext.Current == null)
                throw new InvalidOperationException("FormVarsToObject can only be called from a Web Request");

            HttpRequest Request = HttpContext.Current.Request;

            // try to get a generic reference to a page for recursive find control
            // This value will be null if not dealing with a page (ie. in JSON Web Service)
            Page page = HttpContext.Current.CurrentHandler as Page;


            if (formvarPrefixes == null)
                formvarPrefixes = "";

            DataColumnCollection columns = dataRow.Table.Columns;

            // Look through all prefixes separated by |
            string[] prefixes = formvarPrefixes.Split('|');

            foreach (string prefix in prefixes)
            {
                foreach (DataColumn column in columns)
                {
                    string Name = column.ColumnName;

                    // Lookup key will be field plus the prefix
                    string formvarKey = prefix + Name;

                    // Try a simple lookup at the root first
                    string strValue = Request.Form[prefix + Name];

                    // if not found try to find the control and then
                    // use its UniqueID for lookup instead
                    if (strValue == null && page != null)
                    {
                        Control ctl = WebUtils.FindControlRecursive(page, formvarKey);
                        if (ctl != null)
                            strValue = Request.Form[ctl.UniqueID];
                    }

                    // Bool values and checkboxes might require special handling
                    if (strValue == null)
                    {
                        // Must handle checkboxes/radios
                        if (column.DataType == typeof(Boolean))
                            strValue = "false";
                        else
                            continue;
                    }

                    try
                    {
                        object value = ReflectionUtils.StringToTypedValue(strValue, column.DataType);
                        dataRow[Name] = value;
                    }
                    catch (Exception ex)
                    {
                        isError = true;
                        if (errors != null)
                            errors.Add(Name, ex.Message);
                    }

                }
            }

            return !isError;
        }

        /// <summary>
        /// Retrieves a value by key from a UrlEncoded string.
        /// </summary>
        /// <param name="urlEncodedString">UrlEncoded String</param>
        /// <param name="key">Key to retrieve value for</param>
        /// <returns>returns the value or "" if the key is not found or the value is blank</returns>
        public static string GetUrlEncodedKey(string urlEncodedString, string key)
        {
            string res = StringUtils.ExtractString("&" + urlEncodedString, "&" + key + "=", "&", false, true);
            return HttpUtility.UrlDecode(res);
        }

        /// <summary>
        /// Returns a request value parsed into an integer. If the value is not found
        /// or not a number null is returned.
        /// </summary>
        /// <param name="paramsKey">The request key to retrieve</param>        
        /// <returns>parsed integer or null on failure</returns>
        public static int? GetParamsInt(string paramsKey)
        {
            string val = HttpContext.Current.Request.Params[paramsKey];
            if (val == null)
                return null;

            int ival = 0;
            if (!int.TryParse(val, out ival))
                return null;

            return ival;
        }

        /// <summary>
        /// Returns a request value parsed into an integer with an optional 
        /// default value set if the conversion fails.
        /// </summary>
        /// <param name="paramsKey"></param>
        /// <param name="defaultValue">defaults to -1</param>
        /// <returns></returns>
        public static int GetParamsInt(string paramsKey, int defaultValue = -1)
        {
            string val = HttpContext.Current.Request.Params[paramsKey];
            if (val == null)
                return defaultValue;

            int ival = defaultValue;
            if (!int.TryParse(val, out ival))
                return defaultValue;

            return ival;
        }

        /// <summary>
        /// Returns the content of the POST buffer as string
        /// </summary>
        /// <returns></returns>
        public static string FormBufferToString()
        {
            HttpRequest Request = HttpContext.Current.Request;

            if (Request.TotalBytes > 0)
                return Encoding.Default.GetString(Request.BinaryRead(Request.TotalBytes));

            return string.Empty;
        }

        #endregion


        #region String Helpers

        /// <summary>
        /// Returns the result from an ASPX 'template' page in the /templates directory of this application.
        /// This method uses an HTTP client to call into the Web server and retrieve the result as a string.
        /// </summary>
        /// <param name="templatePageAndQueryString">The name of a page (ASPX, HTM etc.)  to retrieve plus the querystring
        /// Examples: webform1.aspx, subfolder/WebForm1.aspx, ~/WebForm1.aspx,/myVirtual/WebForm1.aspx
        /// </param>
        /// <param name="errorMessage">If this method returns null this message will contain the error info</param>
        /// <returns>Merged Text or null if an HTTP error occurs - note: could also return an Error page HTML result if the template page has an error.</returns>
        public static string AspTextMerge(string templatePageAndQueryString, ref string errorMessage)
        {
            string MergedText = "";

            // Save the current request information
            HttpContext Context = HttpContext.Current;

            // Now call the other page and load into StringWriter
            StringWriter sw = new StringWriter();
            try
            {
                // IMPORTANT: Child page's FilePath still points at current page
                //            QueryString provided is mapped into new page and then reset
                Context.Server.Execute(templatePageAndQueryString, sw);
                MergedText = sw.ToString();
            }
            catch (Exception ex)
            {
                MergedText = null;
                errorMessage = ex.Message;
            }

            return MergedText;
        }


        static string HtmlSanitizeTagBlackList { get; } = "script|iframe|object|embed|form";

        static Regex _RegExScript = new Regex(
            $@"(<({HtmlSanitizeTagBlackList})\b[^<]*(?:(?!<\/({HtmlSanitizeTagBlackList}))<[^<]*)*<\/({HtmlSanitizeTagBlackList})>)",
            RegexOptions.IgnoreCase | RegexOptions.Multiline);

        static Regex _RegExJavaScriptHref = new Regex(
            @"<.*?(href|src|dynsrc|lowsrc)=.{0,10}(javascript:).*?>",
            RegexOptions.IgnoreCase | RegexOptions.Multiline);

        static Regex _RegExOnEventAttributes = new Regex(
            @"<.*?\s(on.{4,12}=([""].*?[""]|['].*?['])).*?(>|\/>)",
            RegexOptions.IgnoreCase | RegexOptions.Multiline);

        /// <summary>
        /// Sanitizes HTML to some of the most of 
        /// </summary>
        /// <remarks>
        /// This provides rudimentary HTML sanitation catching the most obvious
        /// XSS script attack vectors. For mroe complete HTML Sanitation please look into
        /// a dedicated HTML Sanitizer.
        /// </remarks>
        /// <param name="html">input html</param>
        /// <param name="htmlTagBlacklist">A list of HTML tags that are stripped.</param>
        /// <returns>Sanitized HTML</returns>
        public static string SanitizeHtml(string html, string htmlTagBlacklist = "script|iframe|object|embed|form")
        {
            if (string.IsNullOrEmpty(html))
                return html;

            if (!string.IsNullOrEmpty(htmlTagBlacklist) || htmlTagBlacklist == HtmlSanitizeTagBlackList)
            {
                // Replace Script tags - reused expr is more efficient
                html = _RegExScript.Replace(html, string.Empty);
            }
            else
            {
                html = Regex.Replace(html,
                                      $@"(<({htmlTagBlacklist})\b[^<]*(?:(?!<\/({HtmlSanitizeTagBlackList}))<[^<]*)*<\/({htmlTagBlacklist})>)",
                                      "", RegexOptions.IgnoreCase | RegexOptions.Multiline);
            }

            // Remove javascript: directives
            var matches = _RegExJavaScriptHref.Matches(html);
            foreach (Match match in matches)
            {
                var txt = StringUtils.ReplaceString(match.Value, "javascript:", "unsupported:", true);
                html = html.Replace(match.Value, txt);
            }

            // Remove onEvent handlers from elements
            matches = _RegExOnEventAttributes.Matches(html);
            foreach (Match match in matches)
            {
                var txt = match.Value;
                if (match.Groups.Count > 1)
                {
                    var onEvent = match.Groups[1].Value;
                    txt = txt.Replace(onEvent, string.Empty);
                    if (!string.IsNullOrEmpty(txt))
                        html = html.Replace(match.Value, txt);
                }
            }

            return html;
        }

        /// <summary>
        /// Parses a Carriage Return based into a &lt;ul&gt; style HTML list by 
        /// splitting each carriage return separated line.
        /// <seealso>Class WebUtils</seealso>
        /// </summary>
        /// <param name="text">
        /// The carriage return separated text list
        /// </param>
        /// <returns>string</returns>
        public static string TextListToHtmlList(string text)
        {
            string[] TextStrings = text.Split(new char[1] { '\r' }, StringSplitOptions.RemoveEmptyEntries);

            StringBuilder sb = new StringBuilder();
            foreach (string str in TextStrings)
            {
                if (str == "\n")
                    continue;

                sb.Append("<li>" + str + "</li>\r\n");
            }

            sb.Append("</ul>");
            return "<ul>" + sb.ToString();
        }

        #endregion

        #region WebForms Rendering
        /// <summary>
        /// Renders a control to a string - useful for AJAX return values
        /// </summary>
        /// <param name="control"></param>
        /// <returns></returns>
        public static string RenderControl(Control control)
        {
            StringWriter tw = new StringWriter();

            // Simple rendering - just write the control to the text writer
            // works well for single controls without containers
            Html32TextWriter writer = new Html32TextWriter(tw);
            control.RenderControl(writer);
            writer.Close();

            return tw.ToString();
        }

        /// <summary>
        /// Renders a control dynamically by creating a new Page and Form
        /// control and then adding the control to be rendered to it.        
        /// </summary>
        /// <remarks>
        /// This routine works to render most Postback controls but it
        /// has a bit of overhead as it creates a separate Page instance        
        /// </remarks>
        /// <param name="control">The control that is to be rendered</param>
        /// <param name="useDynamicPage">if true forces a Page to be created</param>
        /// <returns>Html or empty</returns>
        public static string RenderControl(Control control, bool useDynamicPage)
        {
            if (!useDynamicPage)
                return RenderControl(control);

            const string STR_BeginRenderControlBlock = "<!-- BEGIN RENDERCONTROL BLOCK -->";
            const string STR_EndRenderControlBlock = "<!-- End RENDERCONTROL BLOCK -->";

            StringWriter tw = new StringWriter();

            // Create a page and form so that postback controls work          
            Page page = new Page();
            page.EnableViewState = false;

            HtmlForm form = new HtmlForm();
            form.ID = "__t";
            page.Controls.Add(form);

            // Add placeholder to strip out so we get just the control rendered
            // and not the <form> tag and viewstate, postback script etc.
            form.Controls.Add(new LiteralControl(STR_BeginRenderControlBlock + "."));
            form.Controls.Add(control);
            form.Controls.Add(new LiteralControl("." + STR_EndRenderControlBlock));

            HttpContext.Current.Server.Execute(page, tw, true);

            string Html = tw.ToString();

            // Strip out form and ViewState, Event Validation etc.
            int at1 = Html.IndexOf(STR_BeginRenderControlBlock);
            int at2 = Html.IndexOf(STR_EndRenderControlBlock);
            if (at1 > -1 && at2 > at1)
            {
                Html = Html.Substring(at1 + STR_BeginRenderControlBlock.Length);
                Html = Html.Substring(0, at2 - at1 - STR_BeginRenderControlBlock.Length);
            }

            return Html;
        }

        /// <summary>
        /// Renders a user control into a string into a string.
        /// </summary>
        /// <param name="page">Instance of the page that is hosting the control</param>
        /// <param name="userControlVirtualPath"></param>
        /// <param name="includePostbackControls">If false renders using RenderControl, otherwise uses Server.Execute() constructing a new form.</param>
        /// <param name="data">Optional Data parameter that can be passed to the User Control IF the user control has a Data property.</param>
        /// <returns></returns>
        public static string RenderUserControl(string userControlVirtualPath,
                                               bool includePostbackControls,
                                               object data)
        {
            const string STR_NoUserControlDataProperty = "Passed a Data parameter to RenderUserControl, but the user control has no public Data property.";
            const string STR_BeginRenderControlBlock = "<!-- BEGIN RENDERCONTROL BLOCK -->";
            const string STR_EndRenderControlBlock = "<!-- End RENDERCONTROL BLOCK -->";

            StringWriter tw = new StringWriter();
            Control control = null;

            if (!includePostbackControls)
            {
                // Simple rendering works if no post back controls are used
                Page curPage = (Page)HttpContext.Current.CurrentHandler;
                control = curPage.LoadControl(userControlVirtualPath);
                if (data != null)
                {
                    try
                    {
                        ReflectionUtils.SetProperty(control, "Data", data);
                    }
                    catch
                    {
                        throw new ArgumentException(STR_NoUserControlDataProperty);
                    }
                }
                return RenderControl(control);
            }

            // Create a page and form so that postback controls work          
            Page page = new Page();
            page.EnableViewState = false;

            // IMPORTANT: Control must be loaded of this NEW page context or call will fail
            control = page.LoadControl(userControlVirtualPath);

            if (data != null)
            {
                try
                {
                    ReflectionUtils.SetProperty(control, "Data", data);
                }
                catch { throw new ArgumentException(STR_NoUserControlDataProperty); }
            }

            HtmlForm form = new HtmlForm();
            form.ID = "__t";
            page.Controls.Add(form);

            form.Controls.Add(new LiteralControl(STR_BeginRenderControlBlock));
            form.Controls.Add(control);
            form.Controls.Add(new LiteralControl(STR_EndRenderControlBlock));

            HttpContext.Current.Server.Execute(page, tw, true);

            string Html = tw.ToString();

            // Strip out form and ViewState, Event Validation etc.
            Html = StringUtils.ExtractString(Html, STR_BeginRenderControlBlock, STR_EndRenderControlBlock);

            return Html;
        }

        /// <summary>
        /// Renders a user control into a string into a string.
        /// </summary>
        /// <param name="userControlVirtualPath">virtual path for the user control</param>
        /// <param name="includePostbackControls">If false renders using RenderControl, otherwise uses Server.Execute() constructing a new form.</param>
        /// <param name="data">Optional Data parameter that can be passed to the User Control IF the user control has a Data property.</param>
        /// <returns></returns>
        public static string RenderUserControl(string userControlVirtualPath,
                                               bool includePostbackControls)
        {
            return RenderUserControl(userControlVirtualPath, includePostbackControls, null);
        }

        /// <summary>
        /// Finds a Control recursively. Note finds the first match and exits
        /// </summary>
        /// <param name="ContainerCtl">The top level container to start searching from</param>
        /// <param name="IdToFind">The ID of the control to find</param>
        /// <param name="alwaysUseFindControl">If true uses FindControl to check for hte primary Id which is slower, but finds dynamically generated control ids inside of INamingContainers</param>
        /// <returns></returns>
        public static Control FindControlRecursive(Control Root, string id, bool alwaysUseFindControl = false)
        {
            if (alwaysUseFindControl)
            {
                Control ctl = Root.FindControl(id);
                if (ctl != null)
                    return ctl;
            }
            else
            {
                if (Root.ID == id)
                    return Root;
            }

            foreach (Control Ctl in Root.Controls)
            {
                Control foundCtl = FindControlRecursive(Ctl, id, alwaysUseFindControl);
                if (foundCtl != null)
                    return foundCtl;
            }

            return null;
        }

        #endregion

        #region System Functions

        /// <summary>
        /// Converts an ImageFormat value to a Web Content Type
        /// </summary>
        /// <param name="formatGuid"></param>
        /// <returns></returns>
        public static string ImageFormatToContentType(ImageFormat format)
        {
            string ct = null;

            if (format.Equals(ImageFormat.Png))
                ct = "image/png";
            else if (format.Equals(ImageFormat.Jpeg))
                ct = "image/jpeg";
            else if (format.Equals(ImageFormat.Gif))
                ct = "image/gif";
            else if (format.Equals(ImageFormat.Tiff))
                ct = "image/tiff";
            else if (format.Equals(ImageFormat.Bmp))
                ct = "image/bmp";
            else if (format.Equals(ImageFormat.Icon))
                ct = "image/x-icon";
            else if (format.Equals(ImageFormat.Wmf))
                ct = "application/x-msmetafile";
            else
                throw new InvalidOperationException(string.Format(Resources.ERROR_UnableToConvertImageFormatToContentType, format.ToString()));

            return ct;
        }

        /// <summary>
        /// Returns an image format from an HTTP content type string
        /// </summary>
        /// <param name="contentType">Content Type like image/jpeg</param>
        /// <returns>Corresponding image format</returns>
        public static ImageFormat ImageFormatFromContentType(string contentType)
        {
            ImageFormat format = ImageFormat.Png;

            contentType = contentType.ToLower();

            if (contentType == "image/png")
                return format;
            else if (contentType == "image/gif")
                format = ImageFormat.Gif;
            else if (contentType == "image/jpeg")
                format = ImageFormat.Jpeg;
            else if (contentType == "image/tiff")
                format = ImageFormat.Jpeg;
            else if (contentType == "image/bmp")
                format = ImageFormat.Bmp;
            else if (contentType == "image/x-icon")
                format = ImageFormat.Icon;
            else if (contentType == "application/x-msmetafile")
                format = ImageFormat.Wmf;
            else
                throw new InvalidOperationException(string.Format(Resources.ERROR_UnableToConvertContentTypeToImageFormat, contentType));

            return format;
        }

        /// <summary>
        /// Determines if GZip is supported
        /// </summary>
        /// <returns></returns>
        public static bool IsGZipSupported()
        {
            string AcceptEncoding = HttpContext.Current.Request.Headers["Accept-Encoding"];
            if (!string.IsNullOrEmpty(AcceptEncoding) &&
                    (AcceptEncoding.Contains("gzip") || AcceptEncoding.Contains("deflate")))
                return true;
            return false;
        }

        /// <summary>
        /// Sets up the current page or handler to use GZip through a Response.Filter
        /// IMPORTANT:  
        /// You have to call this method before any output is generated!
        /// </summary>
        public static void GZipEncodePage()
        {
            HttpResponse Response = HttpContext.Current.Response;

            if (IsGZipSupported())
            {
                string AcceptEncoding = HttpContext.Current.Request.Headers["Accept-Encoding"];

                if (AcceptEncoding.Contains("gzip"))
                {
                    Response.Filter = new System.IO.Compression.GZipStream(Response.Filter,
                                                System.IO.Compression.CompressionMode.Compress);
                    Response.Headers.Remove("Content-Encoding");
                    Response.AppendHeader("Content-Encoding", "gzip");
                }
                else
                {
                    Response.Filter = new System.IO.Compression.DeflateStream(Response.Filter,
                                                System.IO.Compression.CompressionMode.Compress);
                    Response.Headers.Remove("Content-Encoding");
                    Response.AppendHeader("Content-Encoding", "deflate");
                }
            }

            // Allow proxy servers to cache encoded and unencoded versions separately
            Response.AppendHeader("Vary", "Content-Encoding");
        }


        /// <summary>
        /// Returns the IIS version for the given Operating System.
        /// Note this routine doesn't check to see if IIS is installed
        /// it just returns the version of IIS that should run on the OS.
        /// 
        /// Returns the value from Request.ServerVariables["Server_Software"]
        /// if available. Otherwise uses OS sniffing to determine OS version
        /// and returns IIS version instead.
        /// </summary>
        /// <returns>version number or -1 </returns>
        public static decimal GetIisVersion()
        {
            // if running inside of IIS parse the SERVER_SOFTWARE key
            // This would be most reliable
            if (HttpContext.Current != null && HttpContext.Current.Request != null)
            {
                string os = HttpContext.Current.Request.ServerVariables["SERVER_SOFTWARE"];
                if (!string.IsNullOrEmpty(os))
                {
                    //Microsoft-IIS/7.5
                    int dash = os.LastIndexOf("/");
                    if (dash > 0)
                    {
                        decimal iisVer = 0M;
                        if (Decimal.TryParse(os.Substring(dash + 1), out iisVer))
                            return iisVer;
                    }
                }
            }

            decimal osVer = (decimal)Environment.OSVersion.Version.Major +
                            ((decimal)Environment.OSVersion.Version.MajorRevision / 10);

            // Windows 7 and Win2008 R2
            if (osVer == 6.1M)
                return 7.5M;
            // Windows Vista and Windows 2008
            else if (osVer == 6.0M)
                return 7.0M;
            // Windows 2003 and XP 64 bit
            else if (osVer == 5.2M)
                return 6.0M;
            // Windows XP
            else if (osVer == 5.1M)
                return 5.1M;
            // Windows 2000
            else if (osVer == 5.0M)
                return 5.0M;

            // error result
            return -1M;
        }

        /// <summary>
        /// Attempts to restart the active Web Application               
        /// </summary>
        /// <remarks>
        /// Requires either Full Trust (HttpRuntime.UnloadAppDomain) or
        /// or Write access to web.config otherwise the operation
        /// will fail and return false.
        /// </remarks>
        public static bool RestartWebApplication()
        {
            bool error = false;
            try
            {
                // This requires full trust so this will fail
                // in many scenarios
                HttpRuntime.UnloadAppDomain();
            }
            catch
            {
                error = true;
            }

            if (!error)
                return true;

            // Couldn't unload with Runtime - let's try modifying web.config
            // This requires write access in the application's folder for
            // application's Application Pool account.
            string ConfigPath = HttpContext.Current.Request.PhysicalApplicationPath + "\\web.config";

            try
            {
                File.SetLastWriteTimeUtc(ConfigPath, DateTime.UtcNow);
            }
            catch
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Returns an encrypted string based on the local machine key of the local machine.
        /// Can be used to create machine specific seed values that cannot be tracked via code        
        /// </summary>
        /// <param name="keyData">Optional - If no data is passed in a default seed value is used.</param>
        /// <returns>Encoded string</returns>
        /// <remarks>Applies only to ASP.NET Web applications</remarks>
        public static string MachineKeySeedValue(byte[] keyData = null)
        {
            if (keyData == null)
                keyData = new byte[] { 33, 34, 1, 10, 44, 156, 255, 200, 255, 1, 0, 44, 10, 144, 33, 77, 21, 153, 12, 8 };
            return MachineKey.Encode(keyData, MachineKeyProtection.Encryption);
        }

        #endregion

        #region JSON Value Encoding

        /// <summary>
        /// Encodes a string to be represented as a string literal. The format
        /// is essentially a JSON string that is returned in double quotes.
        /// 
        /// The string returned includes outer quotes: 
        /// "Hello \"Rick\"!\r\nRock on"
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static string EncodeJsString(string s)
        {
            if (s == null)
                return "null";

            StringBuilder sb = new StringBuilder();
            sb.Append("\"");
            foreach (char c in s)
            {
                switch (c)
                {
                    case '\"':
                        sb.Append("\\\"");
                        break;
                    case '\\':
                        sb.Append("\\\\");
                        break;
                    case '\b':
                        sb.Append("\\b");
                        break;
                    case '\f':
                        sb.Append("\\f");
                        break;
                    case '\n':
                        sb.Append("\\n");
                        break;
                    case '\r':
                        sb.Append("\\r");
                        break;
                    case '\t':
                        sb.Append("\\t");
                        break;
                    default:
                        int i = (int)c;
                        if (i < 32  || c == '<' ||  c == '>')
                        {
                            sb.AppendFormat("\\u{0:X04}", i);
                        }
                        else
                        {
                            sb.Append(c);
                        }
                        break;
                }
            }
            sb.Append("\"");


            return sb.ToString();
        }

        /// <summary>
        /// Parses a JSON string into a string value
        /// </summary>
        /// <param name="encodedString">JSON string</param>
        /// <returns>unencoded string</returns>
        public static string DecodeJsString(string encodedString)
        {
            // actual value of null is not valid for 
            if (encodedString == null)
                return null;

            // null as a string is a valid value for a string
            if (encodedString == "null")
                return null;

            // Has to be bracketed in quotes
            if (!encodedString.StartsWith("\"") || !encodedString.EndsWith("\""))
                encodedString = "\"" + encodedString + "\"";

            if (encodedString == "\"\"")
                return string.Empty;

            // strip off leading and trailing quote chars
            encodedString = encodedString.Substring(1, encodedString.Length - 2);

            // Escape the double escape characters in json ('real' backslash)  temporarily to alternate chars
            const string ESCAPE_ESCAPECHARS = @"^#^#";

            encodedString = encodedString.Replace(@"\\", ESCAPE_ESCAPECHARS);

            encodedString = encodedString.Replace(@"\r", "\r");
            encodedString = encodedString.Replace(@"\n", "\n");
            encodedString = encodedString.Replace(@"\""", "\"");
            encodedString = encodedString.Replace(@"\t", "\t");
            encodedString = encodedString.Replace(@"\b", "\b");
            encodedString = encodedString.Replace(@"\f", "\f");

            if (encodedString.Contains("\\u"))
                encodedString = Regex.Replace(encodedString, @"\\u....",
                                      new MatchEvaluator(UnicodeEscapeMatchEvaluator));

            // Convert escaped characters back to the actual backslash char 
            encodedString = encodedString.Replace(ESCAPE_ESCAPECHARS, "\\");

            return encodedString;
        }


        /// <summary>
        /// Converts a .NET date to a JavaScript JSON date value.
        /// </summary>
        /// <param name="date">.Net Date</param>
        /// <returns></returns>
        public static string EncodeJsDate(DateTime date, JsonDateEncodingModes dateMode = JsonDateEncodingModes.ISO)
        {
            TimeSpan tspan = date.ToUniversalTime().Subtract(DAT_JAVASCRIPT_BASEDATE);
            double milliseconds = Math.Floor(tspan.TotalMilliseconds);

            // ISO 8601 mode string "2009-03-28T21:55:21.1234567Z"
            if (dateMode == JsonDateEncodingModes.ISO)
                // this is the same format that browser JSON formatters produce
                return string.Concat("\"", date.ToString("yyyy-MM-ddThh:mm:ss.fffZ"), "\"");

            // raw date expression - new Date(1227578400000)
            if (dateMode == JsonDateEncodingModes.NewDateExpression)
                return "new Date(" + milliseconds.ToString() + ")";

            // MS Ajax style string: "\/Date(1227578400000)\/"
            if (dateMode == JsonDateEncodingModes.MsAjax)
            {
                StringBuilder sb = new StringBuilder(40);
                sb.Append(@"""\/Date(");
                sb.Append(milliseconds);

                // Add Timezone 
                sb.Append((TimeZone.CurrentTimeZone.GetUtcOffset(date).Hours * 100).ToString("0000").PadLeft(4, '0'));

                sb.Append(@")\/""");
                return sb.ToString();
            }

            throw new ArgumentException("Date Format not supported.");
        }

        /// <summary>
        /// Matchevaluated to unescape string encoded Unicode character in the format of \u03AF
        /// </summary>
        /// <param name="match"></param>
        /// <returns></returns>
        private static string UnicodeEscapeMatchEvaluator(Match match)
        {
            // last 4 digits are hex value
            string hex = match.Value.Substring(2);
            char val = (char)ushort.Parse(hex, System.Globalization.NumberStyles.HexNumber);
            return val.ToString();
        }

        #endregion


        #region Localization

        /// <summary>
        /// Sets the culture and UI culture to a specific culture. Allows overriding of currency
        /// and optionally disallows setting the UI culture.
        /// 
        /// You can also limit the locales that are allowed in order to minimize
        /// resource access for locales that aren't implemented at all.
        /// </summary>
        /// <param name="culture">
        /// 2 or 5 letter ietf string code for the Culture to set. 
        /// Examples: en-US or en</param>
        /// <param name="uiCulture">ietf string code for UiCulture to set</param>
        /// <param name="currencySymbol">Override the currency symbol on the culture</param>
        /// <param name="setUiCulture">
        /// if uiCulture is not set but setUiCulture is true 
        /// it's set to the same as main culture
        /// </param>
        /// <param name="allowedLocales">
        /// Names of 2 or 5 letter ietf locale codes you want to allow
        /// separated by commas. If two letter codes are used any
        /// specific version (ie. en-US, en-GB for en) are accepted.
        /// Any other locales revert to the machine's default locale.
        /// Useful reducing overhead in looking up resource sets that
        /// don't exist and using unsupported culture settings .
        /// Example: de,fr,it,en-US
        /// </param>
        public static void SetUserLocale(string culture = null, 
            string uiCulture = null, 
            string currencySymbol = null, 
            bool setUiCulture = true,
            string allowedLocales = null)
        {
            // Use browser detection in ASP.NET
            if (string.IsNullOrEmpty(culture) && HttpContext.Current != null)
            {
                HttpRequest Request = HttpContext.Current.Request;

                // if no user lang leave existing but make writable
                if (Request.UserLanguages == null)
                {
                    Thread.CurrentThread.CurrentCulture = Thread.CurrentThread.CurrentCulture.Clone() as CultureInfo;
                    if (setUiCulture)
                        Thread.CurrentThread.CurrentUICulture = Thread.CurrentThread.CurrentUICulture.Clone() as CultureInfo ;
                    
                    return;
                }

                culture = Request.UserLanguages[0];
            }
            else
                culture = culture.ToLower();

            if (!string.IsNullOrEmpty(uiCulture))
                setUiCulture = true;

            if (!string.IsNullOrEmpty(culture) && !string.IsNullOrEmpty(allowedLocales))
            {
                allowedLocales = "," + allowedLocales.ToLower() + ",";
                if (!allowedLocales.Contains("," + culture + ","))
                {
                    int i = culture.IndexOf('-');
                    if (i > 0)
                    {                        
                        if (!allowedLocales.Contains("," + culture.Substring(0, i) + ","))
                        {                            
                            // Always create writable CultureInfo
                            Thread.CurrentThread.CurrentCulture = Thread.CurrentThread.CurrentCulture.Clone() as CultureInfo;
                            if (setUiCulture)
                                Thread.CurrentThread.CurrentUICulture = Thread.CurrentThread.CurrentUICulture.Clone() as CultureInfo;
                                        
                            return;
                        }
                    }
                }
            }

            if (string.IsNullOrEmpty(culture))
                culture = CultureInfo.InstalledUICulture.IetfLanguageTag;

            if (string.IsNullOrEmpty(uiCulture))
                uiCulture = culture;

            try
            {
                CultureInfo Culture = new CultureInfo(culture);
                
                if (currencySymbol != null && currencySymbol != "")
                    Culture.NumberFormat.CurrencySymbol = currencySymbol;

                Thread.CurrentThread.CurrentCulture = Culture;

                if (setUiCulture)
                {                    
                    var UICulture = new CultureInfo(uiCulture);
                    Thread.CurrentThread.CurrentUICulture = UICulture;
                }   
            }
            catch { }            
        }

        /// <summary>
        /// Sets a user's Locale based on the browser's Locale setting. If no setting
        /// is provided the default Locale is used.
        /// </summary>
        /// <param name="currencySymbol">If not null overrides the currency symbol for the culture. 
        /// Use to force a specify currency when multiple currencies are not supported by the application
        /// </param>
        /// <param name="setUiCulture">if true sets the UI culture in addition to core culture</param>
        [Obsolete("Use the many parametered version of SetUserLocale instead.")]
        public static void SetUserLocale(string currencySymbol, bool setUiCulture)
        {
            SetUserLocale(null, null, currencySymbol, setUiCulture);
        }

        /// <summary>
        /// Returns a JavaScript Encoded string from a Global Resource
        /// Defaults to the "Resources" resource set.
        /// </summary>
        /// <param name="resourceKey"></param>
        /// <returns></returns>
        public static string GResJs(string resourceKey)
        {
            return GResJs("Resources", resourceKey);
        }

        /// <summary>
        /// Returns a resource string. Shortcut for HttpContext.GetGlobalResourceObject.
        /// </summary>
        /// <param name="resourceSet">Resource Set Id (ie. name of the file or 'resource set')</param>
        /// <param name="resourceId">The key in the resource set</param>
        /// <returns></returns>
        public static string GRes(string resourceSet, string resourceId)
        {
            string Value = HttpContext.GetGlobalResourceObject(resourceSet, resourceId) as string;
            if (string.IsNullOrEmpty(Value))
                return resourceId;

            return Value;
        }

        /// <summary>
        /// Returns a resource string. Shortcut for HttpContext.GetGlobalResourceObject.
        /// 
        /// This version defaults to Resources as the resource set it.
        /// Defaults to "Resources" as the ResourceSet (ie. Resources.xx.resx)
        /// </summary>
        /// <param name="resourceId">Key in the Resources resource set</param>
        /// <returns></returns>
        public static string GRes(string resourceId)
        {
            return GRes("Resources", resourceId);
        }

        /// <summary>
        /// Returns a JavaScript Encoded string from a Global Resource
        /// </summary>
        /// <param name="classKey"></param>
        /// <param name="resourceId"></param>
        /// <returns></returns>
        public static string GResJs(string classKey, string resourceId)
        {
            string Value = GRes(classKey, resourceId) as string;
            return EncodeJsString(Value);
        }

        /// <summary>
        /// Returns a local resource from the resource set of the current active request
        /// local resource.
        /// </summary>       
        /// <param name="resourceId">The resourceId of the item in the local resourceSet file to retrieve</param>
        /// <returns></returns>
        public static string LRes(string resourceId)
        {
            string vPath = HttpContext.Current.Request.AppRelativeCurrentExecutionFilePath;
            //vPath = vPath.Replace("~/", "").Replace("~", "");
            string value = HttpContext.GetLocalResourceObject(vPath, resourceId) as string;

            if (value == null)
                return resourceId;

            return value;
        }

        /// <summary>
        /// Returns a local resource for the given resource set that you specify explicitly.
        /// 
        /// Use this method only if you need to retrieve resources from a local resource not
        /// specific to the current request.
        /// </summary>
        /// <param name="resourceSet">The resourceset specified as: subdir/page.aspx or page.aspx or as a virtual path (~/subdir/page.aspx)</param>
        /// <param name="resourceKey">The resource ID to retrieve from the resourceset</param>
        /// <returns></returns>
        public static string LRes(string resourceSet, string resourceKey)
        {
            if (!resourceSet.StartsWith("~/"))
                resourceSet = "~/" + resourceSet;

            string Value = HttpContext.GetLocalResourceObject(resourceSet, resourceKey) as string;
            if (Value == null)
                return resourceKey;

            return Value;
        }

        /// <summary>
        /// Returns a local resource properly encoded as a JavaScript string 
        /// including the quote characters.
        /// </summary>
        /// <param name="resourceId"></param>
        /// <returns></returns>
        public static string LResJs(string resourceId)
        {
            return LRes(EncodeJsString(resourceId));
        }

        #endregion
    }

    public class UserLocaleResult
    {
        public CultureInfo Culture;
        public CultureInfo UiCulture;
    }


}