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

        public CallbackException(string message, int statusCode = 500) : base(message)
        {
            if (message == null)
                message = string.Empty;

        }

        public bool IsError {get; set; }
        public int StatusCode { get; set; }
    }
}
