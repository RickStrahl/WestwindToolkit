using System;
using System.Linq;
using System.Web.UI;
using System.ComponentModel;
using Westwind.Utilities;
using System.Collections;
using System.Collections.Generic;
using System.Data;

namespace Westwind.Web.Controls
{
    /// <summary>
    /// The Pager class provides a standalone pager that can be added
    /// on any page and doesn't require a DataSource or related control. The
    /// control can be manually assigned values to display or automatically
    /// infer values using one of the FilterXXX methods to filter a data source.
    /// 
    /// &lt;&lt;img src="images/pager1.png" /&gt;&gt;
    /// <seealso>Using the Pager Web Control to Page Content and Data Results</seealso>
    /// </summary>
    public class Pager : Control
    {
        /// <summary>
        /// Total number of pages for this pager
        /// </summary>
        [Description("Total number of pages for this pager")]
        [Category("Pager"), DefaultValue(0)]
        public int TotalPages
        {
            get { return _TotalPages; }
            set { _TotalPages = value; }
        }
        private int _TotalPages = 0;

        /// <summary>
        /// The page to display. Values are 1 based.
        /// </summary>
        [Description("The page to display. Values are 1 based.")]
        [Category("Pager"), DefaultValue(1)]
        public int ActivePage
        {
            get { return _ActivePage; }
            set { _ActivePage = value; }
        }
        private int _ActivePage = 1;

        /// <summary>
        /// The number of items on the page
        /// </summary>
        [Description("The number of items on the page.")]
        [Category("Pager"), DefaultValue(10)]
        public int PageSize
        {
            get { return _PageSize; }
            set { _PageSize = value; }
        }
        private int _PageSize = 10;

        /// <summary>
        /// Total number of items available - must be set manually or via one of the filter methods
        /// </summary>
        [Browsable(false)]
        public int TotalItems
        {
            get { return _TotalItems; }
            set { _TotalItems = value; }
        }
        private int _TotalItems = 0;

        /// <summary>
        /// The base Url for each of the paging links.
        /// If left blank the control will use the current
        /// page Url and append.
        /// </summary>
        [Description("The base Url for the paging links. If empty automatically uses current page Url.")]
        [Category("Pager"), DefaultValue("")]
        public string BaseUrl
        {
            get { return _BaseUrl; }
            set { _BaseUrl = value; }
        }
        private string _BaseUrl = "";

        
        /// <summary>
        /// Query string key name for the Page variable
        /// </summary>
        [Description("Query string key name for the page to display")]
        [Category("Pager"), DefaultValue("page")]
        public string QueryStringPageField
        {
            get { return _QueryStringPageField; }
            set { _QueryStringPageField = value; }
        }
        private string _QueryStringPageField = "page";


        /// <summary>
        /// The CSS class used for the immediate pager control. By default this
        /// is rendered as a div tag. Default styling floats it right. 
        /// 
        /// Use RenderPagerContainerDiv and PagerContainerCssClass to specify an 
        /// 'outer' container if desired.
        /// </summary>
        [Description("CSS Class for the immediate pager. Note there's an additional Pager Container that can also be rendered. Renders on a <div> tag that by default is styled to float right.")]
        [Category("Pager Display"), DefaultValue("pager")]
        public string CssClass
        {
            get { return _CssClass; }
            set { _CssClass = value; }
        }
        private string _CssClass = "pager";


        /// <summary>
        /// CSS Class used for page links
        /// </summary>
        [Description("CSS Class used for page links")]
        [Category("Pager Display"), DefaultValue("pagerbutton")]
        public string PageLinkCssClass
        {
            get { return _PageLinkCssClass; }
            set { _PageLinkCssClass = value; }
        }
        private string _PageLinkCssClass = "pagerbutton";


