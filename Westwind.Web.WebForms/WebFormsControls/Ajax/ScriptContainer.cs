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
using System.Collections.Generic;
using System.Web.UI;
using System.ComponentModel;
using System.Web.UI.HtmlControls;
using System.Web;
using System.Web.UI.Design;
using Westwind.Utilities;
using System.IO;
using System.Diagnostics;
using Westwind.Web.WebForms.Properties;

namespace Westwind.Web.Controls
{
    /// <summary>
    /// Class that provides script embedding functionality to ASP.NET pages. 
    /// Features include the ability to use ResolveUrl style src urls to place 
    /// script includes inline (to the control), in the Header or using standard 
    /// ASP.NET ClientScript or ScriptManager. The control can also optimize 
    /// 
    /// scripts if a .min.js script file is available by using the AllowMinScript 
    /// property on the script.
    /// 
    /// The purpose of this control is to provide Intellisense compatible script 
    /// embedding without requiring the ASP.NET AJAX ScriptManager control since 
    /// that control automatically includes MS AJAX scripts even if none of the MS 
    /// AJAX features are otherwise used.
    /// 
    /// Using markup the control can embed scripts like this:
    /// 
    /// &lt;&lt;code lang="HTML"&gt;&gt;&lt;ww:ScriptContainer ID="scripts" 
    /// runat="server" RenderMode="Header"&gt;
    ///     &lt;Scripts&gt;
    ///         &lt;Script Src="Scripts/jquery.js"  
    /// Resource="jquery"&gt;&lt;/Script&gt;
    ///         &lt;Script Src="Scripts/ui.core.js"  
    /// AllowMinScript="true"&gt;&lt;/Script&gt;
    ///         &lt;Script Src="Scripts/ui.draggable.js"  &gt;&lt;/Script&gt;
    ///         &lt;Script Src="Scripts/wwscriptlibrary.js"  
    /// Resource="wwscriptlibrary"&gt;&lt;/Script&gt;
    ///         &lt;Script Resource="WebResources.Menu.js" 
    /// ResourceControl="txtInput" /&gt;
    ///     &lt;/Scripts&gt;
    /// &lt;/ww:ScriptContainer&gt;
    /// &lt;&lt;/code&gt;&gt;
    /// 
    /// The options on the &lt;Script&gt; tag can be found in the 
    /// <see cref="_2EU16Y1L1">ScriptItem class</see>. Unfortunately due to the requirement for an 
    /// HtmlGeneric control (so Intellisense still works for scripts) there's no 
    /// Intellisense for the properties of Script elements in markup. They do work 
    /// however.
    /// 
    /// Using CodeBehind .NET code, the static Singleton ScriptContainer.Current 
    /// can be used to add scripts (even if no script instance pre-exists):
    /// 
    /// &lt;&lt;code lang="C#"&gt;&gt;
    /// ScriptContainer script = ScriptContainer.Current;
    /// script.AddScript("~/scripts/wwEditable.js","jquery");  // as  known 
    /// resource
    /// script.AddScript("~/scripts/wwEditable.js", true);  // as .min.js
    /// &lt;&lt;/code&gt;&gt;
    /// 
    /// Markup scripts always have precendence over scripts embedded in code in 
    /// terms of rendering order, but you can choose where scripts are rendered to 
    /// individually - Header, Script, Inline or the default of InheritFromControl.
    ///  This allows some control over where scripts are loaded.
    /// <seealso>Class ScriptItem</seealso>
    /// </summary>
    /// <remarks>
    /// Only one instance of this component can exist on the page otherwise an 
    /// exception is thrown.
    /// </remarks>
    [NonVisualControl, Designer(typeof(ScriptContainerDesigner))]
    [ParseChildren(true, "Scripts")]
    [PersistChildren(false)]
    [DefaultProperty("Scripts")]
    [ToolboxData("<{0}:ScriptContainer runat=\"server\">\r\n\t<Scripts>\r\n\t</Scripts></{0}:ScriptContainer>")]
    public class ScriptContainer : Control
    {
        private const string STR_CONTEXTID = "ScriptContainer";

