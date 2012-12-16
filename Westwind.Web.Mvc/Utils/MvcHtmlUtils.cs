using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Westwind.Utilities;
using System.Collections;
using System.IO;

namespace Westwind.Web.Mvc
{
    /// <summary>
    /// Set of generic MVC Html Helper utilities
    /// </summary>
    public static class MvcHtmlUtils
    {

        /// <summary>
        /// Returns a selectListItem list for an enumerated value
        /// </summary>
        /// <param name="enumType"></param>
        /// <returns></returns>
        public static IEnumerable<SelectListItem> SelectListItemsFromEnum(Type enumType, bool valueAsFieldValueNumber = false)
        {
            var vals = ReflectionUtils.GetEnumList(enumType,valueAsFieldValueNumber);            

            foreach (var val in vals)
            {
                yield return  new SelectListItem() 
                { 
                    Value = val.Key,
                    Text = val.Value 
                };                
            }
        }

        /// <summary>
        /// Takes a dictionary of values and turns it into a SelectListItems enumeration.
        /// Converts the dictionary keys  into the value and the dictionary value into
        /// the text.
        /// </summary>
        /// <remarks>
        /// Assumes the dictionary keys and values can be turned 
        /// into strings using ToString()
        /// </remarks>
        /// <param name="dict"></param>
        /// <returns></returns>
        public static IEnumerable<SelectListItem> SelectListItemsFromDictionary(IDictionary dict)
        {
            foreach (var entry in dict.Keys)
            {
                yield return new SelectListItem()
                {
                     Text = dict[entry].ToString(),
                     Value = entry.ToString()                    
                };
            }
        }


        /// <summary>
        /// Renders a Razor view to a string.
        /// </summary>
        /// <param name="viewPath"></param>
        /// <param name="model"></param>
        /// <param name="controllerContext"></param>
        /// <returns></returns>
        public static string RenderPartialViewToString(string viewPath, object model,
                                                ControllerContext controllerContext)
        {
            string error = null;
            return RenderViewToString(viewPath, model, controllerContext, true, out error);
        }

        /// <summary>
        /// Renders a Razor view to a string.
        /// </summary>
        /// <param name="viewPath"></param>
        /// <param name="model"></param>
        /// <param name="controllerContext"></param>
        /// <returns></returns>
        public static string RenderViewToString(string viewPath, object model,
                                                ControllerContext controllerContext)
        {
            string error = null;
            return RenderViewToString(viewPath, model, controllerContext, false, out error);
        }

        /// <summary>
        /// Renders a view to a string
        /// </summary>
        /// <param name="viewPath">
        /// The path to the view to render. Either in same controller, shared by 
        /// name or as fully qualified ~/ path including extension
        /// </param>
        /// <param name="model"></param>
        /// <param name="controllerContext"></param>
        /// <param name="partial">If true renders partial view (ie. no _layout pages)</param>
        /// <param name="razorErrorMessage">Error message to capture</param>
        /// <returns></returns>
        public static string RenderViewToString(string viewPath, object model,
                                                ControllerContext controllerContext,
                                                bool partial,
                                                out string razorErrorMessage)
        {
            razorErrorMessage = null;

            // first find the ViewEngine for this view
            ViewEngineResult viewEngineResult = null;            
            if (partial)
                viewEngineResult = ViewEngines.Engines.FindPartialView(controllerContext, viewPath);
            else
                viewEngineResult = ViewEngines.Engines.FindView(controllerContext, viewPath, null);

            if (viewEngineResult == null)
            {
                razorErrorMessage = Properties.Resources.ViewCouldNotBeFound;
                return null;
            }
            
            var view = viewEngineResult.View;
            

            //var viewData = new ViewDataDictionary();
            //viewData.Model = model;
            
            //var vr = new ViewResult
            //{
            //    ViewName = viewPath,
            //    MasterName = null,
            //    ViewData = viewData,
            //    TempData = null,
            //    ViewEngineCollection = ViewEngines.Engines
            //};

            controllerContext.Controller.ViewData.Model = model;
            
                                                            
            string result = String.Empty;
            try
            {
                using (var sw = new StringWriter())
                {
                    var ctx = new ViewContext(controllerContext, view,
                                              controllerContext.Controller.ViewData,
                                              controllerContext.Controller.TempData,
                                              sw);
                    view.Render(ctx, sw);
                    result = sw.ToString();
                }
            }
            catch (Exception ex)
            {
                razorErrorMessage = ex.GetBaseException().Message;
                return null;
            }

            return result;
        }

    }
}