        /// <summary>
        /// CSS class used for the selected page
        /// </summary>
        [Description("CSS class used for the selected page")]
        [Category("Pager Display"), DefaultValue("pagerbutton-selected")]
        public string SelectedPageCssClass
        {
            get { return _SelectedPageCssClass; }
            set { _SelectedPageCssClass = value; }
        }
        private string _SelectedPageCssClass = "pagerbutton-selected";


        /// <summary>
        /// Pages: text string
        /// </summary>
        [Localizable(true)]
        [Description("Pages: text")]
        [Category("Pager Text"), DefaultValue("Pages: ")]
        public string PagesText
        {
            get { return _PagesText; }
            set { _PagesText = value; }
        }
        private string _PagesText = "Pages: ";


        /// <summary>
        /// Pages: text
        /// </summary>
        [Description("Pages: text CSS class")]
        [Category("Pager Display"), DefaultValue("pagertext")]
        public string PagesTextCssClass
        {
            get { return _PagesTextCssClass; }
            set { _PagesTextCssClass = value; }
        }
        private string _PagesTextCssClass = "pagertext";

        /// <summary>
        /// The text displayed for the previous button. If this
        /// text is empty the button is not displayed.
        /// </summary>
        [Localizable(true)]
        [Description("The text displayed for the previous button. Empty = no button")]
        [Category("Pager Text"), DefaultValue("Prev")]
        public string PreviousText
        {
            get { return _PreviousText; }
            set { _PreviousText = value; }
        }
        private string _PreviousText = "Prev";

        /// <summary>
        /// The text displayed for the next button. If empty
        /// this button isn't displayed.
        /// </summary>
        [Localizable(true)]
        [Description("The text displayed for the next button. Empty = no button")]
        [Category("Pager Text"), DefaultValue("Next")]
        public string NextText
        {
            get { return _NextText; }
            set { _NextText = value; }
        }
        private string _NextText = "Next";

        /// <summary>
        /// 
        /// </summary>
        [Description("The maximum number of pages to display around hte active page")]
        [Category("Pager Navigation"), DefaultValue(10)]
        public int MaxPagesToDisplay
        {
            get { return _MaxPagesToDisplay; }
            set { _MaxPagesToDisplay = value; }
        }
        private int _MaxPagesToDisplay = 10;


        /// <summary>
        /// Determines whether the 1... and ...n page links are shown
        /// before and after the displayed pages
        /// 
        /// Only shown if there are more pages than MaxPagesToDisplay
        /// </summary>
        [Description("Determines whether First and Last Page links are rendered. Only shown if there are more Pages than MaxPagesToDisplay")]
        [Category("Pager Navigation"), DefaultValue(true)]
        public bool ShowFirstAndLastPageLinks
        {
            get { return _ShowFirstAndLastPageLinks; }
            set { _ShowFirstAndLastPageLinks = value; }
        }
        private bool _ShowFirstAndLastPageLinks = true;

        
        /// <summary>
        /// Determines whether the Previous and Next buttons are displayed
        /// </summary>
        [Description("Determines whether the Previous and Next buttons are displayed")]
        [Category("Pager Navigation"), DefaultValue(true)]
        public bool ShowPreviousNextLinks
        {
            get { return _ShowPreviousNextLinks; }
            set { _ShowPreviousNextLinks = value; }
        }
        private bool _ShowPreviousNextLinks = true;

        /// <summary>
        /// Determines whether a container div tag is generated. 
        /// Useful to allow nothing to be rendered if there are less than 2 pages
        /// as it hides the container. Alternately you can render the container
        /// through your markup but in that case you may end up with an empty container
        /// if there's no data or only a single page.
        /// </summary>
        [Description("Determines whether a container div tag is generated. Useful to allow nothing to be rendered if there are less than 2 pages.")]
        [Category("Pager Container"), DefaultValue(false)]
        public bool RenderContainerDiv
        {
            get { return _RenderContainerDiv; }
            set { _RenderContainerDiv = value; }
        }
        private bool _RenderContainerDiv = false;

