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

namespace Westwind.Web.Controls
{
	/// <summary>
	/// This class presents a Help Link on a page. Specify a linkname in the 
	/// HelpTopic property. Usually this is the name of a page.
	/// 
	/// Note: Can only be dropped on a wwWebForm derived Page class as it
	/// depends on methods in this class to figure out base URLs.
	/// 
	/// Note: The image does not display in design mode.
	/// </summary>
	[ToolboxData("<{0}:HelpControl runat=\"server\" />")]
	public class HelpControl : WebControl
	{
	
		/// <summary>
		/// The Help Topic to display. This should be just the help detail - usually
		/// the filename only (ie. _SomePage.htm) in your help base path.
		/// 
		/// If this value is blank it inherits the page's HelpTopic. If both are
		/// blank this help link is not displayed.
		/// 
		/// Note: Make sure you set the static <see cref="wwWebForm.HelpBaseUrl">wwWebForm.HelpBaseUrl</see>> property in your application
		/// startup so the proper help base path is used.
		/// </summary>
		[Category("Help Content"),Description("The page name of the HTML  topic to display. If this value is blank it inherits the value from the page. If both are blank the link won't render at all."),
		DefaultValue("")]
		public string HelpTopic
		{
			get
			{                
				return _HelpTopic;
			}
			set
			{
				_HelpTopic = value;
			}
		}
		private string _HelpTopic = "";

		/// <summary>
		/// The text of the label. Leave blank to show only the image
		/// </summary>
		[Category("Help Content"),Description("The text of the label. Leave blank to show only the image"),
		DefaultValue("Help")]
		public string Text
		{
			get
			{
				return _Text;
			}
			set
			{
				_Text = value;
			}
		}
		private string _Text = "Help";

		
		/// <summary>
		/// The alternate text for the image displayed when the mouse hovers over the image.
		/// </summary>
		[Category("Help Content"),Description("The alternate text displayed when hovering over the image."),
		 DefaultValue("")]
		public string AltText
		{
			get
			{
				return _AltText;
			}
			set
			{
				_AltText = value;
			}
		}
		private string _AltText = "";


		/// <summary>
		/// Determines whether this control handles a help link and/or F1 key operation
		/// </summary>
		[Category("Help Content"),Description("Determines whether this control handles a help link and/or F1 key operation."),
        DefaultValue(HelpControlTypes.HelpLink)]
		public HelpControlTypes  HelpControlType
		{
			get
			{
				return _HelpControlType;
			}
			set
			{
				_HelpControlType = value;
			}
		}
		private HelpControlTypes  _HelpControlType = HelpControlTypes.HelpLink;

        /// <summary>
        /// The image used for the help icon. Defaults to ~/images/help.gif
        /// <seealso>Class wwHelpControl                                         </seealso>
        /// </summary>
        [Category("Help Content"), Description("Help Icon image used."),
        DefaultValue("WebResource")]
        public string HelpImage
        {
            get
            {
                return _HelpImage;
            }
            set
            {
                _HelpImage = value;
            }
        }
        private string _HelpImage = "WebResource";

        /// <summary>
		/// The base Help Url that is used as a base for Help Topics.
		/// 
		/// You should set this value in your application's startup code (Application_Start 
		/// or static constructor of a class). Any HelpTopic Ids used are appended
        /// to this base path. Note that this also works with dynamic URLs that
        /// use a querystring as long as the Topic id is the last thing and can
        /// simply be appended to the URL.
		/// </summary>
		public static string HelpBaseUrl = "~/help/";


		/// <summary>
		/// Overridden to render the help control text. Calls back to the
		/// wwWebForm.GetHelpHyperLink() to do all the work.
		/// 
		/// Note: Requires the wwWebForm base class. Throws an exception otherwise
		/// </summary>
		/// <param name="writer"></param>
		protected override void Render(HtmlTextWriter writer)
		{
            if (DesignMode) 
                writer.Write("<b>?</b> <a href='" + HelpTopic +  "'>" + Text +  "</a>");
            else 
            {
                string imageLink = HelpImage;
                if (imageLink == "WebResource")
                    imageLink = Page.ClientScript.GetWebResourceUrl(GetType(), WebResources.HELP_ICON_RESOURCE);

                if (HelpControlType != HelpControlTypes.F1Handler)
					writer.Write( GetHelpHyperLink( HelpTopic, Text, AltText, imageLink, CssClass) );
			}
			
			//base.Render (writer);            
		}