        public ScriptContainer()
        {
            _Scripts = new List<HtmlGenericControl>();
            _InternalScripts = new List<ScriptItem>();
        }


        public override void Dispose()
        {
            base.Dispose();
            this.Page.Items.Remove(STR_CONTEXTID);
            
            //if (HttpContext.Current != null)
            //    HttpContext.Current.Items.Remove(STR_CONTEXTID);
        }


        /// <summary>
        /// Returns a current instance of this control if an instance
        /// is already loaded on the page. Otherwise a new instance is
        /// created, added to the Form and returned.
        /// 
        /// It's important this function is not called too early in the
        /// page cycle - it should not be called before Page.OnInit().
        /// 
        /// This property is the preferred way to get a reference to a
        /// ScriptContainer control that is either already on a page
        /// or needs to be created. Controls in particular should always
        /// use this property.
        /// </summary>
        public static ScriptContainer Current
        {
            get
            {
                // We need a context for this to work!
                if (HttpContext.Current == null)
                    return null;

                Page page = HttpContext.Current.CurrentHandler as Page;
                if (page == null)
                    throw new InvalidOperationException(Resources.ERROR_ScriptContainer_OnlyWorks_With_PageBasedHandlers); 

                ScriptContainer ctl = null;

                // Retrieve the current instance
                ctl = page.Items[STR_CONTEXTID] as ScriptContainer;
                if (ctl != null)
                    return ctl;
                
                ctl = new ScriptContainer();                
                page.Form.Controls.Add(ctl);

                return ctl;
            }
        }
 

        /// <summary>
        /// Collection of ScriptItems
        /// </summary>
        [Description("Collection of ScriptItems")]
        [Category("Script")]        
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [PersistenceMode(PersistenceMode.InnerProperty)]
        public List<HtmlGenericControl> Scripts
        {
            get { return _Scripts; }
            // IMPORTANT: If set method exists error occurs during designer loading.
            // set { _ScriptItems = value; }  
        }
        private List<HtmlGenericControl> _Scripts = null;

        /// <summary>
        /// Internally stored list of parsed or manually added Scripts
        /// </summary>
        protected List<ScriptItem> InternalScripts
        {
            get { return _InternalScripts; }
            //set { _InternalScripts = value; }
        }
        private List<ScriptItem> _InternalScripts = null;

        /// <summary>
        /// Determines where scripts are rendered by default. Defaults to script which renders using ClientScript or ScriptManager.
        /// </summary>
        [Category("Script")]
        [DefaultValue(typeof(ScriptRenderModes),"Script")]
        public ScriptRenderModes RenderMode
        {
            get { return _RenderMode; }
            set { _RenderMode = value; }
        }
        private ScriptRenderModes _RenderMode = ScriptRenderModes.Script;

        /// <summary>
        /// Internally tracked client script proxy that goes either
        /// to MS Ajax ScriptManager if available or to Page.ClientScript 
        /// </summary>
        private ClientScriptProxy scriptProxy = null;



        /// <summary>
        /// Script extension for minimized or packed scripts. Used only
        /// for entries that AllowMinScript=True.
        /// </summary>
        [Description("Script extension for minimized or packed scripts. Used on entries that have AllowMinScript=True")]
        [Category("Script"), DefaultValue(".min.js")]
        public string MinScriptExtension
        {
            get { return _MinScriptExtension; }
            set { _MinScriptExtension = value; }
        }
        private string _MinScriptExtension = ".min.js";


