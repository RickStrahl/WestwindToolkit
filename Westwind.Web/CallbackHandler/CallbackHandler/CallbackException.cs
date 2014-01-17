using System;

namespace Westwind.Web
{
    /// <summary>
    /// Special return type used to indicate that an exception was
    /// fired on the server. This object is JSON serialized and the
    /// client can check for Result.IsCallbackError to see if a 
    /// a failure occured on the server.
    /// </summary>    
    public class CallbackException : Exception
    {
        public CallbackException()
        {
            message = string.Empty;
            stackTrace = string.Empty;            
        }
        public bool isCallbackError {get; set; }
        public string message { get; set; }
        public string stackTrace  { get; set; }
    }

    /// <summary>
    /// Special return type that can be used to return messages to the
    /// caller directly rather than throwing an exception.
    /// </summary>    
    public class CallbackMessage
    {
        public CallbackMessage()
        {
            message = string.Empty;
        }

        public bool isError {get; set; }
        public string message {get; set;}
        public object resultData {get; set;}
    }
}
