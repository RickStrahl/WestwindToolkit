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
using System.IO;
using System.Text;
using System.Drawing;
using System.Web.UI;
using System.ComponentModel;
using System.Collections;
using System.Collections.Specialized;
using System.Web.UI.WebControls;
using System.Collections.Generic;
using Westwind.Utilities;


namespace Westwind.Web.Controls
{

    #region TabControl

    /// <summary>
    /// The TabControl provides a simple client control tab interface  to display 
    /// multiple pages with a tabbed interface at the top. Content pages are simply
    ///  created as plain &lt;div&gt; tags with an ID that is referenced via the 
    /// TabPageClientId property. A default tab can be chosen which is displayed 
    /// initially and the control keeps track of the tabpage active across 
    /// apostbacks.
    /// 
    /// &lt;&lt;img src="images/TabControl.png"&gt;&gt;
    /// 
    /// The tab control only fires on the client and there are no server side 
    /// events fired. You can assign the SelectedTab property but otherwise the 
    /// server side has no additional control. Client side code can activate the 
    /// tab with a JavaScript ActivateTab(tabId, num) which specifies the ClientID 
    /// of the tab control and the tab by number or client ID of the Div.
    /// </summary>
    /// <example>
    /// &lt;&lt;code lang=&quot;HTML&quot;&gt;&gt;
    /// &lt;div class=&quot;containercontent&quot;&gt;
    /// 
    ///     &lt;ww:TabControl runat=&quot;server&quot; ID=&quot;TabControls&quot; 
    /// TabHeight=&quot;25&quot; TabWidth=&quot;120&quot;
    ///         TabstripSeparatorHeight=&quot;&quot; &gt;
    ///         &lt;TabPages&gt;
    ///             &lt;ww:TabPage runat=&quot;server&quot; ID=&quot;Page1&quot; 
    /// TabPageClientId=&quot;Page1&quot; Caption=&quot;Page 1&quot;
    ///                 
    /// Style=&quot;height:25px;width:120px;height:25px;width:120px;&quot; /&gt;
    ///             &lt;ww:TabPage runat=&quot;server&quot; ID=&quot;Page2&quot; 
    /// TabPageClientId=&quot;Page2&quot; Caption=&quot;Page 2&quot;
    ///                 
    /// Style=&quot;height:25px;width:120px;height:25px;width:120px;&quot; /&gt;
    ///             &lt;ww:TabPage runat=&quot;server&quot; ID=&quot;Page3&quot; 
    /// TabPageClientId=&quot;Page3&quot; Caption=&quot;Page 3&quot;
    ///                 
    /// Style=&quot;height:25px;width:120px;height:25px;width:120px;&quot;/&gt;
    /// 
    ///         &lt;/TabPages&gt;
    ///     &lt;/ww:TabControl&gt;
    /// 
    ///     &lt;div id=&quot;Page1&quot; class=&quot;tabpage&quot;&gt;
    ///         Page 1
    ///     &lt;/div&gt;
    ///     &lt;div id=&quot;Page2&quot; class=&quot;tabpage&quot;&gt;
    ///         Page 2
    ///     &lt;/div&gt;
    ///     &lt;div id=&quot;Page3&quot; class=&quot;tabpage&quot;&gt;
    ///         Page 3
    ///     &lt;/div&gt;
    /// &lt;/div&gt;
    /// &lt;&lt;/code&gt;&gt;
    /// </example>
    [ToolboxData("<{0}:TabControl runat=server></{0}:TabControl>")]
    [ToolboxBitmap(typeof(System.Web.UI.WebControls.Image))]
    [ParseChildren(true, "TabPages")]
    [PersistChildren(false)]
    public class TabControl : Control, IPostBackDataHandler, INamingContainer
    {
        const string HIDDEN_FORMVAR_PREFIX = "__TABSELECTION_";

        public TabControl()
        {
            _Tabs = new List<TabPage>();
        }


        /// <summary>
        /// Collection of Tabpages.
        /// </summary>
        //[Bindable(true)]
        //[NotifyParentProperty(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]  // .Visible Content generates code for each page
        [PersistenceMode(PersistenceMode.InnerProperty)]
        public List<TabPage> TabPages
        {
            // IMPORTANT: No Setter or TabPages doesn't show in the designer
            get { return _Tabs; }
        }
        private List<TabPage> _Tabs = new List<TabPage>();


        /// <summary>
        /// The completed control output
        /// </summary>
        private string Output = "";

        /// <summary>
        /// The output for the tabs generated by RenderTabs
        /// </summary>
        private string TabOutput = "";

