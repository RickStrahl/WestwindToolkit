using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;
using Westwind.Web;

namespace AlbumViewerAngular
{
    public class CallbackExceptionHandlerAttribute : HandleErrorAttribute
    {
        public CallbackExceptionHandlerAttribute()
        { }

        public CallbackExceptionHandlerAttribute(bool allowDebugMode)
        {
            AllowDebugMode = allowDebugMode;
        }

        /// <summary>
        /// When true returns standard ASP.NET yellow screen of death
        /// when DebugMode is enabled.
        /// </summary>
        public bool AllowDebugMode { get; set; }

        public override void OnException(ExceptionContext filterContext)
        {
            if (AllowDebugMode && HttpContext.Current.IsDebuggingEnabled)
            {
                base.OnException(filterContext);
                return;
            }

            var ex = filterContext.Exception;
            var callbackException = ex as CallbackException;
            
            var response = filterContext.HttpContext.Response;
            
            if(callbackException != null)
                response.StatusCode = callbackException.statusCode;

            response.ContentType = "application/json";
            response.Write(JsonConvert.SerializeObject(ex));
            
            filterContext.ExceptionHandled = true;            
            response.TrySkipIisCustomErrors = true;
        }

    }
}