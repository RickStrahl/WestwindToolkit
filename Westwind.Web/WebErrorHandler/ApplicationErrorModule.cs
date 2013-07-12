using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Westwind.Web
{
    /// <summary>
    /// Application Error Module that can be used to handle
    /// errors.
    /// 
    /// </summary>
    public abstract class ApplicationErrorModule : IHttpModule
    {
        /// <summary>
        /// Determines the error handling mode - 
        /// if you want to customize this value override it
        /// in your application's startup code or from a configuration
        /// value by overriding the Init().
        /// </summary>
        public static ErrorHandlingModes ErrorHandlingMode { get; set; }
       
        public virtual void Init(HttpApplication context)
        {
            OnInitializeErrorHandlingMode();
            context.Error += context_Error;
        }

        void context_Error(object sender, EventArgs e)
        {
            var handler = new WebErrorHandler();

            handler.LogError += OnLogError;
            handler.DisplayError += OnDisplayError;

            handler.HandleError(ErrorHandlingMode);

            handler.LogError -= OnLogError;
            handler.DisplayError -= OnDisplayError;
            
            handler = null;
        }


        /// <summary>
        ///  Override this method to set the error handling mode
        /// which is configured when the appplication starts.
        /// </summary>
        protected virtual void OnInitializeErrorHandlingMode()
        {
            
        }

        /// <summary>
        /// Override this method to handle displaying of error information.
        /// You can write to the HTTP output stream. If using MVC you can
        /// use the Westwind.Web.ViewRenderer class to easily display an
        /// MVC view.
        /// </summary>
        /// <param name="errorHandler"></param>
        /// <param name="model"></param>
        protected virtual void  OnDisplayError(WebErrorHandler errorHandler, ErrorViewModel model)
        {
            
        }

        /// <summary>
        /// Override this method to handle logging of errors. Gets passed
        /// the WebErrorHandler instance that is fully parsed and filled
        /// with the error and Http request data.
        /// </summary>
        /// <param name="errorHandler"></param>
        /// <param name="ex"></param>
        protected virtual void OnLogError(WebErrorHandler errorHandler, Exception ex)
        {
               
        }

        public void Dispose()
        {
            
        }
    }
}
