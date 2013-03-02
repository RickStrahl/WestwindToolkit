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
using System.Text;
using System.Web.UI.WebControls;
using System.Web;
using System.Globalization;
using System.ComponentModel;
using System.Drawing;
using System.Web.UI;

using Westwind.Utilities;
using Westwind.Web.JsonSerializers;

namespace Westwind.Web.Controls
{

    /// <summary>
    /// ASP.NET jQuery DatePicker Control Wrapper
    /// by Rick Strahl
    /// http://www.west-wind.com/
    /// 
    /// License: Free
    /// 
    /// Simple DatePicker control that uses jQuery UI DatePicker to pop up 
    /// a date picker. 
    /// 
    /// Important Requirements:
    /// ~/scripts/jquery.js             (available from WebResource)
    /// ~/scripts/jquery-ui.js   (custom build of jQuery.ui)
    /// ~/scripts/themes/base           (choose any theme name one theme to display styling)
    /// 
    /// Resources are embedded into the assembly so you don't need
    /// to reference or distribute anything. You can however override
    /// each of these resources with relative URL based resources.
    /// </summary>
    [ToolboxBitmap(typeof(System.Web.UI.WebControls.Calendar)), DefaultProperty("Text"),
    ToolboxData("<{0}:jQueryDatePicker runat=\"server\"  />")]
    public class jQueryDatePicker : TextBox
    {

        public jQueryDatePicker()
        {
            // Date specific width
            Width = Unit.Pixel(80);
        }

        /// <summary>
        /// The currently selected date
        /// </summary>
        [DefaultValue(typeof(DateTime), ""),
        Category("Date Selection")]
        public DateTime? SelectedDate
        {
            get
            {
                DateTime defaultDate = DateTime.Parse("01/01/1900", CultureInfo.InstalledUICulture);

                if (Text == "")
                    return defaultDate;

                DateTime.TryParse(Text, out defaultDate);
                return defaultDate;
            }
            set
            {
                if (!value.HasValue)
                    Text = "";
                else
                {
                    string dateFormat = DateFormat;
                    if (dateFormat == "Auto")
                        dateFormat = CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern;
                    Text = value.Value.ToString(dateFormat);
                }
            }
        }


        /// <summary>
        /// Determines how the datepicking option is activated
        /// </summary>
        [Description("Determines how the datepicking option is activated")]
        [Category("Date Selection"), DefaultValue(typeof(DatePickerDisplayModes), "ImageButton")]
        public DatePickerDisplayModes DisplayMode
        {
            get { return _DisplayMode; }
            set { _DisplayMode = value; }
        }
        private DatePickerDisplayModes _DisplayMode = DatePickerDisplayModes.ImageButton;



        /// <summary>
        /// Url to a Calendar Image or WebResource to use the default resource image.
        /// Applies only if the DisplayMode = ImageButton
        /// </summary>
        [Description("Url to a Calendar Image or WebResource to use the default resource image")]
        [Category("Resources"), DefaultValue("WebResource")]
        public string ButtonImage
        {
            get { return _ButtonImage; }
            set { _ButtonImage = value; }
        }
        private string _ButtonImage = "WebResource";

        /// <summary>
        /// The CSS that is used for the calendar
        /// </summary>
        [Category("Resources"), Description("The CSS that is used for the calendar or empty. WebResource loads from resources. This property serves as the base url - use Theme to apply a specific theme"),
         DefaultValue("~/scripts/themes/base/jquery-ui-all.css")]
        public string CalendarCss
        {
            get { return _CalendarCss; }
            set { _CalendarCss = value; }
        }
        private string _CalendarCss = "~/scripts/themes/base/jquery.ui.all.css";


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
        /// Location for the calendar JavaScript
        /// </summary>
        [Description("Location for the calendar JavaScript or empty for none. WebResource loads from resources")]
        [Category("Resources"), DefaultValue("~/scripts/jquery-ui.js")]
        public string CalendarJs
        {
            get { return _CalendarJs; }
            set { _CalendarJs = value; }
        }
        private string _CalendarJs = "~/scripts/jquery-ui.js";


