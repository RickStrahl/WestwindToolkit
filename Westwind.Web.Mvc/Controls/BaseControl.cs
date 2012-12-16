using System.Web;
using System.Web.Mvc;
using System.Text;
using System.Collections.Generic;
using System.Web.Routing;
using System.Collections;

namespace Westwind.Web.Mvc
{
    public class BaseControl
    {
        /// <summary>
        /// The ID rendered into the page
        /// </summary>
        public string Id { get; set; }
        
        
        /// <summary>
        /// 
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Text for the control if applicable. This property may 
        /// not apply to all types of controls
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// A value for the control for rendering. May not apply for all controls
        /// </summary>
        public string Value { get; set; }


        /// <summary>
        /// The CSS class rendered
        /// </summary>
        public string CssClass { get; set; }


        /// <summary>
        /// Any Html Attributes for the main element of this control specified as key value strings
        /// </summary>
        public RouteValueDictionary Attributes
        {
            get { return _Attributes; }
            set { _Attributes = value; }
        }
        private RouteValueDictionary _Attributes = new RouteValueDictionary();

        /// <summary>
        /// A set of styles specified as key value strings
        /// </summary>
        public RouteValueDictionary Styles
        {
            get { return _Styles; }
            set { _Styles = value; }
        }
        private RouteValueDictionary _Styles = new RouteValueDictionary();


        /// <summary>
        /// A style string you want to apply. Augmented by the Styles dictionary property
        /// </summary>
        public string Style {get; set; }


        /// <summary>
        /// Internal output writer
        /// </summary>
        protected StringBuilder Output
        {
            get { return _Output; }
            set { _Output = value; }
        }
        private StringBuilder _Output = new StringBuilder();


        public HtmlHelper HtmlHelper
        {
            get { return _HtmlHelper; }
            set { _HtmlHelper = value; }
        }
        private HtmlHelper _HtmlHelper = null;


        public UrlHelper UrlHelper
        {
            get { return _UrlHelper; }
            set { _UrlHelper = value; }
        }
        private UrlHelper _UrlHelper = null;

        
        /// <summary>
        /// Page level helper routines and state features for
        /// use internally in the control's code
        /// </summary>
        public PageEnvironment Page
        {
            get { return _Page; }
        }
        private PageEnvironment _Page = PageEnvironment.Current;


        /// <summary>
        /// renders standard attributes. Override for any custom attributes
        /// you want to add to the control. When overriding the base class
        /// method call the base method to include the stock rendering.
        /// 
        /// Called by RenderAttributes.
        /// </summary>
        public virtual void ApplyAttributes()
        {
            if (!string.IsNullOrEmpty(this.Id))
                Attributes.Add("id", this.Id.Replace(".","_"));

            if (!string.IsNullOrEmpty(this.CssClass))
                Attributes.Add("class", this.CssClass);

            StringBuilder sb = new StringBuilder();
            foreach (var style in this.Styles)
            {
                sb.Append(style.Key + ":" + style.Value + ";");
            }
            if (sb.Length > 0)
                Attributes.Add("style", sb.ToString());
        }

        /// <summary>
        /// Renders the attributes into the output stream
        /// </summary>
        protected virtual void RenderAttributes()
        {
            this.ApplyAttributes();

            foreach (var attr in this.Attributes)
            {
                Output.Append(attr.Key + "=\"" + attr.Value + "\" ");
            }
        }


        protected virtual void PreRender()
        {
        }

        protected virtual void PostRender()
        {
        }

        /// <summary>
        /// Returns rendered HTML output
        /// </summary>
        /// <returns></returns>
        public virtual string Render()
        {
            PreRender();

            PostRender();

            return Output.ToString();
        }


        /// <summary>
        /// Determines whether the control has already been rendered. Use to control
        /// whether scripts or other shared page components get rendered more than
        /// once.
        /// </summary>
        protected bool IsFirstRenderOnPage
        {
            get
            {
                string rendered = HttpContext.Current.Items[this] as string;
                if (string.IsNullOrEmpty(rendered))
                    return true;
                return false;
            }
            set
            {
                if (!value)
                    HttpContext.Current.Items[this] = "1";
                else
                    HttpContext.Current.Items[this] = null;
            }
        }

        /// <summary>
        /// Merges attributes from an object map 
        /// </summary>
        /// <param name="attributes"></param>
        public void MergeAttributes(object htmlAttributes)
        {
            var attrs = new RouteValueDictionary(htmlAttributes);
            foreach (var attr in attrs)
            {
                this.Attributes[attr.Key] =  attr.Value;
            }
        }

    }
}