        /// <summary>
        /// Determines whether a br clear='all' is rendered inside of the container div to break content.
        /// </summary>
        [Description("Determines whether a br clear='all' is rendered inside of the container div to break content.")]
        [Category("Pager Container"), DefaultValue(true)]
        public bool RenderContainerDivBreak
        {
            get { return _RenderContainerDivBreak; }
            set { _RenderContainerDivBreak = value; }
        }
        private bool _RenderContainerDivBreak = true;        
        
        /// <summary>
        /// The CSS Class used for the container div
        /// </summary>
        [Description("The CSS Class used for the container div if enabled")]
        [Category("Pager Container"), DefaultValue("pagercontainer")]
        public string ContainerDivCssClass
        {
            get { return _ContainerDivCssClass; }
            set { _ContainerDivCssClass = value; }
        }
        private string _ContainerDivCssClass = "pagercontainer";

        /// <summary>
        /// Internally used to hold the first page to render when max pages is exceeded
        /// </summary>
        private int _startPage = 1;

        /// <summary>
        /// Internally used to hold the last page to render when max pages are exceeded
        /// </summary>
        private int _endPage = 0;

/// <summary>
/// overridden to handle custom pager rendering for runtime and design time
/// </summary>
/// <param name="writer"></param>
protected override void Render(HtmlTextWriter writer)
{
    base.Render(writer);

    if (TotalPages == 0 && TotalItems > 0)                            
       TotalPages = CalculateTotalPagesFromTotalItems();  // calculate based on totalitems            

    if (DesignMode)
        TotalPages = 10;

    // don't render pager if there's only one page
    if (TotalPages < 2)
        return;

    if (RenderContainerDiv)
    {
        if (!string.IsNullOrEmpty(ContainerDivCssClass))
            writer.AddAttribute("class", ContainerDivCssClass);
        writer.RenderBeginTag("div");
    }

    // main pager wrapper
    writer.WriteBeginTag("div");
    writer.AddAttribute("id", this.ClientID);
    if (!string.IsNullOrEmpty(CssClass))
        writer.WriteAttribute("class", this.CssClass);
    
    //writer.Write(HtmlTextWriter.TagRightChar + "\r\n");


    // Pages Text
    writer.WriteBeginTag("span");
    if (!string.IsNullOrEmpty(PagesTextCssClass))
        writer.WriteAttribute("class", PagesTextCssClass);
    writer.Write(HtmlTextWriter.TagRightChar);
    writer.Write(this.PagesText);
    writer.WriteEndTag("span");

    // if the base url is empty use the current URL
    FixupBaseUrl();        

    // set _startPage and _endPage
    ConfigurePagesToRender();

    // write out first page link
    if (ShowFirstAndLastPageLinks && _startPage != 1)
    {
        writer.WriteBeginTag("a");
        string pageUrl = StringUtils.SetUrlEncodedKey(BaseUrl, QueryStringPageField, (1).ToString());
        writer.WriteAttribute("href", pageUrl);
        if (!string.IsNullOrEmpty(PageLinkCssClass))
            writer.WriteAttribute("class", PageLinkCssClass + " " + PageLinkCssClass + "-first");
        writer.Write(HtmlTextWriter.SelfClosingTagEnd);
        writer.Write("1");
        writer.WriteEndTag("a");        
    }

    // write out all the page links
    for (int i = _startPage; i < _endPage + 1; i++)
    {
        if (i == ActivePage)
        {
            writer.WriteBeginTag("span");
            if (!string.IsNullOrEmpty(SelectedPageCssClass))
                writer.WriteAttribute("class", SelectedPageCssClass);
            writer.Write(HtmlTextWriter.TagRightChar);
            writer.Write(i.ToString());
            writer.WriteEndTag("span");
        }
        else
        {
            writer.WriteBeginTag("a");
            string pageUrl = StringUtils.SetUrlEncodedKey(BaseUrl, QueryStringPageField, i.ToString()).TrimEnd('&');
            writer.WriteAttribute("href", pageUrl);
            if (!string.IsNullOrEmpty(PageLinkCssClass))
                writer.WriteAttribute("class", PageLinkCssClass);
            writer.Write(HtmlTextWriter.TagRightChar);
            writer.Write(i.ToString());
            writer.WriteEndTag("a");
        }

        writer.Write("\r\n");
    }

    // write out last page link
    if (ShowFirstAndLastPageLinks && _endPage < TotalPages)
    {
        writer.WriteBeginTag("a");
        string pageUrl = StringUtils.SetUrlEncodedKey(BaseUrl, QueryStringPageField, TotalPages.ToString());
        writer.WriteAttribute("href", pageUrl);
        if (!string.IsNullOrEmpty(PageLinkCssClass))
            writer.WriteAttribute("class", PageLinkCssClass + " " + PageLinkCssClass + "-last");
        writer.Write(HtmlTextWriter.TagRightChar);
        writer.Write(TotalPages.ToString());
        writer.WriteEndTag("a");
        writer.Write("\r\n");
    }


    // Previous link
    if (ShowPreviousNextLinks && !string.IsNullOrEmpty(PreviousText) && ActivePage > 1)
    {        
        writer.WriteBeginTag("a");
        string pageUrl = StringUtils.SetUrlEncodedKey(BaseUrl, QueryStringPageField, (ActivePage - 1).ToString());
        writer.WriteAttribute("href", pageUrl);
        if (!string.IsNullOrEmpty(PageLinkCssClass))
            writer.WriteAttribute("class", PageLinkCssClass + " " + PageLinkCssClass + "-prev");
        writer.Write(HtmlTextWriter.TagRightChar);
        writer.Write(PreviousText);
        writer.WriteEndTag("a");
        writer.Write("\r\n");
    }

    // Next link
    if (ShowPreviousNextLinks && !string.IsNullOrEmpty(NextText) && ActivePage < TotalPages)
    {     
        writer.WriteBeginTag("a");
        string pageUrl = StringUtils.SetUrlEncodedKey(BaseUrl, QueryStringPageField, (ActivePage + 1).ToString());
        writer.WriteAttribute("href", pageUrl);
        if (!string.IsNullOrEmpty(PageLinkCssClass))
            writer.WriteAttribute("class", PageLinkCssClass + " " + PageLinkCssClass + "-next");
        writer.Write(HtmlTextWriter.TagRightChar);
        writer.Write(NextText);
        writer.WriteEndTag("a");
        writer.Write("\r\n");
    }

    writer.WriteEndTag("div");

    if (RenderContainerDiv)
    {
        if (RenderContainerDivBreak)
            writer.Write("<div style=\"clear:both\"></div>\r\n");

        writer.WriteEndTag("div");
        writer.Write("\r\n");
    }
}