        /// <summary>
        /// Read the HtmlGeneric Script controls and parse them into
        /// Internal scripts at page load time
        /// </summary>
        /// <param name="e"></param>
        protected override void OnInit(EventArgs e)
        {

            // Save a Per Request instance in Page.Items so we can retrieve it
            // generically from code with wwScriptContainer.Current
            
            if (!Page.Items.Contains(STR_CONTEXTID))
                Page.Items[STR_CONTEXTID] = this;
            else
            {
                // ScriptContainer already exists elsewhere on the page - combine scripts
                ScriptContainer container = Page.Items[STR_CONTEXTID] as ScriptContainer;
                foreach(HtmlGenericControl scriptItem in container.Scripts)
                {
                    this.Scripts.Add(scriptItem);
                }
            }

            base.OnInit(e);

            scriptProxy = ClientScriptProxy.Current;

            // Pick up HtmlGeneric controls parse into Script objects
            // and add to Internal scripts for final processing
            foreach (HtmlGenericControl ctl in Scripts)
            {
                // Take generic control and parse into Script object
                ScriptItem script = ParseScriptProperties(ctl);
                if (script == null)
                    continue;

                // Insert into the list at the head of the list before 'manually'
                // added script entries
                //AddScript(script);

                // Register these items immediately at startup 
                // script refs defined in doc take precendence over code added ones
                RegisterScriptItem(script);
                
            }
        }

        /// <summary>
        /// Handle writing out scripts to header or 'ASP.NET script body'
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            // Use ClientScript Proxy for all script injection except inline
            scriptProxy = ClientScriptProxy.Current;

            // Now process all but inline scripts - 
            // inline scripts must be rendered in Render()
            foreach (ScriptItem script in InternalScripts)
            {
                RegisterScriptItem(script);
            }
        }

        /// <summary>
        /// Registers an individual script item in the page
        /// </summary>
        /// <param name="script"></param>
        private void RegisterScriptItem(ScriptItem script)
        {
            // Allow inheriting from control which is the default behavior
            if (script.RenderMode == ScriptRenderModes.Inherit)
                script.RenderMode = RenderMode;

            // Skip over inline scripts
            if (script.RenderMode == ScriptRenderModes.Inline)
                return;

            // special case jquery load from Content network
            if (ScriptLoader.jQueryLoadMode == jQueryLoadModes.ContentDeliveryNetwork &&
                script.Resource == WebResources.JQUERY_SCRIPT_RESOURCE || 
                script.Resource == "jquery" || script.Resource == "jquery.js" ||
                script.Src.ToLower().Contains("/jquery.js") )
            {
                ScriptLoader.LoadjQuery(Page);
                return;
            }

            // special case jquery-uiload from Content network
            if (script.Src.ToLower().Contains("/jquery-ui.") || script.Resource == "jqueryui" || script.Resource == "jquery-ui")
            {
                ScriptLoader.LoadjQueryUi(Page, null);
                return;
            }

            string src = string.Empty;
            Type ctlType = typeof(WebResources);

            if (script.ResourceControlType != null || !string.IsNullOrEmpty(script.ResourceControl))
            {
                if (script.ResourceControlType != null)
                    ctlType = script.ResourceControlType;
                else
                {
                    Control ctl = WebUtils.FindControlRecursive(Page, script.ResourceControl, false);
                    if (ctl != null)
                    {
                        ctlType = ctl.GetType();
                    }
                    else
                        throw new ArgumentException("Invalid Web Control passed for resource retrieval. Please pass a control from the assembly where the resource are located.");
                }
                src = scriptProxy.GetClientScriptResourceUrl(Page, ctlType, script.Resource);                
            }            
            else if (!string.IsNullOrEmpty(script.Resource))
            {
                src = scriptProxy.GetClientScriptResourceUrl(this, ctlType, script.Resource);            
            }
            else
            {
                src = ResolveUrl(script.Src);

                // Fix up the URL so we can allow ~/script syntax                    
                if (script.AllowMinScript && !HttpContext.Current.IsDebuggingEnabled)
                    src = src.ToLower().Replace(".js", MinScriptExtension);
            }

            // if there's a version number implied add a par
            if ( !string.IsNullOrEmpty(script.Version) )
            {
                if (src.Contains("?"))
                    src += "&ver=" + script.Version;
                else
                    src += "?ver=" + script.Version;
            }

            scriptProxy.RegisterClientScriptInclude(Page, ctlType, src, script.RenderMode);
        }

