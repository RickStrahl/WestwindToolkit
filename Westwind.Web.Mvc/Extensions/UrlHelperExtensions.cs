using System;
using System.Web;
using System.Web.Mvc;
using Westwind.Web.Mvc;
using Westwind.Utilities;
using System.Text;
using System.Collections.Generic;
using System.Web.Routing;
using System.IO;

namespace Westwind.Web.Mvc
{
    /// <summary>
    /// Application specific UrlHelper Extensions
    /// </summary>
    public static class UrlHelperExtensions
    {
     

        private static string[] CssPaths = 
        {   
            // Theme path first
            "~/css/{1}/{0}",
           // Base Css Path second
            "~/css/{0}"
        };

        private static string[] ViewPaths =
        {
            "~/views/{1}/{0}",
           // Base Css Path second
            "~/views/{0}"
        };

        private static Dictionary<string, string> CssFileCache = new Dictionary<string, string>();
        private static Dictionary<string, string> ViewFileCache = new Dictionary<string, string>();
        

        /// <summary>
        /// Retrieves a CSS link based on a themes path
        /// </summary>
        /// <param name="urlHelper"></param>
        /// <param name="cssFile"></param>
        /// <param name="theme"></param>
        /// <returns></returns>
        public static string Css(this UrlHelper urlHelper, string cssFile, string theme = null)
        {                        
            object oTheme = urlHelper.RequestContext.HttpContext.Session["Theme"];
            if (oTheme != null)
                theme = oTheme as string;
            
            string url = null;

            if (string.IsNullOrEmpty(theme))
                url = urlHelper.Content("~/css/" + cssFile);
            else
            {
                string lowerCssFile = cssFile.ToLower();
                theme=theme.ToLower();
                string key = theme + "|" + cssFile;


                if (CssFileCache.ContainsKey(key))
                    return CssFileCache[key];

                foreach (string path in CssPaths)
                {
                    string webPath = string.Format(path, cssFile, theme);
                    if (File.Exists(urlHelper.RequestContext.HttpContext.Server.MapPath(webPath)))
                    {
                        url = urlHelper.Content(webPath);
                        CssFileCache.Add(key, url);
                        break;
                    }
                }
            }

            return url;
        }
    }
}
