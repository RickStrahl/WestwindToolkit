using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI;
using Westwind.Utilities;
using System.IO;
using System.Reflection;

namespace Westwind.Web
{
    /// <summary>
    /// Class that handles embedding of common script files like jQuery into 
    /// a page or return complete script tags for insertion into a ViewPage as a string.
    /// </summary>
    public static class ScriptLoader
    {

        /// <summary>
        /// Determines what location jQuery is loaded from
        /// </summary>
        public static jQueryLoadModes jQueryLoadMode = jQueryLoadModes.ContentDeliveryNetwork;

        /// <summary>
        /// jQuery CDN Url on Google
        /// </summary>
        public static string jQueryCdnUrl = "//ajax.googleapis.com/ajax/libs/jquery/1.11.0/jquery.min.js";

        /// <summary>
        /// Fallback Url if CDN can't be reached.
        /// </summary>
        public static string jQueryCdnFallbackUrl = "~/scripts/jquery.11.0.min.js";

        /// <summary>
        /// jQuery CDN Url on Google
        /// </summary>
        public static string jQueryUiCdnUrl = "//ajax.googleapis.com/ajax/libs/jqueryui/1.10.3/jquery-ui.min.js";

        /// <summary>
        /// jQuery UI fallback Url if CDN is unavailable or WebResource is used
        /// Note: The file needs to exist and hold the minimized version of jQuery ui
        /// </summary>
        public static string jQueryUiLocalFallbackUrl = "~/scripts/jquery-ui.min.js";

        /// <summary>
        /// The url the jQuery UI *base* CSS theme. Used in jQueryUiCssLink. Should point
        /// at the jQuery UI base theme - the theme is replaced either explicitly or from
        /// the jQueryUiTheme property value.soap
        /// </summary>
        public static string jQueryUiCssBaseUrl = "//ajax.googleapis.com/ajax/libs/jqueryui/1.10.3/themes/base/jquery-ui.css";

        /// <summary>
        /// The theme that is applied to the jQueryUiCssBaseUrl
        /// </summary>
        public static string jQueryUiTheme = "redmond";
        
        
        /// <summary>
        /// Internally used Page instance so we can get access to
        /// a Page instance when no Page might be available. To Access ClientScript etc.
        /// </summary>
        internal static Page CachedPage
        {
            get
            {
                if (_CachedPage == null)
                    _CachedPage = new Page();
                return _CachedPage;
            }
        }
        private static Page _CachedPage;


        /// <summary>
        /// Loads jQuery depending on configuration settings (CDN, WebResource or site url) 
        /// and injects the full script link into the page.
        /// </summary>
        /// <param name="control"></param>
        /// <param name="jQueryUrl">Optional url to jQuery as a virtual or absolute server path</param>
        public static void LoadjQuery(Control control, string jQueryUrl)
        {
            ClientScriptProxy p = ClientScriptProxy.Current;

            if (!string.IsNullOrEmpty(jQueryUrl))
                p.RegisterClientScriptInclude(control, typeof(WebResources), jQueryUrl, ScriptRenderModes.HeaderTop);
            else if (jQueryLoadMode == jQueryLoadModes.WebResource)
                p.RegisterClientScriptResource(control, typeof(WebResources), WebResources.JQUERY_SCRIPT_RESOURCE, ScriptRenderModes.HeaderTop);
            else if (jQueryLoadMode == jQueryLoadModes.ContentDeliveryNetwork)
            {
                // Load from CDN Url specified
                p.RegisterClientScriptInclude(control, typeof(WebResources), jQueryCdnUrl, ScriptRenderModes.HeaderTop);

                // check if jquery loaded - if it didn't we're not online and use WebResource
                string scriptCheck =
                    @"if (typeof(jQuery) == 'undefined')  
        document.write(unescape(""%3Cscript src='{0}' type='text/javascript'%3E%3C/script%3E""));";

                jQueryUrl = p.GetClientScriptResourceUrl(control, typeof(WebResources),
                                WebResources.JQUERY_SCRIPT_RESOURCE);
                p.RegisterClientScriptBlock(control, typeof(WebResources),
                                "jquery_register", string.Format(scriptCheck, jQueryUrl), true,
                                ScriptRenderModes.HeaderTop);
            }

            return;
        }