        /// <summary>
        /// Renders the scripts contained in this control
        /// </summary>
        /// <param name="writer"></param>
        protected override void Render(HtmlTextWriter writer)
        {
            // Use ClientScript Proxy for all script injection except inline
            ClientScriptProxy scriptProxy = ClientScriptProxy.Current;

            writer.WriteLine(); // empty line
            
            foreach (ScriptItem script in InternalScripts)
            {
                // We only need to handle inline here - others have been handled in OnPreRender
                if (script.RenderMode != ScriptRenderModes.Inline)
                    continue;

                // Allow inheriting from control which is the default behavior
                if (script.RenderMode == ScriptRenderModes.Inherit)
                    script.RenderMode = RenderMode;

                // Fix up url to allow ~/ syntax
                string src = ResolveUrl(script.Src);

                if (script.AllowMinScript && !HttpContext.Current.IsDebuggingEnabled)
                    src = src.ToLower().Replace(".js", MinScriptExtension);                

                writer.Write("<script src=\"" + src + "\" type=\"text/javascript\"></script>\r\n");
            }
        }


        /// <summary>
        /// Adds a script item to the page with full options
        /// </summary>
        /// <param name="scriptUrl">The Url to load script from. Can include ~/ syntax</param>
        /// <param name="renderMode">Determines where the script is rendered</param>
        /// <param name="allowMinScript">Determines if in non-debug mode .min.js files are used</param>
        public void AddScript(ScriptItem script)
        {
            ScriptItem match = null;
                 
            if (!string.IsNullOrEmpty(script.Src))
            {
                // Grab just the path
                script.FileId = Path.GetFileName(script.Src).ToLower();

                // Check if the script was already added to the page 
                match = InternalScripts.Find( item => item.FileId == script.FileId );
            }

            if (match == null)                
                InternalScripts.Add(script);
            else
                match = script;
        }



        /// <summary>
        /// Adds a script to the collection of embedded scripts
        /// </summary>
        /// <param name="scriptUrl"></param>
        /// <param name="renderMode"></param>
        /// <param name="allowMinScript"></param>
        public void AddScript(string scriptUrl, ScriptRenderModes renderMode, bool allowMinScript)
        {
            ScriptItem script = new ScriptItem();
            script.Src = scriptUrl;
            script.RenderMode = renderMode;
            script.AllowMinScript = allowMinScript;

            AddScript(script);
        }

        /// <summary>
        /// Adds a script to the page using the control's rendermode
        /// </summary>
        /// <param name="scriptUrl"></param>
        public void AddScript(string scriptUrl)
        {
            AddScript(scriptUrl, ScriptRenderModes.Inherit, false);            
        }
        /// <summary>
        /// Adds a script to the page using the control's rendermode and allows specifying of min.js script files in non-debug mode.
        /// </summary>
        /// <param name="scriptUrl"></param>
        /// <param name="allowMinScript"></param>
        public void AddScript(string scriptUrl, bool allowMinScript)
        {
            AddScript(scriptUrl, ScriptRenderModes.Inherit, allowMinScript);
        }

