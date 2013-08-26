using System;
using System.Web;

namespace Westwind.Web
{
    /// <summary>
    /// Application Error Module that can be used to handle
    /// errors. Uses WebErrorHandler to collect error and
    /// request information in a single place, and abstracts
    /// the error handling to OnLogError and OnDisplayError
    /// hooks to manage only the parts that we are typically
    /// interested in in error processing.
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
       
        /// <summary>
        /// Hook up Application.Error event
        /// </summary>
        /// <param name="context"></param>
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
        /// <param name="errorHandler">Contains error and request information</param>
        /// <param name="model">
        /// Model that contains a few simple properties like title and message
        /// as well as an instance of the errorHandler object passed in to allow
        /// for error pages that can provide a wealth of detail if desired.
        /// </param>
        protected virtual void  OnDisplayError(WebErrorHandler errorHandler, ErrorViewModel model)
        {            
        }

        /// <summary>
        /// Override this method to handle logging of errors. Gets passed
        /// the WebErrorHandler instance that is fully parsed and filled
        /// with the error and Http request data.
        /// </summary>
        /// <param name="errorHandler">Contains formatted error and request information</param>
        /// <param name="ex">The original exception that caused the error</param>
        protected virtual void OnLogError(WebErrorHandler errorHandler, Exception ex)
        {               
        }

        public void Dispose()
        {            
        }
    }
}