		/// <summary>
		/// Method used to format a Help Url into a fully qualified help Url.
		/// 
		/// This method should be overridden in an application specific method.
		/// </summary>
		protected static string FormatHelpUrl(string HelpTopic) 
		{
			if (HelpBaseUrl.StartsWith("~"))
				return WebUtils.ResolveUrl( HelpBaseUrl ) + HelpTopic;
		
			return HelpBaseUrl + HelpTopic;
		}

		/// <summary>
		/// Returns a fully HREF string that can be embedded into a page 
		/// with <%= ShowHelpLink() %>.
		/// 
		/// Assumption: ~/images/help.gif exists for a help icon
		/// </summary>
		public static string GetHelpHyperLink(string HelpTopic,string LabelText,string AltText,string imageLink, string cssClass) 
		{
			if (HelpTopic == "") 
				return "";

            // *** Retrieve formatted Url
            string HelpUrl = FormatHelpUrl(HelpTopic);

            if (string.IsNullOrEmpty(imageLink))
                imageLink = "~/images/help.gif";
            imageLink = WebUtils.ResolveUrl(imageLink);
            
			// Add Alternate text if provided
			if (AltText != null && AltText != "")
				AltText = " alt=\"" +AltText + "\" ";
			else
				AltText = "";

			if (LabelText == "")
				LabelText = null;

            if (!string.IsNullOrEmpty(cssClass))
                cssClass = "class=\"" + cssClass + "\" ";

            
			return "<a href='" + HelpUrl + "' " + cssClass + "style='text-decoration:none' target='WebStoreHelp'><img src='" + 
				imageLink +	"' border='0'" + AltText+ ">"  + 
				(LabelText == null ? "" : " " + LabelText) +   "</a>";
		}

		public string GetHelpHyperLink(string HelpTopic) 
		{
			return GetHelpHyperLink(HelpTopic,"Help",null,null,null);
		}

		public string GetHelpHyperLink() 
		{
			return GetHelpHyperLink(null,"Help",null,null,null);
		}


		protected override void OnLoad(EventArgs e)
		{            
			base.OnLoad (e);

			// Overridden to handle Help Topic F1 functionality
			if (HelpControlType != HelpControlTypes.HelpLink && 
			    HelpTopic != null && HelpTopic != "")

                Page.ClientScript.RegisterStartupScript(GetType(), "F1HelpHandler",
					@"<script>
function OpenHelp(e) 
{
alert('opening');
	window.open('"  + FormatHelpUrl(HelpTopic) + @"','WebStoreHelp','toolbar=no,top=10,left=10,width=600,height=600,resizable=yes,status=yes,scrollbars=yes');	
	e.cancelBubble = true;
	e.stopPropagation();
	return false;
}
function OpenHelpTopic(HelpTopic) 
{
	window.open('" + ResolveUrl(HelpBaseUrl) + @"' +  HelpTopic,'WebStoreHelp','toolbar=no,top=10,left=10,width=600,height=600,resizable=yes,status=yes,scrollbars=yes');
	return false;
}
function KeyDownHandler(e) 
{
	if (! e )  // IE uses event object
		e = window.event;
	
	if (e.keyCode == 112)  // F1
	{
       	e.cancelBubble = true;
		e.stopPropagation();
        e.returnValue = false;
        OpenHelp(e);
	}
}	
	
    if ( window.navigator.userAgent.indexOf('MSIE') == -1 )
        document.onkeydown = KeyDownHandler;
    else
	    document.onhelp = OpenHelp;
</script>
");
		}


	}

	/// <summary>
	/// Determines whether link is treated as a link or as a page F1 handler.
	/// </summary>
	public enum HelpControlTypes 
	{
		/// <summary>
		/// A Help link is displayed
		/// </summary>
		HelpLink,
		/// <summary>
		/// This control only tries to handle page F1 access
		/// </summary>
		F1Handler,
		/// <summary>
		/// This control should display both a help link
		/// and handle the F1 key
		/// </summary>
		HelpLinkAndF1Handler
	}
}