        /// <summary>
        /// Location of jQuery library. Use WebResource for loading from resources
        /// </summary>
        [Description("Location of jQuery library or empty for none. Use WebResource for loading from resources")]
        [Category("Resources"), DefaultValue("WebResource")]
        public string jQueryJs
        {
            get { return _jQueryJs; }
            set { _jQueryJs = value; }
        }
        private string _jQueryJs = "WebResource";


        /// <summary>
        /// Determines the Date Format used. Auto uses CurrentCulture. Format: MDY/  month, date,year separator
        /// </summary>
        [Description("Determines the Date Format used. Auto uses CurrentCulture. Format: MDY/  month, date,year separator")]
        [Category("Date Selection"), DefaultValue("Auto")]
        public string DateFormat
        {
            get { return _DateFormat; }
            set { _DateFormat = value; }
        }
        private string _DateFormat = "Auto";

        /// <summary>
        /// Minumum allowable date. Leave blank to allow any date
        /// </summary>
        [Description("Minumum allowable date")]
        [Category("Date Selection"), DefaultValue(typeof(DateTime?), null)]
        public DateTime? MinDate
        {
            get { return _MinDate; }
            set { _MinDate = value; }
        }
        private DateTime? _MinDate = null;

        /// <summary>
        /// Maximum allowable date. Leave blank to allow any date.
        /// </summary>
        [Description("Maximum allowable date. Leave blank to allow any date.")]
        [Category("Date Selection"), DefaultValue(typeof(DateTime?), null)]
        public DateTime? MaxDate
        {
            get { return _MaxDate; }
            set { _MaxDate = value; }
        }
        private DateTime? _MaxDate = null;


        /// <summary>
        /// Client event handler fired when a date is selected
        /// </summary>
        [Description("Client event handler fired when a date is selected")]
        [Category("Date Selection"), DefaultValue("")]
        public string OnClientSelect
        {
            get { return _OnClientSelect; }
            set { _OnClientSelect = value; }
        }
        private string _OnClientSelect = "";


        /// <summary>
        /// Client event handler that fires before the date picker is activated
        /// </summary>
        [Description("Client event handler that fires before the date picker is activated")]
        [Category("Date Selection"), DefaultValue("")]
        public string OnClientBeforeShow
        {
            get { return _OnClientBeforeShow; }
            set { _OnClientBeforeShow = value; }
        }
        private string _OnClientBeforeShow = "";


        /// <summary>
        /// Determines where the Close icon is displayed. True = top, false = bottom.
        /// </summary>
        [Description("Determines where the Today and Close buttons are displayed on the bottom (default styling) of the control.")]
        [Category("Date Selection"), DefaultValue(true)]
        public bool ShowButtonPanel
        {
            get { return _CloseAtTop; }
            set { _CloseAtTop = value; }
        }
        private bool _CloseAtTop = true;


        /// <summary>
        /// Code that embeds related resources (.js and css)
        /// </summary>
        /// <param name="scriptProxy"></param>
        protected void RegisterResources(ClientScriptProxy scriptProxy)
        {
            scriptProxy.LoadControlScript(this, jQueryJs, WebResources.JQUERY_SCRIPT_RESOURCE, ScriptRenderModes.HeaderTop);
            scriptProxy.RegisterClientScriptInclude(Page, typeof(WebResources), CalendarJs, ScriptRenderModes.Header);

            string cssPath = CalendarCss;
            if (!string.IsNullOrEmpty(Theme))
                cssPath = cssPath.Replace("/base/", "/" + Theme + "/");

            scriptProxy.RegisterCssLink(Page, typeof(WebResources), cssPath, cssPath);
        }

