#region License
/*
 **************************************************************
 *  Author: Rick Strahl 
 *          © West Wind Technologies, 2008 - 2011
 *          http://www.west-wind.com/
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
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Westwind.Utilities;
using System.Web.UI.HtmlControls;

namespace Westwind.Web.Controls
{

    /// <summary>
    /// This class is used to display messages with a single command via the 
    /// DisplayMessage() method. This class also has the ability to automatically 
    /// redirect to another page which allows moving to new pages and still be able
    ///  to set Cookies if required.
    /// 
    /// This class is abstract and requires an application specific subclass - 
    /// generally MessageDisplay.aspx that must follow a few simple but 
    /// <see>Creating a custom MessageDisplay page</see>.
    /// </summary>
    public class MessageDisplayBase : System.Web.UI.Page
    {

        /// <summary>
        /// Static member that determines the name of the MessageDisplay page.
        /// </summary>
        public static string Pagename = "MessageDisplay.aspx";


        /// <summary>
        /// The Header to be displayed on the page. Used only in the 'display' code.
        /// </summary>
        protected string MessageHeader
        {
            get { return (string)Context.Items["ErrorMessage_Header"]; }
        }

        /// <summary>
        /// The message to be displayed on the page. This text may be in HTML format. Used only in the display code.
        /// </summary>
        protected string Message
        {
            get
            { return (string)Context.Items["ErrorMessage_Message"]; }
        }

        /// <summary>
        /// The Url to redirect to. Optional. Used only in the display code
        /// </summary>
        protected string RedirectUrl
        {
            get { return (string)Context.Items["ErrorMessage_RedirectUrl"]; }
        }


        /// <summary>
        /// A stylesheet reference that gets embedded into the page if set.
        /// </summary>
        public string StyleSheet
        {
            get { return _StyleSheet; }
            set { _StyleSheet = value; }
        }
        private string _StyleSheet = "";

        /// <summary>
        /// This property is set during the loading of the page and can be used
        /// inside of the page to allow relative links to be found.
        /// </summary>
        public string BasePath
        {
            get { return _BasePath; }
            set { _BasePath = value; }
        }
        private string _BasePath = "";



        /// <summary>
        /// Displays the page with the appropriate controls filled in.
        /// </summary>
        /// <remarks>Assumes that lblHeader, lblMessage, lblRedirectHyperLink are defined.</remarks>
        public void DisplayPage(Label messageHeader, Label message, Label redirectHyperLink)
        {
            messageHeader.Text = MessageHeader;
            message.Text = Message;
            Page.Title = MessageHeader;

            //// Get the base path						
            //if (string.IsNullOrEmpty(BasePath))
            //    BasePath = Request.Url.GetLeftPart(UriPartial.Authority) +
            //                    ResolveUrl(Request.ApplicationPath);
            //else
            //    BasePath = ResolveUrl(BasePath);

            if (!string.IsNullOrEmpty(StyleSheet))
            {
                HtmlLink css = new HtmlLink();
                css.Href = ResolveUrl(StyleSheet);
                css.Attributes.Add("rel", "stylesheet");
                css.Attributes.Add("type", "text/css");
                Header.Controls.Add(css);
            }
            if (!string.IsNullOrEmpty(BasePath))
            {
                   Literal lit = new Literal();
                lit.Text = string.Format("<base href='{0}' />", ResolveUrl(BasePath));
                Header.Controls.Add(lit);
            }

            if (RedirectUrl != null)
            {
                string NewUrl = RedirectUrl;

                /// Must fix up the path in case we're in a separate sub-dir
                /// because the page is using <base> we must include the full relative path
                if (NewUrl.StartsWith("~") || NewUrl.StartsWith("/"))
                    NewUrl = ResolveUrl(NewUrl);
                else if (!NewUrl.ToLower().StartsWith("http:") &&
                          !NewUrl.ToLower().StartsWith("https:"))
                {
                    // It's a relative Path. Must use current server path  + relative path
                    NewUrl = Request.FilePath.Substring(0, Request.FilePath.LastIndexOf("/") + 1) + NewUrl;
                }


                // Create META tag and add to header controls
                HtmlMeta RedirectMetaTag = new HtmlMeta();
                RedirectMetaTag.HttpEquiv = "Refresh";

                RedirectMetaTag.Content = string.Format("{0}; URL={1}", Context.Items["ErrorMessage_Timeout"], NewUrl);
                Header.Controls.Add(RedirectMetaTag);

                // Also add Visible link onto the page
                redirectHyperLink.Text = "<a href='" + NewUrl + "'>Click here</a> if your browser is not automatically continuing.";

                Header.DataBind();
            }
        }

        /// <summary>
        /// Displays the page by setting the lblHeader, lblMessage and lblRedirectHyperLink controls
        /// </summary>
        public void DisplayPage()
        {
            // We have to locate the various controls since the base class doesn't
            // get values assigned through class hierarchy
            DisplayPage((Label)WebUtils.FindControlRecursive(this, "lblHeader"),
                             (Label)WebUtils.FindControlRecursive(this, "lblMessage"),
                             (Label)WebUtils.FindControlRecursive(this, "lblRedirectHyperLink"));
        }


        /// <summary>
        /// Generates a self-contained error message display page that issues a 
        /// Server.Transfer to the MessageDisplay.aspx page in your application root.
        /// <seealso>Class wwMessageDisplay</seealso>
        /// </summary>
        /// <param name="Header">
        /// Header message and title of the page
        /// </param>
        /// <param name="Message">
        /// The body of the message - this is HTML
        /// </param>
        /// <param name="RedirectUrl">
        /// Url to redirect to
        /// </param>
        /// <param name="Timeout">
        /// Timeout for the page before redirecting
        /// </param>
        /// <returns>Void</returns>
        /// <example>
        /// MessageDisplay.DisplayMessage("Clearing Profile",
        /// 	"We're clearing out your profile to log you out of the "+
        /// 	App.Configuration.StoreName + " for this computer.",
        /// 	"/default.aspx",4);
        /// </example>
        public static void DisplayMessage(string TemplatePageName, string Header, string Message, string RedirectUrl, int Timeout)
        {
            HttpContext Context = HttpContext.Current;
            Context.Items.Add("ErrorMessage_Header", Header);
            Context.Items.Add("ErrorMessage_Message", Message);
            Context.Items.Add("ErrorMessage_Timeout", Timeout);
            Context.Items.Add("ErrorMessage_RedirectUrl", RedirectUrl);

            Context.Response.Clear();
            Context.Server.Transfer(Context.Request.ApplicationPath + "/" + TemplatePageName);
        }

        /// <summary>
        /// Generates a self-contained error message display page that issues a 
        /// Server.Transfer to the MessageDisplay.aspx page in your application root.
        /// <seealso>Class wwMessageDisplay</seealso>
        /// </summary>
        /// <param name="Header">
        /// Header message and title of the page
        /// </param>
        /// <param name="Message">
        /// The body of the message - this is HTML
        /// </param>
        /// <param name="RedirectUrl">
        /// Url to redirect to
        /// </param>
        /// <param name="Timeout">
        /// Timeout for the page before redirecting
        /// </param>
        /// <returns>Void</returns>
        /// <example>
        /// MessageDisplay.DisplayMessage("Clearing Profile",
        /// 	"We're clearing out your profile to log you out of the "+
        /// 	App.Configuration.StoreName + " for this computer.",
        /// 	"/default.aspx",4);
        /// </example>		
        public static void DisplayMessage(string Header, string Message, string RedirectUrl, int Timeout)
        {
            DisplayMessage(Pagename, Header, Message, RedirectUrl, Timeout);
        }

        /// <summary>
        /// Generates a self-contained error message display page that issues a 
        /// Server.Transfer to the MessageDisplay.aspx page in your application root.
        /// <seealso>Class wwMessageDisplay</seealso>
        /// </summary>
        /// <param name="Header">
        /// Header message and title of the page
        /// </param>
        /// <param name="Message">
        /// The body of the message - this is HTML
        /// </param>
        /// <returns>Void</returns>
        /// <example>
        /// MessageDisplay.DisplayMessage("Clearing Profile",
        /// 	"We're clearing out your profile to log you out of the "+
        /// 	App.Configuration.StoreName + " for this computer.",
        /// 	"/default.aspx",4);
        /// </example>
        public static void DisplayMessage(string Header, string Message)
        {
            DisplayMessage(Header, Message, null, 0);
        }
    }


}
