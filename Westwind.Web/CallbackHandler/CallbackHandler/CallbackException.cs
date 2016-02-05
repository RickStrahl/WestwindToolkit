using System;

namespace Westwind.Web
{
    /// <summary>
    /// Special exception type used to indicate that an exception was
    /// fired on the server for a JSON callback. This exception is captured
    /// by the CallbackHandler and serialized into CallbackErrorResponse
    /// to the client in a consistent format.
    /// 
    /// Also used in various MVC related AJAX error handlers 
    /// (BaseController and JsonCallback
    /// </summary>    
    public class CallbackException : Exception
    {
        public CallbackException()
        {
            IsError = true;           
            StatusCode = 500;
        }

        public CallbackException(string message, Exception innerException) : base(message, innerException)
        {            
        }

        public CallbackException(string message, int statusCode = 500) : base(message)
        {            
            StatusCode = statusCode;
        }

        public bool IsError {get; set; }

        /// <summary>
        /// Additional Error Detail to assign to this error instance
        /// </summary>
        public string ErrorDetail { get; set; }

        public int StatusCode { get; set; }
    }


    /// <summary>
    /// Mimicks the CallbackException class except this is not based on an Exception.
    /// Ideal for passing back as an error result from a failed serialization request
    /// in an API or other service interface.
    /// </summary>
    public class CallbackError 
    {
        public CallbackError()
        {
            IsError = true;
            StatusCode = 500;
        }

        public CallbackError(string message, int statusCode = 500) 
        {
            if (message == null)
                message = string.Empty;

            IsError = true;
            Message = message;
            StatusCode = statusCode;
        }

        public bool IsError { get; set; }

        public int StatusCode { get; set; }

        public string Message { get; set; }

        /// <summary>
        /// Additional Error Detail to assign to this error instance
        /// </summary>
        public string ErrorDetail { get; set; }


        public string StackTrace { get; set; }
    }
}