        /// <summary>
        /// Inserts a script link to load jQuery into the page based on the jQueryLoadModes settings
        /// of this class. Default load is by CDN plus WebResource fallback
        /// </summary>
        /// <param name="url">
        /// An optional explicit URL to load jQuery from. Url is resolved. 
        /// When specified no fallback is applied
        /// </param>        
        /// <returns>full script tag and fallback script for jQuery to load</returns>
        public static string jQueryLink(jQueryLoadModes jQueryLoadMode = jQueryLoadModes.Default, string url = null)
        {

            if (jQueryLoadMode == jQueryLoadModes.Default)
                jQueryLoadMode = ScriptLoader.jQueryLoadMode;
            if (jQueryLoadMode == jQueryLoadModes.None)
                return string.Empty;

            string jQueryUrl = string.Empty;
            string fallbackScript = string.Empty;

            var script = ClientScriptProxy.Current;

            if (!string.IsNullOrEmpty(url))
                jQueryUrl = WebUtils.ResolveUrl(url);
            else if (jQueryLoadMode == jQueryLoadModes.WebResource)
                jQueryUrl = script.GetClientScriptResourceUrl(typeof(WebResources),
                                                              WebResources.JQUERY_SCRIPT_RESOURCE);
            else if (jQueryLoadMode == jQueryLoadModes.ContentDeliveryNetwork)
            {
                jQueryUrl = ScriptLoader.jQueryCdnUrl;

                if (!string.IsNullOrEmpty(jQueryCdnUrl))
                {
                    // check if jquery loaded - if it didn't we're not online and use WebResource
                    fallbackScript =

@"<script type=""text/javascript"">if (typeof(jQuery) == 'undefined')
        document.write(unescape(""%3Cscript src='{0}' type='text/javascript'%3E%3C/script%3E""));
</script>";

                    fallbackScript = string.Format(fallbackScript, WebUtils.ResolveUrl(jQueryCdnFallbackUrl));
                }
            }

            string output = "<script src=\"" + jQueryUrl + "\" type=\"text/javascript\"></script>";

            // add in the CDN fallback script code
            if (!string.IsNullOrEmpty(fallbackScript))
                output += "\r\n" + fallbackScript + "\r\n";

            return output;
        }


        /// <summary>
        /// Inserts a script link to load jQuery into the page based on the jQueryLoadModes settings
        /// of this class. Default load is by CDN plus WebResource fallback
        /// </summary>
        /// <param name="url">
        /// An optional explicit URL to load jQuery from. Url is resolved. 
        /// When specified no fallback is applied
        /// </param>        
        /// <returns>full script tag and fallback script for jQuery to load</returns>
        public static string jQueryUiLink(jQueryLoadModes jQueryLoadMode = jQueryLoadModes.Default, string url = null)
        {
            if (jQueryLoadMode == jQueryLoadModes.Default)
                jQueryLoadMode = ScriptLoader.jQueryLoadMode;
            if (jQueryLoadMode == jQueryLoadModes.None)
                return url ?? string.Empty;

            string jQueryUiUrl = string.Empty;
            string fallbackScript = string.Empty;

            var script = ClientScriptProxy.Current;

            if (!string.IsNullOrEmpty(url))
                jQueryUiUrl = WebUtils.ResolveUrl(url);
            else if (jQueryLoadMode == jQueryLoadModes.WebResource)
                jQueryUiUrl = WebUtils.ResolveUrl(jQueryUiLocalFallbackUrl);
            else if (jQueryLoadMode == jQueryLoadModes.ContentDeliveryNetwork)
            {
                jQueryUiUrl = ScriptLoader.jQueryUiCdnUrl;

                if (!string.IsNullOrEmpty(jQueryCdnUrl))
                {
                    // check if jquery loaded - if it didn't we're not online and use WebResource
                    fallbackScript =
@"<script type=""text/javascript"">if (typeof(jQuery) == 'undefined')
        document.write(unescape(""%3Cscript src='{0}' type='text/javascript'%3E%3C/script%3E""));
</script>";

                    fallbackScript = string.Format(fallbackScript, WebUtils.ResolveUrl(jQueryUiLocalFallbackUrl));
                }
            }

            string output = "<script src=\"" + jQueryUiUrl + "\" type=\"text/javascript\"></script>";

            // add in the CDN fallback script code
            if (!string.IsNullOrEmpty(fallbackScript))
                output += "\r\n" + fallbackScript + "\r\n";

            return output;
        }

        /// <summary>
        /// Returns the global jQuery UI Url and theme set on this class
        /// </summary>
        /// <param name="theme"></param>
        /// <returns></returns>
        public static string jQueryUiCssLink(string theme = null)
        {
            if (theme == null)
                theme = jQueryUiTheme;

            var url = WebUtils.ResolveUrl(jQueryUiCssBaseUrl);

            return "<link rel=\"stylesheet\" href=\"" + url.Replace("/base/", "/" + theme.ToLower() + "/") + "\" type=\"text/css\" />";
        }


