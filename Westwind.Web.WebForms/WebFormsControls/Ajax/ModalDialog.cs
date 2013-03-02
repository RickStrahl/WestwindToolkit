
#region License
/*
 **************************************************************
 *  Author: Rick Strahl 
 *          © West Wind Technologies, 2008 - 2011
 *          http://www.west-wind.com/
 * 
 * Created: 09/18/2008
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
using System.Text;

using System.ComponentModel;
using System.Web.UI;
using System.Drawing;
using System.Drawing.Design;
using System.Globalization;
using Westwind.Utilities;

namespace Westwind.Web.Controls
{
    /// <summary>
    /// This control allows creating client side pop up window that blocks out the 
    /// background and brings a dialog to the foreground. This control can be  used
    ///  to replace ugly alert or prompt boxes to pop up rich dialogs that must  be
    ///  edited modally. Any dialog can be popped up like 
    /// 
    /// The background can be just an opaque color with a transparency value - the 
    /// default is black at 70% for example which gives a grey looking overlay - or
    ///  can be a custom Html element that is expanded and then made transparent as
    ///  in the following figure.
    /// 
    /// &lt;&lt;img src="images/wwModalDialog.png"&gt;&gt;
    /// 
    /// The control is based on DragPanel so the dialogs can be draggable and 
    /// closable. Events are available to fire when a button or the close button is
    ///  clicked with event firing either on the client or server.
    /// 
    /// The client side counter part class can be used without this server side  
    /// control and allows a few extra features, such as a mechanism to display a  
    /// dialog without controls on the page to map to.
    /// </summary>
    [DefaultProperty("ContentId"),ToolboxBitmap(typeof(System.Web.UI.WebControls.WebParts.PageCatalogPart)),
     ToolboxData("<{0}:ModalDialog runat=\"server\"\r\nstyle=\"background:white;display:none;\" ContentId=\"MessageBoxContent\" HeaderId=\"MessageBoxContent\">\r\n<div id=\"MessageBoxHeader\" class='gridheader'>Header</div>\r\n<div style=\"padding:10px;\">\r\n<div id=\"MessageBoxContent\"></div>\r\n\r\n</div>\r\n</{0}:ModalDialog>")]
    public class ModalDialog : DragPanel
    {

        /// <summary>
        /// The opacity of of the overlay background in a decimal percentage. Default to .85
        /// </summary>    
        [Description("The opacity of of the overlay background in a decimal percentage.)"),
         DefaultValue(typeof(decimal),".75"), 
        Category("Modal")]
        public decimal BackgroundOpacity
        {
            get { return _BackgroundOpacity; }
            set { _BackgroundOpacity = value; }
        }
        private decimal _BackgroundOpacity = .75M;

        /// <summary>
        /// The client ID of the element that receives the content message. 
        /// If not specified the message is written to the body of the control.
        /// </summary>
        [Description("Optional Id of the content area that is set when showDialog is called. If not provided the main dialog is updated)"),
         DefaultValue(""), Category("Modal")]
        public string ContentId
        {
            get { return _ContentId; }
            set { _ContentId = value; }
        }
        private string _ContentId = "";

        /// <summary>
        /// ID of a header element that receives the Title when calling showDialog()
        /// on the client. Optional - if not specified the header is not set which 
        /// means the dialog displays as designed.
        /// </summary>
        [Description("Optional Id of the header area that is set when showDialog is called. If not no title is set"),
         Category("Modal"),DefaultValue("")]
        public string HeaderId
        {
          get { return _HeaderId; }
          set { _HeaderId = value; }
        }
        private string _HeaderId = "";

        /// <summary>
        /// Optional Id that is to be used for the shaded Overlay. This allows you
        /// to create a colored or otherwise designed background that pops over the
        /// existing content.
        /// </summary>
        [Description("Optional client ID for a DOM element to be used for the overlay. If not specified a default element will be created.)"),
         DefaultValue(""), Category("Modal")]        
        public string OverlayId
        {
            get { return _OverlayId; }
            set { _OverlayId = value; }
        }
        private string _OverlayId = "";

        /// <summary>
        /// When true fades in the background by slowly increasing
        /// opacity of the background
        /// </summary>
        [Description("When true fades the background in slowly increasing the opacity."),
         DefaultValue(false),Category("Modal")]
        public bool FadeinBackground
        {
            get { return _FadeinBackground; }
            set { _FadeinBackground = value; }
        }
        private bool _FadeinBackground = false;
        

        /// <summary>
        /// Client side event handler that is fired when anything is clicked inside of the
        /// dialog. This can be a button, a hyperlink or any element.
        /// 
        /// The even fired will be a jQuery style event with a jQuery event object passed.
        /// this points at the clicked element.
        /// 
        /// Handler code should check for specific elements like buttons clicked and
        /// then based on the id or value decide what action to take.
        /// </summary>
        [Description("Optional Client Event Handler for that traps button clicks. Handler receives jQuery event info in the context of the button (this set to element, event parameter passed)"),
        DefaultValue(""), Category("Client Events")]
        public override string ClientDialogHandler
        {
            get { return base.ClientDialogHandler;  }
            set { base.ClientDialogHandler = value; }
        }        

        /// <summary>
        /// The zIndex value for the overlay and dialog. This value must be 
        /// higher than any other control on the page in order for the 
        /// dialog to pop up on top. 
        /// </summary>
        [Description("The zIndex for the overlay and dialog.)"),
        DefaultValue(10000), Category("Modal")]
        public int zIndex
        {
            get { return _zIndex; }
            set { _zIndex = value; }
        }
        private int _zIndex = 10000;


        /// <summary>
        /// Override to force simple IDs all around
        /// </summary>
        public override string UniqueID
        {
            get
            {
                return ID;
            }
        }

        /// <summary>
        /// Override to force simple IDs all around
        /// </summary>
        public override string ClientID
        {
            get
            {
                return ID;
            }
        }

        public ModalDialog()
        {
            Draggable = false;
        }

        
        protected override void OnLoad(EventArgs e)
        {
            if (Visible)
            {
                StringBuilder script = new StringBuilder();

                script.Append("'#" + ClientID + "',{");

                if (!string.IsNullOrEmpty(HeaderId))
                    script.Append("headerId: '" + HeaderId + "',");

                if (!string.IsNullOrEmpty(ContentId))
                    script.Append("contentId: '" + ContentId + "',");

                if ( !string.IsNullOrEmpty(ClientDialogHandler) )
                {
                    script.Append("dialogHandler:" + ClientDialogHandler + ",");

                    // Clear out the handler so the dragpanel doesn't implement it
                    ClientDialogHandler = null;
                }

                if (FadeinBackground)
                    script.Append("fadeInBackground: true,");

                if ( !string.IsNullOrEmpty(OverlayId) )
                    script.Append("overlayId: '" + OverlayId + "',");

                script.Append("backgroundOpacity:" + BackgroundOpacity.ToString(CultureInfo.InvariantCulture) + ",");


                string scrpt = "var " + ID + " =  new _ModalDialog(" + script.ToString().TrimEnd(',') + "});\r\n";

                ScriptProxy.RegisterStartupScript(this, typeof(WebResources), ID + "_ModalStart", scrpt, true);
            }

           
            base.OnLoad(e);
        }


        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            if (Closable)
            {
                string script = "\r\n$( function() { $('#" + DragHandleID + "').find('img').click( function(e) { " + ClientID + ".hide(); } );})\r\n";
                ScriptProxy.RegisterStartupScript(this, typeof(WebResources), ID + "_ModalCloseHandler", script, true);
            }


            if (!string.IsNullOrEmpty(ShowScript))
                ScriptProxy.RegisterStartupScript(this, typeof(WebResources), ID + "_ModalShow", ShowScript, true);
        }
        private string ShowScript = null;


        /// <summary>
        /// Shows the dialog as designed without any customizations
        /// </summary>
        public void Show()
        {
            Show(null,null,false);
        }

        /// <summary>
        /// A server side display method that forces the modal dialog to be to be displayed
        /// when the page loads. 
        /// 
        /// Note the page still loads and when you click out of the dialog you will
        /// be back on the original page. IOW, it's still full client side behavior
        /// even though activated through the server.
        /// </summary>
        /// <param name="Message"></param>
        /// <param name="Title"></param>
        /// </summary>
        /// <param name="message"></param>
        /// <param name="title"></param>
        /// <param name="okButtonText"></param>
        /// <param name="cancelButtonText"></param>
        public void Show(string message, string title, bool asHtml)
        {
            //message = message ?? "null";
            //title = title ?? "null";            

            // Hooks up window.load event handler - we'll need to load the page first
            // or IE will choke on the script execution.
            ShowScript = string.Format(
@"$(function() {{ {0}.show({1},{2},{3}); }});
", ID, WebUtils.EncodeJsString(message), WebUtils.EncodeJsString(title), asHtml.ToString().ToLower() );

        }

    }
}
