#region License
/*
 **************************************************************
 *  Author: Rick Strahl 
 *          © West Wind Technologies, 2008 - 2011
 *          http://www.west-wind.com/
 * 
 * Created: 
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
using System.ComponentModel.Design;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Drawing.Design;

using Westwind.Utilities;

namespace Westwind.Web.Controls
{


    /// <summary>
    /// The ErrorDisplay control provides a simple mechanism for displaying error
    ///  and status messages in an easy to use, attractive and reusable control.
    /// 
    /// The class includes several ways to do display data (ShowError, ShowMessage)
    ///  as well as direct assignment to the .Text property which allows direct 
    /// display of content.
    /// 
    /// Assigning to .Text and with a UserMessage set:
    /// &lt;&lt;img  src="images\wwErrorMsg.png"&gt;&gt;
    /// 
    /// ShowMessage:
    /// &lt;&lt;img  src="images\wwErrorMsg_Msg.png"&gt;&gt;
    /// 
    /// ShowError:
    /// &lt;&lt;img  src="images\wwErrorMsg_Error.png"&gt;&gt;
    /// 
    /// ShowError and ShowMessage are plain display mechanisms that show only the  
    /// essage specified along with an icon to the left of the message. You can 
    /// also assign the Text property directly which assign the message body text. 
    ///  The UserMessage displays at the top of the control is configurable. You 
    /// can also have the control timeout and 'fade out' after a few seconds of  
    /// dplaying the message text.
    /// </summary>
    [ToolboxBitmap(typeof(ValidationSummary))]
    [ToolboxData("<{0}:ErrorDisplay />")]
    [DefaultProperty("Text")]
    public class ErrorDisplay : WebControl
    {
        private ClientScriptProxy ClientScriptProxy = null;
        private new bool DesignMode = HttpContext.Current == null;
        
        /// <summary>
        /// The detail text of the error message
        /// </summary>
        [Description("The error message to be displayed."), Category("ErrorMessage"), DefaultValue("")]
        [Localizable(true)]
        public string Text
        {
            get
            {
                return _Text;
            }
            set
            {                
                _Text = value ;
            }
        }
        private string _Text = "";


        /// <summary>
        /// The message to display above the error message.
        /// For example: Please correct the following:
        /// </summary>
        [Description("The message to display above the error strings."), 
         Category("ErrorMessage"), 
         DefaultValue(""),
         Localizable(true)]
        public string UserMessage
        {
            get
            {
                return _UserMessage;
            }
            set
            {
                _UserMessage = value;
            }
        }
        private string _UserMessage = "";

        

        /// <summary>
        /// An image Url that is displayed with the ShowMessage method. Defaults to 
        /// InfoResource which loads an icon from the control assembly.
        /// <seealso>Class ErrorDisplay</seealso>
        /// </summary>
        [Description("The image to display when ShowMessage is called. Default value is InfoResource which loads an image resource."), Category("ErrorMessage"),
         Editor("System.Web.UI.Design.ImageUrlEditor", typeof(UITypeEditor)), DefaultValue("InfoResource")]
        public string InfoImage
        {
            get { return _InfoImage; }
            set { _InfoImage = value; }
        }
        private string _InfoImage = "InfoResource";



        /// <summary>
        /// Determines whether the display box is centered
        /// </summary>
        [Category("ErrorMessage"), Description("Centers the Error Display on the page."), DefaultValue(true)]
        public bool Center
        {
            get
            {
                return _CenterDisplay;
            }
            set
            {
                _CenterDisplay = value;
            }
        }
        private bool _CenterDisplay = true;

        /// <summary>
        /// Determines whether the control keeps its space padding
        /// when it is is hidden in order not to jump the display.
        /// Controls the visibility style attribute.
        /// </summary>
        [Category("ErrorMessage"),DefaultValue(false),
        Description("Determines whether the control keeps its space padding when it is is hidden. Controls visibility style attribute.")]
        public bool UseFixedHeightWhenHiding
        {
            get { return _UseFixedHeightWhenHiding; }
            set { _UseFixedHeightWhenHiding = value; }
        }
        private bool _UseFixedHeightWhenHiding = false;


        /// <summary>
        /// Flag that determines whether the message is displayed
        /// as HTML or as text. By default message is encoded as text (true).
        /// </summary>
        [Category("ErrorMessage"), Description("Determines whether the message is formatted as text (true) or raw html (false)"), DefaultValue(false)]        
        public bool HtmlEncodeMessage
        {
            get { return _HtmlEncodeMessage; }
            set { _HtmlEncodeMessage = value; }
        }
        private bool _HtmlEncodeMessage = false;


        /// <summary>
        /// Determines how the error dialog renders
        /// </summary>
        [Category("ErrorMessage"), Description("Determines whether the control renders text or Html"), DefaultValue(RenderModes.Html)]
        public RenderModes RenderMode
        {
            get
            {
                return _RenderMode;
            }
            set
            {
                _RenderMode = value;
            }
        }
        private RenderModes _RenderMode = RenderModes.Html;

        /// <summary>
        /// The width of the ErrorDisplayBox
        /// </summary>
        [Description("The width for the control"),DefaultValue("400px")]
        public new Unit Width
        {
            get
            {
                return _Width;
            }
            set
            {
                _Width = value;
            }
        }
        private Unit _Width = Unit.Pixel(400);


        /// <summary>
        /// The CSS Class used for the table and column to display this item.
        /// </summary>
        [DefaultValue("errordisplay")]
        public new string CssClass
        {
            get
            {
                return _CssClass;
            }
            set
            {
                _CssClass = value;
            }
        }
        private string _CssClass = "errordisplay";


        /// <summary>
        /// Holds a modelstate errors collection
        /// </summary>
        public ValidationErrorCollection DisplayErrors
        {
            get
            {
                if (_DisplayErrors == null)
                    _DisplayErrors = new ValidationErrorCollection();

                return _DisplayErrors;
            }
        }
        private ValidationErrorCollection _DisplayErrors = null;


        /// <summary>
        /// A timeout in milliseconds for how long the error display is visible. 0 means no timeout.
        /// </summary>
        [Description("A timeout in milliseconds for how long the error display is visible. 0 means no timeout."), DefaultValue(0)]
        public int DisplayTimeout
        {
            get { return _DisplayTimeout; }
            set { _DisplayTimeout = value; }
        }
        private int _DisplayTimeout = 0;

        private ErrorDisplayTypes ErrorDisplayType = ErrorDisplayTypes.Error;

        protected void RenderTop(HtmlTextWriter writer)
        {

            foreach (string Key in Style.Keys)
            {
                writer.AddStyleAttribute(Key, Style[Key]);
            }
            writer.AddAttribute(HtmlTextWriterAttribute.Class, this.CssClass);
            
            writer.Write("<div class=\"errordisplay\" ");

            if (Width != 0)
                writer.AddStyleAttribute("width", Width.ToString());
            if (Center)
            {
                writer.AddStyleAttribute(HtmlTextWriterStyle.MarginLeft, "auto");
                writer.AddStyleAttribute(HtmlTextWriterStyle.MarginRight, "auto");
            }
            writer.RenderBeginTag(HtmlTextWriterTag.Div);

        }

        protected void RenderBottom(HtmlTextWriter writer)
        {
            // close out the dialog
            writer.RenderEndTag();  //div
            //RenderBeginTag(HtmlTextWriterTag.Div);
        }

        protected void RenderDisplayErrors(HtmlTextWriter writer)
        {
            if (this.DisplayErrors.Count > 0)
            {
                writer.WriteLine("<hr/>");
                writer.WriteLine(this.DisplayErrors.ToHtml());
            }
        }

        /// <summary>
        /// Method 
        /// </summary>
        /// <param name="width"></param>
        /// <param name="center"></param>
        /// <returns></returns>
        protected override void Render(HtmlTextWriter writer)
        {

            if (string.IsNullOrEmpty(Text) && !DesignMode)
            {
                   base.Render(writer);
                   return;
            }

            // In designmode we want to write out a container so it
            // so the designer properly shows the controls as block control
            if (DesignMode)
                writer.WriteLine("<div style='width:100%'>");

            RenderTop(writer);

            if (ErrorDisplayType == ErrorDisplayTypes.Error)
                writer.WriteLine("<div class=\"errordisplay-warning-icon\"></div>");
            else
                writer.WriteLine("<div class=\"errordisplay-info-icon\"></div>");

            writer.WriteLine("<div class=\"errordisplay-text\">");

            writer.WriteLine(this.HtmlEncodeMessage ? HttpUtility.HtmlEncode(Text) : Text);
            RenderDisplayErrors(writer);

            writer.WriteLine("</div>");

            RenderBottom(writer);

            if (DesignMode)
                writer.WriteLine("</div>");
            
        }


        ///// <summary>
        ///// Renders the container
        ///// </summary>
        ///// <param name="writer"></param>
        //protected override void Render(HtmlTextWriter writer)
        //{
        //    if (Text == "" && !DesignMode)
        //    {
        //        base.Render(writer);
        //        return;
        //    }

        //    if (RenderMode == RenderModes.Text)
        //        Text = HtmlUtils.DisplayMemo(Text);

        //    string TStyle = Style["position"];
        //    bool IsAbsolute = false;
        //    if (TStyle != null && TStyle.Trim() == "absolute")
        //        IsAbsolute = true;

        //    // <Center> is still the only reliable way to get block structures centered
        //    if (!IsAbsolute && Center)
        //    {                
        //        writer.AddStyleAttribute(HtmlTextWriterStyle.MarginLeft, "auto");
        //        writer.AddStyleAttribute(HtmlTextWriterStyle.MarginRight, "auto");

        //        // In designmode we want to write out a container so it
        //        // so the designer properly shows the controls as block control
        //        if (DesignMode)
        //            writer.Write("<div style='width:100%'>");
        //    }

        //    writer.AddAttribute(HtmlTextWriterAttribute.Id, ClientID);
        //    writer.AddAttribute(HtmlTextWriterAttribute.Class, CssClass);                      
        //    writer.AddStyleAttribute(HtmlTextWriterStyle.Width, Width.ToString());
            
        //    foreach (string Key in Style.Keys)
        //    {
        //        writer.AddStyleAttribute(Key, Style[Key]);
        //    }

        //    writer.RenderBeginTag(HtmlTextWriterTag.Div);

            
        //    writer.RenderBeginTag(HtmlTextWriterTag.Table);

        //    writer.RenderBeginTag(HtmlTextWriterTag.Tr);

        //    // Set up  image <td> tag                                 
        //    writer.RenderBeginTag(HtmlTextWriterTag.Td);

        //    if (ErrorImage != "")
        //    {
        //        string ImageUrl = ErrorImage.ToLower();
        //        if (ImageUrl == "warningresource")
        //            ImageUrl = Page.ClientScript.GetWebResourceUrl(GetType(), WebResources.WARNING_ICON_RESOURCE);
        //        else if (ImageUrl == "inforesource")
        //            ImageUrl = Page.ClientScript.GetWebResourceUrl(GetType(), WebResources.INFO_ICON_RESOURCE);
        //        else
        //            ImageUrl = ResolveUrl(ErrorImage);

        //        writer.AddAttribute(HtmlTextWriterAttribute.Src, ImageUrl);
        //        writer.RenderBeginTag(HtmlTextWriterTag.Img);
        //        writer.RenderEndTag();
        //    }

        //    writer.RenderEndTag();  // image <td>

        //    // Render content <td> tag            
        //    writer.RenderBeginTag(HtmlTextWriterTag.Td);

        //    if (UserMessage != "")
        //        writer.Write("<span style='font-weight:normal'>" + UserMessage + "</span><hr />");

        //    writer.Write(Text);

        //    writer.RenderEndTag();  // Content <td>
        //    writer.RenderEndTag();  // </tr>
        //    writer.RenderEndTag();  // </table>

        //    writer.RenderEndTag();  // </div>

        //    if (!IsAbsolute)
        //    {
        //        if (Center)
        //        {
        //            if (DesignMode)
        //                writer.Write("</div>");  // </div>
                    
        //        }

        //        writer.WriteBreak();
        //    }    
        //}


        protected override void OnPreRender(EventArgs e)
        {
            ClientScriptProxy = ClientScriptProxy.Current;

            if (Visible && !string.IsNullOrEmpty(Text) && DisplayTimeout > 0)
            {
                // Embed the client script library as Web Resource Link
                ScriptLoader.LoadjQuery(this);

                string Script =
                    @"setTimeout(function() { $('#" + ClientID + @"').fadeOut('slow') }," + DisplayTimeout.ToString() + ");";

                //@"window.setTimeout(""document.getElementById('" + ClientID + @"').style.display='none';""," + DisplayTimeout.ToString() + @");";

                ClientScriptProxy.RegisterStartupScript(this, typeof(ErrorDisplay), "DisplayTimeout", Script, true);
            }
            base.OnPreRender(e);
        }

        /// <summary>
        /// Assigns an error message to the control
        /// <seealso>Class ErrorDisplay</seealso>
        /// </summary>
        /// <param name="Text">
        /// The main message text that is displayed.
        /// </param>
        public void ShowError(string Text)
        {
            ShowError(Text, null);
        }

        /// <summary>
        /// Assigns an error message to the control as well as a UserMessage
        /// <seealso>Class ErrorDisplay</seealso>
        /// </summary>
        /// <param name="text">
        /// The main message text that is displayed.
        /// </param>
        /// <param name="Message">
        /// Optional Message header shown above the message text.
        /// </param>
        public void ShowError(string text, string Message)
        {
            Text = text;

            if (Message != null)
                UserMessage = Message;
            else
                UserMessage = "";

            Visible = true;
        }

        /// <summary>
        /// Displays a simple message in the display area along with the info icon 
        /// before it.
        /// <seealso>Class ErrorDisplay</seealso>
        /// </summary>
        /// <param name="Message">
        /// The message to display.
        /// </param>
        public void ShowMessage(string Message)
        {
            UserMessage = "";            
            Text = Message;
            Visible = true;
        }
    }

    public enum RenderModes
    {
        /// <summary>
        /// Error Text is Text and needs fixing up
        /// </summary>
        Text,
        /// <summary>
        /// The text is HTML and ready to display
        /// </summary>
        Html,
        /// <summary>
        /// Text is plain text and should be rendered as a bullet list
        /// </summary>
        TextAsBulletList
    }


    public enum ErrorDisplayTypes
    {
        Error,
        Message
    }
}