        /// <summary>
        /// Returns a fully qualified script tag for loading ww.jquery.js
        /// </summary>
        /// <param name="jQueryLoadMode"></param>
        /// <param name="url"></param>
        /// <returns></returns>
        public static string wwJqueryLink(jQueryLoadModes jQueryLoadMode = jQueryLoadModes.Default, string url = null)
        {
            if (jQueryLoadMode == jQueryLoadModes.Default)
                jQueryLoadMode = ScriptLoader.jQueryLoadMode;
            if (jQueryLoadMode == jQueryLoadModes.None)
                return url ?? string.Empty;

            string wwJQueryUrl = string.Empty;
            string fallbackScript = string.Empty;

            var script = ClientScriptProxy.Current;

            if (!string.IsNullOrEmpty(url))
                wwJQueryUrl = WebUtils.ResolveUrl(url);
            else if (jQueryLoadMode == jQueryLoadModes.WebResource ||
                     jQueryLoadMode == jQueryLoadModes.ContentDeliveryNetwork)

                wwJQueryUrl = script.GetClientScriptResourceUrl(typeof(WebResources),
                                                          WebResources.WWJQUERY_SCRIPT_RESOURCE);

            if (string.IsNullOrEmpty(wwJQueryUrl))
                wwJQueryUrl = WebUtils.ResolveUrl("~/scripts/ww.jquery.min.js");

            return "<script src=\"" + wwJQueryUrl + "\" type=\"text/javascript\"></script>";
        }


        /// <summary>
        /// Loads the jQuery component uniquely into the page
        /// </summary>
        /// <param name="control"></param>
        /// <param name="jQueryUrl">Optional Url to the jQuery Library. NOTE: Should also have a .min version in place</param>
        public static void LoadjQuery(Control control)
        {
            LoadjQuery(control, null);
        }

        /// <summary>
        /// Loads the ww.jquery.js library from Resources at the end of the Html Header (if available)
        /// </summary>        
        /// <param name="control"></param>
        /// <param name="loadjQuery"></param>
        public static void LoadwwjQuery(Control control, bool loadjQuery = true)
        {
            // jQuery is also required
            if (loadjQuery)
                LoadjQuery(control);

            ClientScriptProxy p = ClientScriptProxy.Current;
            p.RegisterClientScriptResource(control, typeof(WebResources), WebResources.WWJQUERY_SCRIPT_RESOURCE, ScriptRenderModes.Header);
        }

        /// <summary>
        /// Loads the appropriate jScript library out of the scripts directory and 
        /// injects into a WebForms page.
        /// </summary>
        /// <param name="control"></param>
        /// <param name="jQueryUiUrl">Optional url to jQuery as a virtual or absolute server path</param>
        public static void LoadjQueryUi(Control control, string jQueryUiUrl)
        {
            ClientScriptProxy p = ClientScriptProxy.Current;

            // jQuery UI isn't provided as a Web Resource so default to a fixed URL
            if (jQueryLoadMode == jQueryLoadModes.WebResource)
            {
                //throw new InvalidOperationException(Resources.WebResourceNotAvailableForJQueryUI);                
                jQueryUiUrl = WebUtils.ResolveUrl(jQueryUiLocalFallbackUrl);
            }

            if (!string.IsNullOrEmpty(jQueryUiUrl))
                p.RegisterClientScriptInclude(control, typeof(WebResources), jQueryUiUrl, ScriptRenderModes.Header);
            else if (jQueryLoadMode == jQueryLoadModes.ContentDeliveryNetwork)
            {
                // Load from CDN Url specified
                p.RegisterClientScriptInclude(control, typeof(WebResources), jQueryUiCdnUrl, ScriptRenderModes.Header);

                // check if jquery loaded - if it didn't we're not online and use WebResource
                string scriptCheck =
                    @"if (typeof(jQuery.ui) == 'undefined')  
        document.write(unescape(""%3Cscript src='{0}' type='text/javascript'%3E%3C/script%3E""));";

                p.RegisterClientScriptBlock(control,
                                            typeof(WebResources), "jquery_ui",
                                            string.Format(scriptCheck, WebUtils.ResolveUrl(jQueryUiLocalFallbackUrl)),
                                            true, ScriptRenderModes.Header);
            }

            return;
        }


    }

    /// <summary>
    /// The location from which jQuery and jQuery UI are loaded
    /// in Release mode.
    /// </summary>
    public enum jQueryLoadModes
    {
        ContentDeliveryNetwork,
        WebResource,
        Script,
        Default,
        None
    }
}