        /// <summary>
        /// Helper function that is used to load script resources for various AJAX controls
        /// Loads a script resource based on the following scriptLocation values:
        /// 
        /// * WebResource
        ///   Loads the Web Resource specified out of WebResources. Specify the resource
        ///   that applied in the resourceName parameter
        ///   
        /// * Url/RelativeUrl
        ///   loads the url with ResolveUrl applied
        ///   
        /// * empty (no value) 
        ///   No action is taken
        /// </summary>
        /// <param name="control">The control instance for which the resource is to be loaded</param>
        /// <param name="scriptLocation">WebResource, a Url or empty (no value)</param>
        /// <param name="resourceName">The name of the resource when WebResource is used for scriptLocation</param>
        /// <param name="topOfHeader">Determines if scripts are loaded into the header whether they load at the top or bottom</param>
        public void LoadControlScript(Control control, string scriptLocation, string resourceName, ScriptRenderModes renderMode)
        {
            ClientScriptProxy proxy = ClientScriptProxy.Current;

            // Specified nothing to do
            if (string.IsNullOrEmpty(scriptLocation))
                return;

            ScriptItem scriptItem = new ScriptItem();
            scriptItem.Src = scriptLocation;
            scriptItem.RenderMode = renderMode;

            if (scriptLocation == "WebResource")
            {
                scriptItem.Resource = resourceName;
                scriptItem.ResourceAssembly = null;
                scriptItem.Src = resourceName;
            }

            //// It's a relative url
            //if (ClientScriptProxy.LoadScriptsInHeader)
            //    proxy.RegisterClientScriptIncludeInHeader(control, control.GetType(),
            //                                            control.ResolveUrl(scriptLocation), topOfHeader);
            //else
            //    proxy.RegisterClientScriptInclude(control, control.GetType(),
            //                            Path.GetFileName(scriptLocation), control.ResolveUrl(scriptLocation));

            AddScript(scriptItem);
        }
        public void LoadControlScript(Control control, string scriptLocation, string resourceName)
        {
            LoadControlScript(control, scriptLocation, resourceName,ClientScriptProxy.DefaultScriptRenderMode);
        }
        public void LoadControlScript(Control control, string scriptLocation)
        {
            LoadControlScript(control, scriptLocation, null, ClientScriptProxy.DefaultScriptRenderMode);
        }

