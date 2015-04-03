using System;

namespace Westwind.Web
{
    /// <summary>
    /// Special exception type used to indicate that an exception was
    /// fired on the server. This object is JSON serialized and the
    /// client can check for Result.IsError to see if a 
    /// a failure occured on the server.
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