        /// <summary>
        /// Determines the startpage and endpage which are the first
        /// and last page numbers that are rendered.
        /// </summary>
        private void ConfigurePagesToRender()
        {
            // figure out which page counts should be displayed (10 typically)
            _startPage = 1;
            _endPage = TotalPages;

            if (_endPage - _startPage > MaxPagesToDisplay)
            {
                int halfOfMaxPages = (MaxPagesToDisplay / 2);

                if (ActivePage > 2)
                {
                    _startPage = ActivePage - halfOfMaxPages;
                    if (_startPage < 1)
                        _startPage = 1;
                }

                _endPage = ActivePage + halfOfMaxPages;

                // add pages not used in beginning to end
                if (_startPage == 1)
                    _endPage += halfOfMaxPages - ActivePage;

                if (_endPage > TotalPages)
                    _endPage = TotalPages;

                if (_endPage == TotalPages)
                    _startPage = _endPage - MaxPagesToDisplay;

                if (_startPage < 1)
                    _startPage = 1;
            }
        }

        /// <summary>
        /// Tries to retrieve the Page url if one wasn't provided
        /// </summary>
        private void FixupBaseUrl()
        {
            // Fix up the Url for page links            
            if (string.IsNullOrEmpty(BaseUrl) && !DesignMode)
            {
                BaseUrl = Page.Request.Url.ToString();
                if (!BaseUrl.Contains("?"))
                    BaseUrl += "?";
                else
                    BaseUrl += "&";
            }
        }

