#region License
/*
 **************************************************************
 *  Author: Rick Strahl 
 *          © West Wind Technologies, 2008 - 2011
 *          http://www.west-wind.com/
 * 
 * Created: 09/30/2011
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
using System.Web.UI.WebControls;
using System.ComponentModel;
using System.Drawing;
using System.Web.UI;

namespace Westwind.Web.Controls
{

    /// <summary>
    /// A WebForms wrapper control around the jQuery UI AutoComplete control. This control 
    /// provides code based AJAX callbacks either to a specified service URL (which allows calls
    /// to a CallbackHandler HTTP Handler service for example) or directly to the page using an
    /// event based callback to the controls CallbackHandler event.
    /// </summary>
    [ToolboxBitmap(typeof(System.Web.UI.WebControls.DropDownList)), DefaultProperty("Text"),
    ToolboxData("<{0}:AutoComplete runat=\"server\"  />")]
    public class AutoComplete : TextBox
    {
        public AutoComplete()
        {
        }

        /// <summary>
        /// Service Url to explicitly call to retrieve data. Not used if the OnServiceHandlerCallback event is set.
        /// Alternately you can implement the CallbackHandler delegate.
        /// </summary>
        [Description("Service Url to explicitly call to retrieve data. Not used if the OnServiceHandlerCallback event is set.")]
        [Category("Callback"), DefaultValue("")]
        public string ServerUrl
        {
            get { return _ServerUrl; }
            set { _ServerUrl = value; }
        }
        private string _ServerUrl = "";


        /// <summary>
        /// An EventHandler that allows you to serve AJAX data to the AutoComplete 
        /// client control. The handler receives a string input of the search term
        /// typed into the control and should return an array of objects.
        /// Each object should have at minimum 'label' and 'value' properties.
        /// 
        /// This event handler should be set in OnInit of page/control
        /// </summary>
        public Func<string, object> CallbackHandler
        {
            get { return _CallbackHandler; }
            set { _CallbackHandler = value; }
        }
        private Func<string, object> _CallbackHandler = null;


        
        /// <summary>
        /// Determines whether the selected item in the list will automatically be focused
        /// </summary>
        [Description("Determines whether the selected item in the list will automatically be focused")]
        [Category("Display"), DefaultValue(true)]
        public Boolean AutoFocus
        {
            get { return _AutoFocus; }
            set { _AutoFocus = value; }
        }
        private Boolean _AutoFocus = true;


        
        /// <summary>
        /// The delay in milliseconds the Autocomplete waits after a keystroke to activate itself. A zero-delay makes sense for local data (more responsive), but can produce a lot of load for remote data, while being less responsive.
        /// </summary>
        [Description("The delay in milliseconds the Autocomplete waits after a keystroke to activate itself. A zero-delay makes sense for local data (more responsive), but can produce a lot of load for remote data, while being less responsive.")]
        [Category("Display"), DefaultValue(300)]        
        public int Delay
        {
            get { return _Delay; }
            set { _Delay = value; }
        }
        private int _Delay = 300;
        
        /// <summary>
        /// The minimum length of the input string before autocomplete kicks in
        /// </summary>
        [Description("The minimum length of the input string before autocomplete kicks in")]
        [Category("Display"), DefaultValue(1)]
        public int MinLength
        {
            get { return _MinLength; }
            set { _MinLength = value; }
        }
        private int _MinLength = 1;       

        /// <summary>
        /// Theme applied to the base CSS url. Replaces /base/ with the theme selected
        /// </summary>
        [Category("Resources"),
         Description("Theme applied to the base CSS url. Replaces /base/ with the theme selected"),
         DefaultValue("Redmond")]
        public string Theme
        {
            get { return _Theme; }
            set { _Theme = value; }
        }
        private string _Theme = "Redmond";


        /// <summary>
        /// The path to the base CSS Theme. Path is adjusted 
        /// </summary>
        [Category("Resources"),
         Description("jQuery UI's base Css path to the base theme. The Theme is applied against this path."),
         DefaultValue("~/scripts/themes/base/jquery.ui.all.css")]
        public string CssBasePath
        {
            get { return _CssPath; }
            set { _CssPath = value; }
        }
        private string _CssPath = "~/scripts/themes/base/jquery.ui.all.css";


        /// <summary>
        /// The client selection handler called when a selection is made.
        /// The handler receives two parameter - a ssda
        /// </summary>
        [Description("The client selection handler called when a selection is made")]
        [Category("Client Events"), DefaultValue("")]
        public string OnClientSelection
        {
            get { return _OnClientSelect; }
            set { _OnClientSelect = value; }
        }
        private string _OnClientSelect = string.Empty;


        protected AjaxMethodCallback AjaxMethodCallback = null;


        /// <summary>
        /// Code that embeds related resources (.js and css)
        /// </summary>
        /// <param name="scriptProxy"></param>
        protected void RegisterResources(ClientScriptProxy scriptProxy)
        {
            // Use ScriptProxy to load jQuery and ww.Jquery
            ScriptLoader.LoadjQuery(this);
            ScriptLoader.LoadjQueryUi(this, null);

            // if a theme is provided embed a link to the stylesheet
            if (!string.IsNullOrEmpty(Theme))
            {

                string cssPath = this.CssBasePath.Replace("/base/", "/" + Theme + "/");
                scriptProxy.RegisterCssLink(Page, typeof(WebResources), cssPath, cssPath);
            }
        }


        protected override void OnInit(EventArgs e)
        {
            // Hook up an AjaxMethodCallback control for the autocomplete handler
            this.Page.PreLoad += (obj, ev) =>
            {
                AjaxMethodCallback = AjaxMethodCallback.CreateControlInstanceOnPage(this);
                AjaxMethodCallback.PageProcessingMode = CallbackProcessingModes.PageLoad;
            };
        }


        /// <summary>
        /// Most of the work happens here for generating the hook up script code
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPreRender(EventArgs e)
        {
            // Ignore if we're calling this in a Callback
            if (AjaxMethodCallback.IsCallback)
                return;

            base.OnPreRender(e);

            // MS AJAX aware script management
            ClientScriptProxy scriptProxy = ClientScriptProxy.Current;

            // Register resources
            RegisterResources(scriptProxy);

            // Capture and map the various option parameters
            StringBuilder sbOptions = new StringBuilder(512);
            sbOptions.Append("{");


            if (!string.IsNullOrEmpty(OnClientSelection))
                sbOptions.Append("select: " + OnClientSelection + ",");
            else
                sbOptions.AppendLine(
                    @"select: function (e, ui) {
                        $(""#" + this.UniqueID + @""").val(ui.item.value);
                    },");

            
            if (CallbackHandler != null)
            {
                // point the service Url back to the current page method 
                if (AjaxMethodCallback != null)
                    ServerUrl = Page.Request.Url.LocalPath + "?" + "Method=AutoCompleteCallbackHandler&CallbackTarget=" + AjaxMethodCallback.ID ;
            }

            if ( !string.IsNullOrEmpty(ServerUrl)  )
                sbOptions.Append("source: \"" + ServerUrl + "\",");

            sbOptions.AppendFormat("autoFocus: {0},delay: {1},minLength: {2},",AutoFocus.ToString().ToLower(), Delay, MinLength);

            // strip off trailing ,            
            sbOptions.Length--;

            sbOptions.Append("}");

            // Write out initilization code for calendar
            StringBuilder sbStartupScript = new StringBuilder(400);
            sbStartupScript.AppendLine("$(document).ready( function() {");

            sbStartupScript.AppendFormat("      var autocompl = $(\"#{0}\").autocomplete({1});\r\n",
                                          ClientID, sbOptions.ToString());

            // close out the script function
            sbStartupScript.AppendLine("} );");

            scriptProxy.RegisterStartupScript(Page, typeof(WebResources), "_cal" + UniqueID,
                 sbStartupScript.ToString(), true);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="writer"></param>
        public override void RenderControl(HtmlTextWriter writer)
        {
            // Render the base text box
            base.RenderControl(writer);            
        }

        /// <summary>
        /// Method that handles the actual callback routing.
        /// </summary>
        /// <param name="term"></param>
        /// <returns></returns>
        [CallbackMethod]
        public object AutoCompleteCallbackHandler(string term)
        {
            if (string.IsNullOrEmpty(term))
                term = this.Page.Request.Params["term"];

            // Call the delegate to handle the actual work
            // of creating the JSON object
            if (this.CallbackHandler == null)
                return null;

            object obj = this.CallbackHandler(term);
            return obj;
        }
    }
}
