#region License
/*
 **************************************************************
 *  Author: Rick Strahl 
 *          © West Wind Technologies, 2008 - 2011
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
using System.Web.UI.WebControls;
using System.Web.UI;
using System.Drawing;
using System.ComponentModel;
using System.Globalization;
using System.Drawing.Design;
using System.Text;

namespace Westwind.Web.Controls
{
    /// <summary>
    /// A draggable Panel control that can be dragged around the current browser window.
    /// Dragging is supported for the panel and initiated through a drag handle control - 
    /// a control that is contained in the window and acts as the draggable hot spot that
    /// initiates dragging. You can also make the Panel itself the drag handle.
    /// 
    /// The control can also optionally display a close button that allows hiding the control
    /// by changing its client side visibility.
    /// </summary>
    [ToolboxBitmap(typeof(Panel)),
    ToolboxData("<{0}:DragPanel runat=\"server\"><%-- Drag Handle: Make sure to assign Unique ID and set matching DragHandleID --%><div runat=\"server\"></div><%-- Content Panel: Assign Unique ID --%><div></div></{0}:DragPanel>")]
    public class DragPanel : Panel
    {
        /// <summary>
        /// The ID of the control that is used as the drag handle to initiate a drag operation.
        /// </summary>
        [Description("The ID of the control that is used as the drag handle to initiate a drag operation")]
        [DefaultValue("")]
        [Category("Dragging")]
        public string DragHandleID
        {
            get { return _DragHandleID; }
            set { _DragHandleID = value; }
        }
        private string _DragHandleID = "";

        /// <summary>
        /// Determines whether this control is draggable
        /// </summary>
        [Description("Determines whether this control is draggable")]
        [DefaultValue(true), Category("Dragging")]
        public bool Draggable
        {
            get { return _Draggable; }
            set { _Draggable = value; }
        }
        private bool _Draggable = true;

        /// <summary>
        /// The cursor property for the dragged handle or object
        /// </summary>
        [Description("The cursor property for the dragged handle or object")]
        [DefaultValue("move"), Category("Dragging")]
        public string Cursor
        {
            get { return _Cursor; }
            set { _Cursor = value; }
        }
        private string _Cursor = "move";


        
        /// <summary>
        /// Delay before dragging starts in milliseconds
        /// </summary>
        [Description("Delay before dragging starts in milliseconds")]
        [Category("Miscellaneous"), DefaultValue(100)]
        public int DragDelay
        {
            get { return _DragDelay; }
            set { _DragDelay = value; }
        }
        private int _DragDelay = 100;



        /// <summary>
        /// Flag that determines whether a closebox is rendered into the &lt;div&gt; 
        /// specified as a drag handle.        
        /// </summary>
        [Category("Panel Display")]
        [Description("Flag that determines whether a closebox is rendered into the <div> specified as a drag handle.")]
        [DefaultValue(false)]
        public bool Closable
        {
            get { return _ShowCloseBoxOnDragHandle; }
            set { _ShowCloseBoxOnDragHandle = value; }
        }
        private bool _ShowCloseBoxOnDragHandle = false;

        /// <summary>
        /// Optional Image used for close box if ShowCloseBoxOnDragHandle is true.
        /// </summary>
        [Category("Panel Display")]
        [Description("Optional Image used for close box if ShowCloseBoxOnDragHandle is true.")]
        [DefaultValue("WebResource")]
        public string CloseBoxImage
        {
            get { return _CloseBoxImage; }
            set { _CloseBoxImage = value; }
        }
        private string _CloseBoxImage = "WebResource";

        /// <summary>
        /// Fades out the window when it is closed with the close button
        /// </summary>
        [Description("Fades out the window when it is closed with the close button")]
        [Category("Panel Display"), DefaultValue(false)]
        public bool FadeOnClose
        {
            get { return _FadeOnClose; }
            set { _FadeOnClose = value; }
        }
        private bool _FadeOnClose = false;


       /// <summary>
        /// Optional Opacity level in decimal percentage values (ie. 0.65 for 65%) for the panel background. Supported only in Mozilla and IE browsers. The value is given as fractional percentage.
        /// </summary>
        [Description("Optional Opacity level in full percentage points for the panel background. Supported only in Mozilla and IE browsers. The value is given as fractional percentage."),
        Category("Panel Display"), DefaultValue(typeof(decimal),"1.0")]
        public decimal PanelOpacity
        {
            get { return _PanelOpacity; }
            set { _PanelOpacity = value; }
        }
        private decimal _PanelOpacity = 1.0M;


        /// <summary>
        /// Optionally used to specify a shadow below the panel. If 0 no shadow is created. If greater than 0 the panel is rendered.
        /// </summary>
        [Description("Optionally used to specify a shadow below the panel. If 0 no shadow is created. If greater than 0 the panel is rendered."),
        Category("Panel Display"), DefaultValue(0)]
        public int ShadowOffset
        {
            get { return _PanelShadowOffset; }
            set { _PanelShadowOffset = value; }
        }
        private int _PanelShadowOffset = 0;

        /// <summary>
        /// The opacity of the Panel's shadow if PanelShadoOffset is set.
        /// </summary>
        [Description("The opacity of the Panel's shadow if PanelShadoOffset is set."),
        Category("Panel Display"), DefaultValue(typeof(decimal),".25")]
        public decimal ShadowOpacity
        {
            get { return _PanelShadowOpacity; }
            set { _PanelShadowOpacity = value; }
        }
        private decimal _PanelShadowOpacity = .25M;

        
        /// <summary>
        /// Determines whether the panel is centered in the page
        /// </summary>
        [Description("Determines whether the panel is centered in the page")]
        [Category("Miscellaneous"), DefaultValue(false)]
        public bool Centered
        {
            get { return _Centered; }
            set { _Centered = value; }
        }
        private bool _Centered = false;



        /// <summary>
        /// Client side event handler that is called whenever a click event occurs anywhere inside of the
        /// modal dialog.
        /// 
        /// Client handler is a jQuery event handler and receives this as the element clicked plus
        /// the standard jQuery event object. You can check id and match against specific 
        /// control ids:
        /// 
        /// if (id == "btnClose") doA();
        /// if (id == "btnCancel") doB();
        /// </summary>
        [Description("Client side event handler called when the panel is closed via the Closebox"),
         Category("Client Events")]
        public virtual string ClientDialogHandler
        {
            get { return _ClientDialogHandler; }
            set { _ClientDialogHandler = value; }
        }
        private string _ClientDialogHandler = "";


        /// <summary>
        /// Determines where the ww.jquery.js resource is loaded from. WebResources, Url or an empty string (no resource loaded)
        /// </summary>
        [Description("Determines where the ww.jquery.js resource is loaded from. WebResources, Url or leave empty to do nothing"),
        DefaultValue("WebResource"), Category("Resources"),
        Editor("System.Web.UI.Design.UrlEditor", typeof(UITypeEditor))]
        public string ScriptLocation
        {
            get { return _ScriptLocation; }
            set { _ScriptLocation = value; }
        }
        private string _ScriptLocation = "WebResource";


        /// <summary>
        /// Determines where the jquery.js resource is loaded from. WebResources, Url or leave empty to do nothing
        /// </summary>
        [Description("Determines where the jquery.js resource is loaded from. WebResources, Url or leave empty to do nothing"),
        DefaultValue("WebResource"), Category("Resources"),
        Editor("System.Web.UI.Design.UrlEditor", typeof(UITypeEditor))]
        public string jQueryScriptLocation
        {
            get { return _jQueryScriptLocation; }
            set { _jQueryScriptLocation = value; }
        }
        private string _jQueryScriptLocation = "WebResource";

        /// <summary>
        /// Internal reference of the Client Script Proxy - set up in OnInit
        /// </summary>
        protected ClientScriptProxy ScriptProxy = null;

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            
            if (!DesignMode)
            {
                ScriptProxy = ClientScriptProxy.Current;

                // Use ScriptProxy to load jQuery and ww.Jquery
                if (!string.IsNullOrEmpty(jQueryScriptLocation))
                    ScriptLoader.LoadjQuery(this);

                ScriptLoader.LoadwwjQuery(this, false);                
            }
        }

        protected override void OnPreRender(EventArgs e)
        {
            StringBuilder startupScript = new StringBuilder(2048);

            string DhId = DragHandleID;
            if (string.IsNullOrEmpty(DragHandleID))
                DragHandleID = ClientID;

            Control Ctl = FindControl(DragHandleID);
            if (Ctl != null)
                DhId = Ctl.ClientID;

            startupScript.AppendLine("\t$('#" + ClientID + "')");

            if (Closable && !string.IsNullOrEmpty(DragHandleID) )
            {
                string imageUrl = CloseBoxImage;
                if (imageUrl == "WebResource" )
                    imageUrl = ScriptProxy.GetWebResourceUrl(this, GetType(), WebResources.CLOSE_ICON_RESOURCE);
                
                StringBuilder closableOptions = new StringBuilder("imageUrl: '" + imageUrl + "'");

                if (!string.IsNullOrEmpty(DragHandleID))
                    closableOptions.Append(",handle: $('#" + DragHandleID + "')");

                if (!string.IsNullOrEmpty(ClientDialogHandler))
                    closableOptions.Append(",handler: " + ClientDialogHandler);
       
                if (FadeOnClose)
                    closableOptions.Append(",fadeOut: 'slow'");
                
                startupScript.AppendLine("\t\t.closable({ " + closableOptions + "})");
            }
            
            string options = "";            

            if (Draggable)
            {
                // force auto stacking of windows (last dragged to top of zIndex)
                options = "{  stack: \"*\", opacity: 0.80, dragDelay: " + DragDelay.ToString();
                
                if (!string.IsNullOrEmpty(DragHandleID))
                    options += ",handle:'#" + DragHandleID + "'";

                if (!string.IsNullOrEmpty(Cursor))
                    options += ",cursor:'" + Cursor + "'";
                
                options += " }";

                startupScript.AppendLine("\t\t.draggable(" + options + " )");
            }

            if (ShadowOffset != 0)
            {
                startupScript.AppendLine(
                    "\t\t.shadow({ opacity:" +
                               ShadowOpacity.ToString(CultureInfo.InvariantCulture.NumberFormat) +
                               ",offset:" +
                               ShadowOffset.ToString() + "})");
            }

            if (Centered)
            {
                startupScript.AppendLine(
                    "\t\t.centerInClient()");
            }

            startupScript.Length = startupScript.Length - 2;  // strip last CR/LF \r\n
            startupScript.AppendLine(";");

            string script = "$( function() {\r\n" + startupScript + "});";

            ScriptProxy.RegisterStartupScript(this, GetType(), ID + "_DragBehavior",
                                              script, true);                
            
            base.OnPreRender(e);
        }
    }
}
