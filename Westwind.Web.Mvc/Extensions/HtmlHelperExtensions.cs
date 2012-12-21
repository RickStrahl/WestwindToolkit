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
using System.IO;

namespace Westwind.Web.Mvc
{
    /// <summary>
    /// Application specific UrlHelper Extensions
    /// </summary>
    public static class HtmlHelperExtensions
    {

        /// <summary>
        /// Generic routine that embeds a control derived off base control into the
        /// current output stream.
        /// 
        /// DateControl
        /// ErrorDisplay        
        /// </summary>
        /// <typeparam name="ControlType"></typeparam>
        /// <param name="htmlHelper"></param>
        /// <param name="name"></param>
        /// <param name="control"></param>
        /// <param name="htmlAttributes"></param>
        /// <returns></returns>
        public static HtmlString Control<ControlType>(this HtmlHelper htmlHelper, string name, 
                                                      ControlType control, object htmlAttributes) 
                                                where ControlType : BaseControl
        {            
            control.HtmlHelper = htmlHelper;
            control.UrlHelper = new UrlHelper(htmlHelper.ViewContext.RequestContext);
            control.Id = name.Replace(".","_");
            control.Name = name;

            control.MergeAttributes(htmlAttributes);
            return new HtmlString(control.Render());
        }                
    }
}