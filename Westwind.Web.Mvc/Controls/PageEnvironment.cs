using System;
using System.Web;
using Westwind.Utilities;
using System.Text;
using System.Collections.Generic;
using System.Web.UI;

namespace Westwind.Web.Mvc
{
    /// <summary>
    /// Class that can be used by Controls to access some
    /// common high level functionality
    /// </summary>
    public class PageEnvironment
    {
        private const string STR__PageState = "_PageState";

        /// <summary>
        /// Current Page State Instance
        /// </summary>
        public static PageEnvironment Current 
        {
            get
            {
                PageEnvironment state = HttpContext.Current.Items[STR__PageState] as PageEnvironment;
                if (state != null)
                    return state;

                state = new PageEnvironment();
                HttpContext.Current.Items[STR__PageState] = state;
                return state;            
            }
        }

        protected HashSet<string> ScriptsLoaded = new HashSet<string>();
        protected HashSet<string> CssLoaded = new HashSet<string>();

        private static object SyncLock = new object();


        /// <summary>
        /// Embeds a piece of script into the page uniquely by id. 
        /// 
        /// Use this method to ensure this script block is loaded into the page
        /// only once based on a common id. First script loaded wins.
        /// </summary>
        /// <param name="output"></param>
        /// <param name="script"></param>
        /// <param name="id"></param>
        /// <param name="addScriptTags"></param>
        public void EmbedScriptString(StringBuilder output, string script, string id, bool addScriptTags=true)
        {
            if (this.ScriptsLoaded.Contains(id))
                return;

            if (addScriptTags)
                output.AppendLine("\r\n<script type=\"text/javascript\">");

            output.AppendLine(script);

            if (addScriptTags)
                output.AppendLine("</script>");

            this.ScriptsLoaded.Add(id);
        }


        /// <summary>
        /// Internal Cached Page instance for access to client script
        /// </summary>
        protected static Page CachedPageInstance
        {
            get {
                if (_CachedPage == null)
                {
                    lock (SyncLock)
                    {
                        if (_CachedPage == null)
                            _CachedPage = new Page();
                    }
                }                
                return _CachedPage; 
            }           
        }
        private static Page _CachedPage = null;


        /// <summary>
        /// Returns a Url to a WebResource as a string
        /// </summary>
        /// <param name="type">Any type in the same assembly as the Resource</param>
        /// <param name="resourceId">The full resource Id in the specified assembly</param>
        /// <returns></returns>
        public string GetWebResourceUrl(Type type, string resourceId)
        {
            if (type == null)
                type = this.GetType();

            //MethodInfo mi = typeof(AssemblyResourceLoader).GetMethod(
            //                                     "GetWebResourceUrlInternal",
            //                                      BindingFlags.NonPublic | BindingFlags.Static);
            //return "/" + (string)mi.Invoke(null,
            //             new object[] { Assembly.GetAssembly(type), resourceId, false });
            
            return CachedPageInstance.ClientScript.GetWebResourceUrl(type, resourceId);
        }




        /// <summary>
        /// Embeds a script reference into the page
        /// </summary>
        /// <param name="output"></param>
        /// <param name="resourceId"></param>        
        /// <param name="id"></param>
        public void EmbedScriptResource(StringBuilder output, Type type, string resourceId, string id)
        {
            if (type == null)
                type = this.GetType();

            string url = this.GetWebResourceUrl(type, resourceId);
            EmbedScriptReference(output, url, id);
        }

        /// <summary>
        /// Embeds a script tag that references an external .js/resource
        /// </summary>
        /// <param name="output"></param>
        /// <param name="url"></param>
        /// <param name="id"></param>
        public void EmbedScriptReference(StringBuilder output, string url, string id)
        {
            if (this.ScriptsLoaded.Contains(id))
                return;

            output.AppendLine("<script src=\"" + WebUtils.ResolveUrl(url) + "\" type=\"text/javascript\" ></script>");

            this.ScriptsLoaded.Add(id);
        }               

        public void EmbedCssLink(StringBuilder output, string cssLink, string id)
        {
            if (this.CssLoaded.Contains(id))
                return;

            output.AppendLine("<link href=\"" + WebUtils.ResolveUrl(cssLink) + 
                              "\" rel=\"stylesheet\" type=\"text/css\" />");

            this.CssLoaded.Add(id);
        }
    }
}