        protected override void  OnInit(EventArgs e)
        {
                base.OnInit(e);

                // Retrieve the date explicitly - NOTE: Date written by CLIENTID id & name.
                if (Page.IsPostBack && DisplayMode == DatePickerDisplayModes.Inline)
                    Text = Page.Request.Form[ClientID]; // Note this is the right value!            
        }

        

        
        /// <summary>
        /// Most of the work happens here for generating the hook up script code
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            // MS AJAX aware script management
            ClientScriptProxy scriptProxy = ClientScriptProxy.Current;

            // Register resources
            RegisterResources(scriptProxy);

            string dateFormat = DateFormat;

            if (string.IsNullOrEmpty(dateFormat) || dateFormat == "Auto")
            {
                // Try to create a data format string from culture settings
                // this code will fail if culture can't be mapped on server hence the empty try/catch
                try
                {
                    dateFormat = CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern;
                }
                catch { }
            }
            
            dateFormat = dateFormat.ToLower().Replace("yyyy", "yy");

            // Capture and map the various option parameters
            StringBuilder sbOptions = new StringBuilder(512);
            sbOptions.Append("{");

            string onSelect = OnClientSelect;

            if (DisplayMode == DatePickerDisplayModes.Button)
                sbOptions.Append("showOn: 'button',");
            else if (DisplayMode == DatePickerDisplayModes.ImageButton)
            {
                string img = ButtonImage;
                if (img == "WebResource")
                    img = scriptProxy.GetWebResourceUrl(this, typeof(WebResources), WebResources.CALENDAR_ICON_RESOURCE);
                else
                    img = ResolveUrl(ButtonImage);

                sbOptions.Append("showOn: 'button', buttonImageOnly: true, buttonImage: '" + img + "',buttonText: 'Select date',");
            }
            else if (DisplayMode == DatePickerDisplayModes.Inline)
            {                
                // need to store selection in the page somehow for inline since it's
                // not tied to a textbox
                scriptProxy.RegisterHiddenField(this, ClientID, Text);
                onSelect = ClientID + "OnSelect";
            }

            if (!string.IsNullOrEmpty(onSelect))
                sbOptions.Append("onSelect: " + onSelect + ",");

            if (DisplayMode != DatePickerDisplayModes.Inline)
            {
                if (!string.IsNullOrEmpty(OnClientBeforeShow))
                    sbOptions.Append("beforeShow: function(y,z) { $('#ui-datepicker-div').maxZIndex(); " + 
                                     OnClientBeforeShow + "(y,z); },");
                else
                    sbOptions.Append("beforeShow: function() { $('#ui-datepicker-div').maxZIndex(); },");
                        
            }

            if (MaxDate.HasValue)
                sbOptions.Append("maxDate: " + WebUtils.EncodeJsDate(MaxDate.Value) + ",");

            if (MinDate.HasValue)
                sbOptions.Append("minDate: " + WebUtils.EncodeJsDate(MinDate.Value) + ",");

            if (ShowButtonPanel)
                sbOptions.Append("showButtonPanel: true,");

            sbOptions.Append("dateFormat: '" + dateFormat + "'}");


            // Write out initilization code for calendar
            StringBuilder sbStartupScript = new StringBuilder(400);
            sbStartupScript.AppendLine("$( function() {");


