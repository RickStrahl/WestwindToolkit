using System;
using System.Web;
using System.Web.Mvc;

using Westwind.Web.Mvc;
using Westwind.Utilities;
using System.Text;
using System.Collections.Generic;
using System.Web.Routing;
using System.Globalization;
using System.Collections;

namespace Westwind.Web.Mvc
{
    /// <summary>
    /// Simple Implemnetation of a Date Control that uses
    /// ui.jquery's Date picker control. Very rudimentary
    /// 
    /// Requires: jquery.js and ui.jquery.js loaded (which includes date picker)
    ///           expects ui. themes in folder below scripts
    /// </summary>
    public class DateControl : BaseControl
    {
        public string ButtonImage = "~/css/images/calendar.gif";
        public string jQueryUiCss = ScriptLoader.jQueryUiCssBaseUrl;
        public string Theme = ScriptLoader.jQueryUiTheme;
        public DateTime SelectedDate = DateTime.MinValue;
        public string DateFormat = "d";
        public bool LoadCss = false;

        public override void ApplyAttributes()
        {
            base.ApplyAttributes();
            this.Attributes.Add("name", this.Name);
        }
        protected override void PreRender() 
        {
            if (this.SelectedDate > DateTime.MinValue.Date.AddDays(1))
                this.Value = this.SelectedDate.ToString(this.DateFormat);

            Output.Append("<input type=\"text\" ");
            
            this.RenderAttributes();
            
            if (!string.IsNullOrEmpty(this.Value))
                Output.Append("value=\"" + this.Value + "\" ");

            Output.Append("/>");

            Page.EmbedScriptString(Output,
@"$(document).ready(function() {
$('#" + this.Id +  @"').datepicker({ showOn: 'button', buttonImageOnly: true, buttonImage: '" + this.UrlHelper.Content(this.ButtonImage) + 
@"', buttonText: 'Select date' }).attachDatepickerInputKeys();
});",this.Id,true);
            
            if (this.IsFirstRenderOnPage)
            {
                this.IsFirstRenderOnPage = false;
                this.Page.EmbedScriptString(Output,this.script,"_jqueryUiAttachKeys",true);

                if (this.LoadCss)
                {
                    string cssUrl = this.jQueryUiCss;
                    if (!string.IsNullOrEmpty(Theme))
                        cssUrl = cssUrl.Replace("/base/", "/" + Theme + "/");
                    this.Page.EmbedCssLink(Output, cssUrl, "_jqueryDatePickerCss");
                }
            }
        }

        private string script = 
@"
jQuery.fn.attachDatepickerInputKeys = function(callback) {
    if (this.length < 1) return this;

    this.keydown(function(e) {
        var j = jQuery(this);
        var di = jQuery.datepicker._getInst(this);
        if (!di)
            return;

        jQuery.datepicker._setDateFromField(di);  // force update first

        var d = j.datepicker('getDate');
        if (!d)
            return true;

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
";
    }
}