        /// <summary>
        /// The output of the Script block required to handle tab activation
        /// </summary>
        private string Script = "";

        protected System.Web.UI.WebControls.Literal txtActivationScript;
        protected System.Web.UI.WebControls.Literal txtTabPlaceHolder;

        //private bool DesignMode = (HttpContext.Current == null);

        /// <summary>
        /// The Selected Tab. Set this to the TabPageClientId of the tab that you want to have selected
        /// </summary>
        [Browsable(true), Description("The TabPageClientId of the selected tab. This TabPageClientId must map to TabPageClientId assigned to a tab. Should also match an ID tag in the doc that is shown or hidden when tab is activated.")]
        [Category("Tabs")]
        public string SelectedTab
        {
            get { return _SelectedTab; }
            set { _SelectedTab = value; }
        }
        string _SelectedTab = "";

        /// <summary>
        /// The width for each of the tabs. Each tab will be this width.
        /// </summary>
        [Browsable(true), Description("The width of all the individual tabs in pixels")]
        [Category("Tab Styling")]
        public Unit TabWidth
        {
            get { return _TabWidth; }
            set { _TabWidth = value; }
        }
        Unit _TabWidth = Unit.Empty;

        /// <summary>
        /// The height of each of the tabs.
        /// </summary>
        [Browsable(true), Description("The Height of all the individual tabs in pixels")]
        [Category("Tab Styling")]
        public Unit TabHeight
        {
            get { return _TabHeight; }
            set { _TabHeight = value; }
        }
        public Unit _TabHeight = Unit.Empty;


        /// <summary>
        /// The CSS class that is used to render nonselected tabs.
        /// </summary>
        [Browsable(true), Description("The CSS style used for non selected tabs"), DefaultValue("tabbutton")]
        [Category("Tab Styling")]
        public string TabCssClass
        {
            get { return _TabCssClass; }
            set { _TabCssClass = value; }
        }
        string _TabCssClass = "tabbutton";

        /// <summary>
        /// The CSS class that is used to render a selected button. Defaults to selectedtabbutton.
        /// </summary>
        [Browsable(true), Description("The CSS style used for the selected tab"), DefaultValue("tabbutton-selected")]
        [Category("Tab Styling")]
        public string SelectedTabCssClass
        {
            get { return _SelectedTabCssClass; }
            set { _SelectedTabCssClass = value; }
        }
        string _SelectedTabCssClass = "tabbutton-selected";

        [Browsable(true), Description("The CSS style used for the disabled tab"), DefaultValue("tabbutton-disabled")]
        [Category("Tab Styling")]
        public string DisabledTabCssClass
        {
            get { return _DisabledTabCssClass; }
            set { _DisabledTabCssClass = value; }
        }
        private string _DisabledTabCssClass = "tabbutton-disabled";

        
        /// <summary>
        /// The class used for the separator strip between tab and content
        /// </summary>
        [Description("The class used for the separator strip between tab and content")]
        [Category("Tab Styling"), DefaultValue("tabstripseparator")]
        public string TabStripSeparatorCssClass
        {
            get { return _TabStripSeparatorCssClass; }
            set { _TabStripSeparatorCssClass = value; }
        }
        private string _TabStripSeparatorCssClass = "tabstripseparator";


        [Browsable(true), Description("Optional separator height that separates the tabs from content.")]
        [Category("Tab Styling")]
        public Unit TabstripSeparatorHeight
        {
            get { return _TabstripSeparatorHeight; }
            set { _TabstripSeparatorHeight = value; }
        }
        Unit _TabstripSeparatorHeight = Unit.Empty;


        [Browsable(true), Description("The number of tabs that are rendered at maximum in row before rolling into the next row."), DefaultValue(8)]
        [Category("Tabs")]
        public int TabsPerRow
        {
            get { return _TabsPerRow; }
            set { _TabsPerRow = value; }
        }
        private int _TabsPerRow = 8;


        /// <summary>
        /// Makes MS Ajax aware
        /// </summary>
        private ClientScriptProxy ClientScriptProxy = null;

        private int TabCounter = 0;

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            ClientScriptProxy = ClientScriptProxy.Current;
        }

