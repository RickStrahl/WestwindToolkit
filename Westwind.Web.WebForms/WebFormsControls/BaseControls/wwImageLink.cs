using System;
using System.Data;
using System.Drawing;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.ComponentModel;
using System.Reflection;

namespace Westwind.Web.Controls
{

    /// <summary>
    /// Image Control that can act as a link.
    /// </summary>
    [ToolboxBitmap(typeof(System.Web.UI.WebControls.LinkButton)),
    DefaultProperty("ImageUrl"),
    ToolboxData("<{0}:wwWebImageLink runat='server' size='20'></{0}:wwWebImageLink>")]
    public class wwImageLink : System.Web.UI.WebControls.Image
    {
        /// <summary>
        /// The Url to navigate to when this image is clicked.
        /// </summary>
        [Description("The Url to navigate to when this image is clicked.")]
        [Category("Navigation"),DefaultValue("")]
        public string NavigateUrl
        {
            get { return _NavigateUrl; }
            set { _NavigateUrl = value; }
        }   
        private string _NavigateUrl = "";

        /// <summary>
        /// Optional - The target frame to display the result in.
        /// </summary>
        [Description("Optional - The target frame to display the result in."),
         DefaultValue(""), Category("Navigation")]
        public string Target
        {
            get { return _Target; }
            set { _Target = value; }
        }
        private string _Target = "";

                
        public string Text
        {
            get { return _Text; }
            set { _Text = value; }
        }
        private string _Text = "";



        public override void RenderBeginTag(HtmlTextWriter writer)
        {
            writer.Write("<a href='" + ResolveUrl(NavigateUrl) + "'");

            if (!string.IsNullOrEmpty(Target))
                writer.Write(" Target='" + Target + "'");
            writer.Write(">");

            Attributes.Add("border", "0");
            
            base.RenderBeginTag(writer);
        }

        public override void RenderEndTag(HtmlTextWriter writer)
        {
            base.RenderEndTag(writer);

            if (!string.IsNullOrEmpty(Text))
                writer.Write(" " + Text);
                
            writer.Write("</a>");
        }
    }
}