        /// <summary>
        /// Updates the internal settings based on the url
        /// </summary>
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            int page = ActivePage;            
            if (!DesignMode)
            {
                int.TryParse(Page.Request.Params[QueryStringPageField] ?? "1", out page);
                ActivePage = page;                
            }
        }


        /// <summary>
        /// <summary>
        /// Queries the database for the ActivePage applied manually
        /// or from the Request["page"] variable. This routine
        /// figures out and sets TotalPages, ActivePage and
        /// returns a filtered subset IQueryable that contains
        /// only the items from the ActivePage.
        /// </summary>
        /// <param name="query"></param>
        /// <param name="activePage">
        /// The page you want to display. Sets the ActivePage property when passed. 
        /// Pass 0 or smaller to use ActivePage setting.
        /// </param>
        /// <returns></returns>
        public IQueryable<T> FilterIQueryable<T>(IQueryable<T> query, int activePage)
              where T : class, new()
        {
            ActivePage = activePage < 1 ? ActivePage : activePage;
            if (ActivePage < 1)
                ActivePage = 1;

            TotalItems = query.Count(); 

            if (TotalItems <= PageSize)
            {
                ActivePage = 1;
                TotalPages = 1;
                return query;
            }

            int skip = ActivePage - 1;
            if (skip > 0)
                query = query.Skip(skip * PageSize);

            _TotalPages = CalculateTotalPagesFromTotalItems();

            return query.Take(PageSize);
        }


        /// <summary>
        /// Queries the database for the ActivePage applied manually
        /// or from the Request["page"] variable. This routine
        /// figures out and sets TotalPages, ActivePage and
        /// returns a filtered subset IQueryable that contains
        /// only the items from the ActivePage.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <returns></returns>
        public IQueryable<T> FilterIQueryable<T>(IQueryable<T> query)
              where T : class, new()
        {
            return FilterIQueryable(query, 0);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="items"></param>
        /// <param name="activePage"></param>
        /// <returns></returns>
        public IEnumerable<T> FilterIEnumerable<T>(IEnumerable<T> items, int activePage)
            where T: class, new()
        {
            return FilterIQueryable<T>(items.AsQueryable<T>(), activePage);
        }


        /// <summary>
        /// Filters a data table for an ActivePage.
        /// 
        /// Note: Modifies the data set permanently by remove DataRows
        /// </summary>
        /// <param name="dt">Full result DataTable</param>
        /// <param name="activePage">Page to display. 0 to use ActivePage property </param>
        /// <returns></returns>
        public DataTable FilterDataTable(DataTable dt, int activePage)
        {
            ActivePage = activePage < 1 ? ActivePage : activePage;
            if (ActivePage < 1)
                ActivePage = 1;

            TotalItems = dt.Rows.Count;

            if (TotalItems <= PageSize)
            {
                ActivePage = 1;
                TotalPages = 1;
                return dt;
            }
            
            int skip = ActivePage - 1;            
            if (skip > 0)
            {
                for (int i = 0; i < skip * PageSize; i++ )
                    dt.Rows.RemoveAt(0);
            }
            while(dt.Rows.Count > PageSize)
                    dt.Rows.RemoveAt(PageSize);

            return dt;
        }
        

        /// <summary>
        /// Calculates total pages from TotalItems
        /// </summary>
        /// <returns></returns>
        private int CalculateTotalPagesFromTotalItems()
        {
            int totalPages = Convert.ToInt32(Math.Floor(Convert.ToDecimal(TotalItems) / Convert.ToDecimal(PageSize)));
            int rest = TotalItems % PageSize;
            if (rest != 0)
                totalPages++;

            return totalPages;
        }


    }
}