        protected override void OnPreRender(EventArgs e)
        {
            Page.RegisterRequiresPostBack(this);

            if (string.IsNullOrEmpty(SelectedTab) && _Tabs.Count > 0)
                SelectedTab = _Tabs[0].TabPageClientId;

            // Persist our selection into a hidden var since it's all client side 
            // Script updates this var
            ClientScriptProxy.RegisterHiddenField(this, HIDDEN_FORMVAR_PREFIX + ClientID, SelectedTab);

            string script =
@"
function ActivateTab(tabId, num)
{
    var _Tabs = eval(tabId);
    if (_Tabs.onActivate && _Tabs.onActivate(num))
        return; 
   
    if (typeof(num)=='string') {
        for(var x=0; x< _Tabs.length; x++) {
            if (_Tabs[x].pageId == num)
            { num = x; break;}
        }
    }
    if (typeof(num)=='string') num=0;
    var Tab = _Tabs[num];
    for(var i=0; i<_Tabs.length; i++) {
        var t = _Tabs[i];
        document.getElementById(t.tabId).className = (t.enabled ? '" + TabCssClass + "' : '" + DisabledTabCssClass + @"');
        if (t.pageId)
            document.getElementById(t.pageId).style.display = 'none';
    }
    document.getElementById(Tab.tabId).className = '" + SelectedTabCssClass + @"';
    document.getElementById(Tab.pageId).style.display = '';
    document.getElementById('" + HIDDEN_FORMVAR_PREFIX + ClientID + @"').value=Tab.pageId;
}
";

            // Register only once per page
            ClientScriptProxy.RegisterClientScriptBlock(this, typeof(WebResources), "ActivateTab", script, true);


            StringBuilder sb = new StringBuilder(2048);
            sb.AppendFormat("var {0} = [];\r\n", ClientID);
            for (int i = 0; i < TabPages.Count; i++)
            {
                string iStr = i.ToString();
                TabPage tab = TabPages[i];
                sb.Append(ClientID + "[" + iStr + "] = { id: " + iStr + "," +
                          "tabId: '" + ClientID + "_" + iStr + "'," +
                          "pageId: '" + tab.TabPageClientId + "'," +
                          "enabled: " + tab.Enabled.ToString().ToLower() + "};\r\n");
            }

            ClientScriptProxy.RegisterClientScriptBlock(this, typeof(WebResources), "TabInit_" + ClientID, sb.ToString(), true);


            // Force the page to show the Selected Tab
            if (SelectedTab != "")
            {
                script = "ActivateTab('" + ClientID + "','" + SelectedTab + "');\r\n";
                ClientScriptProxy.RegisterStartupScript(this, typeof(WebResources), "TabStartup_" + ClientID, script, true);
            }

            base.OnPreRender(e);
        }

        protected override void Render(HtmlTextWriter writer)
        {
            bool noTabs = false;
            string selected = null;

            // If no tabs have been defined in design mode write a canned HTML display 
            if (DesignMode && TabPages.Count == 0)
            {
                noTabs = true;
                AddTab("No Tabs", "default", "Tab1");
                AddTab("No Tabs 2", "default", "Tab2");
                selected = SelectedTab;
                SelectedTab = "Tab2";
            }

            // Render the actual control
            RenderControl();

            // Dump the output into the ASP out stream
            writer.Write(Output);

            // Call the base to let it output the writer's output
            base.Render(writer);

            if (noTabs)
            {
                TabPages.Clear();
                SelectedTab = selected;
            }
        }

        /// <summary>
        /// High level routine that renders the actual control
        /// </summary>
        private void RenderControl()
        {
            // Generate the HTML for the tabs and script blocks
            RenderTabs();

            // Generate the HTML for the base page and embed
            // the script etc into the page
            Output = string.Format(TabControl.MasterTemplate,
                                        TabOutput, Script, SelectedTabCssClass,
                                        TabstripSeparatorHeight.ToString());
        }


