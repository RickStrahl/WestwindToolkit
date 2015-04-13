using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;
using Westwind.Web;

namespace AlbumViewerAngular
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class CallbackExceptionHandlerAttribute : HandleErrorAttribute
    {
        public CallbackExceptionHandlerAttribute()
        {
            AllowExceptionDetail = HttpContext.Current.IsDebuggingEnabled;
        }

        public CallbackExceptionHandlerAttribute(bool allowExceptionDetail)
        {
            AllowExceptionDetail = allowExceptionDetail;
        }

        /// <summary>
        /// Determines whether ExceptionDetail is allowed
        /// </summary>
        public bool AllowExceptionDetail { get; set; }   

        /// <summary>
        /// When true returns standard ASP.NET yellow screen of death
        /// when DebugMode is enabled.
        /// </summary>
        public bool ShowErrorPageInDebugMode { get; set; }
     

        public override void OnException(ExceptionContext filterContext)
        {            
            var response = filterContext.HttpContext.Response;
            
            // in allow debug mode just show standard ASP.NET error
            if (ShowErrorPageInDebugMode && filterContext.HttpContext.IsDebuggingEnabled)
            {
                base.OnException(filterContext);
                return;
            }

            var ex = filterContext.Exception.GetBaseException();            

            CallbackErrorResponseMessage resultMessage = new CallbackErrorResponseMessage(ex,AllowExceptionDetail);
            
            response.ContentType = "application/json";
            response.StatusCode = 500;
            var cbEx = ex as CallbackException;
        
            if (cbEx != null && cbEx.StatusCode > 0)
                response.StatusCode = ((CallbackException) ex).StatusCode;

            response.Write(JsonConvert.SerializeObject(resultMessage));
            
            filterContext.ExceptionHandled = true;            
            response.TrySkipIisCustomErrors = true;
        }

    }
}