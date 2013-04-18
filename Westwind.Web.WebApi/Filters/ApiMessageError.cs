using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http.ModelBinding;

namespace Westwind.Web.WebApi
{
    /// <summary>
    /// Class that represents an error returned to
    /// the client caller. Can be explicitly returned or
    /// as part of the UnhandledExceptionFilter.
    /// </summary>
    public class ApiMessageError
    {
        /// <summary>
        /// The text message for the errors
        /// </summary>
        public string message { get; set; }

        /// <summary>
        /// Flag value that indicates to the client that this
        /// is an error response
        /// </summary>
        public bool isCallbackError { get; set; }

        /// <summary>
        /// An optional list of errors that can be set on the
        /// error object. Automatically set when passing in 
        /// a model dictionary with errors.
        /// </summary>
        public List<string> errors { get; set; }

        /// <summary>
        /// Default constructor creates empty error object
        /// </summary>
        public ApiMessageError()
            : this(string.Empty)
        {
        }

        /// <summary>
        /// Pass in a message string for the exception.
        /// </summary>
        /// <param name="errorMessage"></param>
        public ApiMessageError(string errorMessage)
        {
            isCallbackError = true;
            errors = new List<string>();
            message = errorMessage;
        }

        /// <summary>
        /// Pass in an exception and pick up the message.
        /// </summary>
        /// <param name="ex"></param>
        public ApiMessageError(Exception ex)
        {
            isCallbackError = true;
            errors = new List<string>();
            message = ex.Message;            
        }

        /// <summary>
        /// Pass in a modelState dictionary to create a list of
        /// binding errors from the API error message
        /// </summary>
        /// <param name="modelState"></param>
        public ApiMessageError(ModelStateDictionary modelState)
        {
            isCallbackError = true;
            errors = new List<string>();
            message = "Model is invalid.";

            // add errors into our client error model for client
            foreach (var modelItem in modelState)
            {
                var modelError = modelItem.Value.Errors.FirstOrDefault();
                if (!string.IsNullOrEmpty(modelError.ErrorMessage))
                    errors.Add(modelItem.Key + ": " +
                                ParseModelStateErrorMessage(modelError.ErrorMessage));
                else
                    errors.Add(modelItem.Key + ": " +
                                ParseModelStateErrorMessage(modelError.Exception.Message));
            }
        }

        /// <summary>
        /// Strips off anything after period - line number etc. info
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        string ParseModelStateErrorMessage(string msg)
        {
            int period = msg.IndexOf('.');
            if (period < 0 || period > msg.Length - 1)
                return msg;

            // strip off 
            return msg.Substring(0, period);
        }
    }
}