        /// <summary>
        /// Creates various string properties that are merged into the output template.
        /// Creates the tabs and the associated script code.
        /// </summary>
        private void RenderTabs()
        {
            if (_Tabs != null)
            {
                // ActivateTab script code
                StringBuilder Script = new StringBuilder();

                // ShowTabPage script code
                StringBuilder Script2 = new StringBuilder();

                // HtmlTextWriter to handle output generation for the HTML
                StringBuilder sb = new StringBuilder();
                StringWriter sw = new StringWriter(sb);
                HtmlTextWriter tabWriter = new HtmlTextWriter(sw);

                tabWriter.WriteLine("<table border='0' cellspacing='0'><tr>");

                
                
                int count = -1;
                foreach (TabPage tab in _Tabs)
                {
                    bool isPageSelected = false;
                    isPageSelected = _Tabs.Count % 1 == 0; 

                    if (!string.IsNullOrEmpty(tab.TabPageClientId) && 
                        tab.TabPageClientId == _SelectedTab)
                        isPageSelected = true;

                    count++;
                    string id = ClientID + "_" + count.ToString();

                    tabWriter.WriteBeginTag("td");

                    if (!string.IsNullOrEmpty(tab.Style) && !tab.Style.EndsWith(";"))
                        StringUtils.TerminateString(tab.Style, ";");

                    if (!TabHeight.IsEmpty)
                        tab.Style += "height:" + TabHeight.ToString() + ";";
                    if (!TabWidth.IsEmpty)
                        tab.Style += "width:" + TabWidth.ToString() + ";";

                    tabWriter.WriteAttribute("id", id);

                    string ActionLink = FixupActionLink(tab);

                    if (ActionLink != "" && tab.Enabled)
                        tabWriter.WriteAttribute("onclick", ActionLink);

                    if (_Tabs == null)
                        return;

                    if (tab.TabPageClientId != "" && tab.TabPageClientId == _SelectedTab)
                        tabWriter.WriteAttribute("class", SelectedTabCssClass);
                    else
                        tabWriter.WriteAttribute("class", TabCssClass);

                    if (tab.Style != "")
                        tabWriter.WriteAttribute("style", tab.Style);

                    tabWriter.Write(HtmlTextWriter.TagRightChar);

                    if (tab.TabImage != "")
                        tabWriter.Write("<img src='" + ResolveUrl(tab.TabImage) + "' style='margin: 0px 5px 0px 7px;' align='left' />\r\n");

                    tabWriter.Write(tab.Caption);

                    tabWriter.WriteEndTag("td");
                    tabWriter.Write("\r\n");

                }
                tabWriter.Write("</tr>");
                    
                if (TabstripSeparatorHeight != Unit.Empty && TabstripSeparatorHeight.Value > 0.00)
                {
                    tabWriter.Write(
    @"<tr>
    <td class='" + TabStripSeparatorCssClass + @"' colspan='" + TabPages.Count.ToString()  +  "' style='padding: 0px;height: " + TabstripSeparatorHeight.ToString() + @";'></td>
</tr>");
                }

                    
                tabWriter.Write("</table>\r\n");



                TabOutput = sb.ToString();
                tabWriter.Close();
            }
        }

        /// <summary>
        /// Adds a new item to the Tab collection.
        /// </summary>
        /// <param name="Caption">The caption of the tab</param>
        /// <param name="Link">The HTTP or JavaScript link that is fired when the tab is activated. Can optionally be Default which activates the tab and activates the page ID.</param>
        /// <param name="TabPageClientId">The ID for this tab - map this to an ID tag in the HTML form.</param>
        public void AddTab(string Caption, string Link, string TabPageClientId)
        {
            TabPage Tab = new TabPage();
            Tab.Caption = Caption;
            Tab.ActionLink = Link;
            Tab.TabPageClientId = TabPageClientId;
            Tab.ID = ID + "_" + (TabCounter++).ToString();
            AddTab(Tab);
            Page.Response.Write("Add Tab");
        }

        /// <summary>
        /// Adds a new item to the Tab collection.
        /// </summary>
        /// <param name="Caption">The caption of the tab</param>
        /// <param name="Link">The HTTP or JavaScript link that is fired when the tab is activated. Can optionally be Default which activates the tab and activates the page ID.</param>
        public void AddTab(string Caption, string Link)
        {
            AddTab(Caption, Link, "");
        }
        public void AddTab(TabPage Tab)
        {
            Tab.ID = ID + "_" + (TabCounter++).ToString();
            _Tabs.Add(Tab);
        }


        /// <summary>
        /// Fixes up the ActionLink property to final script code
        /// suitable for an onclick handler
        /// </summary>
        /// <returns></returns>
        protected string FixupActionLink(TabPage Tab)
        {
            string Action = Tab.ActionLink;

            if (Action.ToLower() == "default" || string.IsNullOrEmpty(Action))
                Action = "ActivateTab('" + ClientID + "','" + Tab.TabPageClientId + "');";

                // If we don't have 'javascript:' in the text it's a Url: must use document to go there
            else if (Action != "" && Action.IndexOf("script:") < 0)
                Action = "document.location='" + Action + "';";

            return Action;
        }


        //protected override void LoadControlState(object savedState)
        //{
        //    string ControlState = savedState as string;
        //    if (ControlState == null)
        //        return;

        //    SelectedTab = ControlState;
        //}
        //protected override object SaveControlState()
        //{
        //    return SelectedTab;
        //}


        /// <summary>
        /// Required to be able to properly deal with the Collection object
        /// </summary>
        /// <param name="obj"></param>
        protected override void AddParsedSubObject(object obj)
        {
            if (obj is TabPage)
            {
                TabPages.Add((TabPage)obj);
                return;
            }
        }



        /// <summary>
        /// The master HTML template into which the dynamically generated tab display is rendered.
        /// </summary>
        private static string MasterTemplate =
@"
{1}
{0}
";


        #region IPostBackDataHandler Members

        public bool LoadPostData(string postDataKey, NameValueCollection postCollection)
        {
            SelectedTab = postCollection[HIDDEN_FORMVAR_PREFIX + ClientID];
            if (string.IsNullOrEmpty(SelectedTab))
                SelectedTab = TabPages[0].TabPageClientId;

            return false;
        }

        public void RaisePostDataChangedEvent()
        {
        }

        #endregion
    }