        /// <summary>
        /// Parses HtmlgenericControl attributes into a script object
        /// </summary>
        /// <param name="ctl"></param>
        /// <returns></returns>
        private ScriptItem ParseScriptProperties(HtmlGenericControl ctl)
        {
            ScriptItem script = new ScriptItem();

            script.Src = ctl.Attributes["Src"];

            string val = ctl.Attributes["RenderMode"] ?? "";
            switch (val.ToLower())
            {
                case "header":
                    script.RenderMode = ScriptRenderModes.Header;
                    break;
                case "headertop":
                    script.RenderMode = ScriptRenderModes.HeaderTop;
                    break;
                case "script":
                    script.RenderMode = ScriptRenderModes.Script;
                    break;
                case "inline":
                    script.RenderMode = ScriptRenderModes.Inline;
                    break;
                default:
                    script.RenderMode = ScriptRenderModes.Inherit;
                    break;
            }

            val = ctl.Attributes["AllowMinScript"] ?? "";
            if (val.ToLower() == "true")
                script.AllowMinScript = true;


            val = ctl.Attributes["Resource"] ?? "";
            if (!string.IsNullOrEmpty(val))
            {
                script.Resource = val;

                foreach (ScriptResourceAlias alias in ClientScriptProxy.ScriptResourceAliases)
                {
                    if (val == alias.Alias)
                    {
                        script.Resource = alias.Resource;
                        if (alias.ControlType != null)
                            script.ResourceControlType = alias.ControlType;
                    }
                }
            }

            if (string.IsNullOrEmpty(script.Src) && string.IsNullOrEmpty(script.Resource))
                throw new ArgumentException("ScriptContainer <script> tag requires either the 'src' or 'Resource' property set.");


            val = ctl.Attributes["ResourceControl"] ?? "";
            if (!string.IsNullOrEmpty(val))
                script.ResourceControl = val;

            val = ctl.Attributes["Assembly"] ?? "";
            if (!string.IsNullOrEmpty(val))
                script.ResourceAssembly = val;

            val = ctl.Attributes["Version"] ?? "";
            if (!string.IsNullOrEmpty(val))
                script.Version = val;



            return script;
        }
    }


    /// <summary>
    /// Individual Script Item used inside of a script container.
    /// This control maps syntax of the standard HTML script tag
    /// and ads a number of additional properties that are specific
    /// to script generation.
    /// 
    /// Note there's no Intellisense on this child item as it is
    /// rendered as an HtmlGenericControl and parsed into this
    /// object. Hence the properties below must be manually typed
    /// in and are not visible to Intellisense.
    /// </summary>
    //[ToolboxItem(false)]
    [ToolboxData("<Script src=\"\" type=\"text/javascript\" />")]
    [DefaultProperty("Src")]
    [Serializable]
    [DebuggerDisplay("Src ={Src},Resource={Resource}")]
    public class ScriptItem
    {
        /// <summary>
        /// The src location of the file. This path can include ~ pathing
        /// </summary>
        [Description("The src location of the file. This path can include ~ pathing")]
        [UrlProperty("*.js")]
        [Category("Script"), DefaultValue("")]
        public string Src
        {
            get { return _Src; }
            set { _Src = value; }
        }
        private string _Src = "";

        /// <summary>
        /// Determines whether script looks for optimized .min.js file in non-debug mode
        /// </summary>
        [Description("Determines whether script looks for optimized .min.js file in non-debug mode")]
        [Category("Script"), DefaultValue(false)]
        public bool AllowMinScript
        {
            get { return _AllowMinScript; }
            set { _AllowMinScript = value; }
        }
        private bool _AllowMinScript = false;

        /// <summary>
        /// Determines where in the page the script is rendered (HeaderTop,Header,Script,Inline,Inherit)
        /// <seealso>Class ScriptItem</seealso>
        /// <seealso>Embedding JavaScript Links with ScriptContainer</seealso>
        /// <seealso>Enumeration ScriptRenderModes</seealso>
        /// </summary>
        [Description("Determines where in the page the script is rendered (HeaderTop,Header,Script,Inline,Inherit)")]
        [Category("Script"), DefaultValue(typeof(ScriptRenderModes), "InheritFromControl")]
        public ScriptRenderModes RenderMode
        {
            get { return _ScriptRenderMode; }
            set { _ScriptRenderMode = value; }
        }
        private ScriptRenderModes _ScriptRenderMode = ScriptRenderModes.Inherit;


        /// <summary>
        /// If specified loads a 'known' resource by name from resources rather
        /// than the script source. This allows using a src url for getting
        /// debugging but using a WebResource for ensuring the latest version
        /// is always used at runtime.
        /// 
        /// Known resources are:
        /// wwscriptlibrary
        /// jquery
        /// calendar
        /// </summary>
        [Description("Specifies a known resource: ww.jquery, jquery, jqueryui, calendar")]
        [Category("Script"), DefaultValue("")]
        public string Resource
        {
            get { return _Resource; }
            set { _Resource = value; }
        }
        private string _Resource = "";

        /// <summary>
        /// A reference to a control in the assembly that holds the resource.
        /// This is used in lieu of a type so you can specify a control that
        /// is in the same assembly. Specify an ID in markup or a reference
        /// in code.
        /// </summary>
        public string ResourceControl
        {
            get { return _ResourceControl; }
            set { _ResourceControl = value; }
        }
        private string _ResourceControl = null;

        
        /// <summary>
        /// Instead of providing the Id of a control you can also pass the type of
        /// a control/object housed in the assembly. The type is used for getting the assembly
        /// to retrieve resources from.
        /// </summary>
        public Type ResourceControlType
        {
            get { return _ResourceControlType; }
            set { _ResourceControlType = value; }
        }
        private Type _ResourceControlType = null;


        
        /// <summary>
        /// The ID for this file when stored in the collection
        /// of script items. Based on the filename of the script
        /// (ie. somescript.js). Used to avoid duplication
        /// </summary>
        protected internal string FileId
        {
            get { return _FileId; }
            set { _FileId = value; }
        }
        private string _FileId = String.Empty;


        /// <summary>
        /// Resource assembly - maps to Assembly keyword
        /// </summary>
        public string ResourceAssembly
        {
            get { return _ResourceAssembly; }
            set { _ResourceAssembly = value; }
        }
        private string _ResourceAssembly = String.Empty;

        
        /// <summary>
        /// An optional version number that is appended to your
        /// resource url.
        /// </summary>
        public string Version
        {
            get { return _Version; }
            set { _Version = value; }
        }
        private string _Version = string.Empty;
    }


    /// <summary>
    /// Control designer used so we get a grey button display instead of the 
    /// default label display for the control.
    /// </summary>
    internal class ScriptContainerDesigner : ControlDesigner
    {

        public override string GetDesignTimeHtml()
        {
            return base.CreatePlaceHolderDesignTimeHtml();
        }
    }

}