            if (DisplayMode != DatePickerDisplayModes.Inline)
            {
                scriptProxy.RegisterClientScriptBlock(Page,
                                                      typeof(WebResources),
                                                      "__attachDatePickerInputKeys",
                                                      AttachDatePickerKeysScript, true);

                sbStartupScript.AppendFormat("var cal = jQuery('#{0}').datepicker({1}).attachDatepickerInputKeys();\r\n",
                                             ClientID, sbOptions);
            }
            else
            {
                sbStartupScript.AppendLine("var cal = jQuery('#" + ClientID + "Div').datepicker(" + sbOptions.ToString() + ")");

                if (SelectedDate.HasValue && SelectedDate.Value > new DateTime(1900, 1, 1, 0, 0, 0, DateTimeKind.Utc))
                {
                    WestwindJsonSerializer ser = new WestwindJsonSerializer();
                    ser.DateSerializationMode = JsonDateEncodingModes.NewDateExpression;
                    string jsDate = ser.Serialize(SelectedDate);

                    sbStartupScript.AppendLine("cal.datepicker('setDate'," + jsDate + ");");
                }
                else
                    sbStartupScript.AppendLine("cal.datepicker('setDate',new Date());");

                // Assign value to hidden form var on selection
                scriptProxy.RegisterStartupScript(this, typeof(WebResources), UniqueID + "OnSelect",
                    "function  " + ClientID + "OnSelect(dateStr) {\r\n" +                    
                    ((!string.IsNullOrEmpty(OnClientSelect)) ? OnClientSelect + "(dateStr);\r\n" : "") +
                    "jQuery('#" + ClientID + "')[0].value = dateStr;\r\n}\r\n", true);
            }

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
            if (DisplayMode != DatePickerDisplayModes.Inline)
                base.RenderControl(writer);
            else
            {
                
                if (DesignMode)
                    writer.Write("<div id='" + ClientID + "Div' style='width: 200px; height: 200px; padding: 20px;background: silver; color; white'>Inline Calendar Placeholder</div>");
                else
                    writer.Write("<div id='" + ClientID + "Div'></div>");
            }

            // this code is only for the designer
            if (HttpContext.Current == null)
            {
                if (DisplayMode == DatePickerDisplayModes.Button)
                {
                    writer.Write(" <input type='button' value='...' style='width: 20px; height: 20px;' />");
                }
                else if ((DisplayMode == DatePickerDisplayModes.ImageButton))
                {
                    string img;
                    if (ButtonImage == "WebResource")
                        img = Page.ClientScript.GetWebResourceUrl(GetType(), WebResources.CALENDAR_ICON_RESOURCE);
                    else
                        img = ResolveUrl(ButtonImage);

                    writer.AddAttribute(HtmlTextWriterAttribute.Src, img);
                    writer.AddAttribute("hspace", "2");
                    writer.RenderBeginTag(HtmlTextWriterTag.Img);
                    writer.RenderEndTag();
                }
            }
        }

        private string AttachDatePickerKeysScript =
@"
$.fn.attachDatepickerInputKeys = function(callback) {
    if (this.length < 1) return this;    

    this.keydown(function(e) {
        var j = jQuery(this);
        var di = $.datepicker._getInst(this);
        if (!di)
            return;

        $.datepicker._setDateFromField(di);  // force update first

        var d = j.datepicker('getDate');
        if (!d)
            d = new Date(1900,0,1,1,1);

        var month = d.getMonth();
        var year = d.getFullYear();
        var day = d.getDate();

        switch (e.keyCode) {
            case 84: // [T]oday
                d = new Date(); break;
            case 109: case 189:
                d = new Date(year, month, day - 1); break;
            case 107: case 187:
                d = new Date(year, month, day + 1); break;
            case 77: //M
                d = new Date(year, month - 1, day); break;
            case 72: //H
                d = new Date(year, month + 1, day); break;
            default:
                return true;
        }
        
        j.datepicker('setDate', d);
        if (callback)
            callback(this);
        return false;
    });
    return this;
}
$.maxZIndex = $.fn.maxZIndex = function(opt) {
    var def = { inc: 10, group: ""*""};
    $.extend(def, opt);
    var zmax = 0;
    $(def.group).each(function() {
        var cur = parseInt($(this).css('z-index'));
        zmax = cur > zmax ? cur : zmax;
    });
    if (!this.jquery)
        return zmax;

    return this.each(function() {
        zmax += def.inc;
        $(this).css(""z-index"", zmax);
    });
}
";
    }


    public enum DatePickerDisplayModes
    {
        Button,
        ImageButton,
        AutoPopup,
        Inline
    }
}