    #endregion

    #region TabPage Class

    /// <summary>
    /// The individual TabPage class that holds the intermediary Tab page values
    /// </summary>
    [ToolboxData("<{0}:TabPage runat=server></{0}:TabPage>")]
    [ToolboxItem(false)]
    public class TabPage : Control
    {
        [NotifyParentProperty(true)]
        [Browsable(true), Description("The display caption for the Tab.")]
        [Localizable(true)]
        [DefaultValue("")]
        public string Caption
        {
            get { return cCaption; }
            set { cCaption = value; }
        }
        string cCaption = "";

        [NotifyParentProperty(true)]
        [Browsable(true), Description("The TabPageClientId for this item. If you create a TabPageClientId you must create a matching ID tag in your HTML that is to be enabled and disabled automatically.")]
        [DefaultValue("")]
        public string TabPageClientId
        {
            get { return cTabPageClientId; }
            set { cTabPageClientId = value; }
        }
        string cTabPageClientId = "";

        [NotifyParentProperty(true)]
        [Browsable(true), Description("The Url or javascript: code that fires. You can use javascript:ActivateTab(this);ShowPage('TabPageClientIdValue'); to force a tab and page to display.")]
        [DefaultValue("")]
        public string ActionLink
        {
            get { return Href; }
            set { Href = value; }
        }
        string Href = "";

        /// <summary>
        /// Image placed on the left of the Tab
        /// </summary>
        [NotifyParentProperty(true)]
        [Browsable(true), Description("Imaged placed on the left of the tab. Should be small enough to fit on Tab.")]
        [DefaultValue("")]
        public string TabImage
        {
            get { return cTabImage; }
            set { cTabImage = value; }
        }
        string cTabImage = "";

        /// <summary>
        /// Image placed on the left of the Tab
        /// </summary>
        [NotifyParentProperty(true)]
        [Browsable(true), Description("Style applied to a particular Tab heading. This overrides the control level style sheet tags TabStyle and SelectedTabStyle")]
        [DefaultValue("")]
        public string Style
        {
            get { return cStyle; }
            set { cStyle = value; }
        }
        string cStyle = "";

        [NotifyParentProperty(true)]
        [Browsable(true), Description("Determines whether the tab is selectable. If false shows lowlighted.")]
        [DefaultValue(true)]
        public bool Enabled
        {
            get { return _Enabled; }
            set { _Enabled = value; }
        }
        private bool _Enabled = true;



        //		[NotifyParentProperty(true)]
        //		[Browsable(true),Description("Determines whether this tab shows as selected.")]
        //		public bool Selected 
        //		{
        //			get { return bSelected; }
        //			set { 
        //					bSelected = value;
        //				if (Parent != null)  
        //				{
        //					((TabControl) Parent).SelectedTab = TabPageClientId;
        //				}
        //				}
        //			
        //		}
        //		bool bSelected = false;
    }

    #endregion

    #region TabPageCollection Class

    public class TabCollection : CollectionBase
    {
        protected Control Parent = null;

        public TabCollection(Control parent)
        {
            Parent = parent;
        }

        /// <summary>
        /// Indexer property for the collection that returns and sets an item
        /// </summary>
        public TabPage this[int index]
        {
            get
            {
                return (TabPage)List[index];
            }
            set
            {
                List[index] = value;
            }
        }

        /// <summary>
        /// Adds a new error to the collection
        /// </summary>
        public void Add(TabPage Tab)
        {
            List.Add(Tab);
            Parent.Controls.Add(Tab);
        }

        public void Insert(int index, TabPage item)
        {
            List.Insert(index, item);
        }

        public void Remove(TabPage Tab)
        {
            List.Remove(Tab);
        }

        public bool Contains(TabPage Tab)
        {
            return List.Contains(Tab);
        }

        //Collection IndexOf method 
        public int IndexOf(TabPage item)
        {
            return List.IndexOf(item);
        }

        public void CopyTo(TabPage[] array, int index)
        {
            List.CopyTo(array, index);
        }

    }
    #endregion

}
