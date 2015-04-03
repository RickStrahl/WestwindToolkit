using System;

namespace Westwind.Web
{
    /// <summary>
    /// Special return type that can be used to return messages to the
    /// caller directly rather than throwing an exception.
    /// </summary>    
    public class CallbackErrorResponseMessage : CallbackResponseMessage
    {
        public CallbackErrorResponseMessage() : base()
        {
            isError = true;
        }

        public CallbackErrorResponseMessage(string msg) : base(msg)
        {
            isError = true;
        }

        public CallbackErrorResponseMessage(CallbackException ex, bool allowExceptionDetail = false) 
            : this((Exception) ex, allowExceptionDetail)
        {
            isError = true;
            statusCode = ex.StatusCode;
        }
        public CallbackErrorResponseMessage(Exception ex, bool allowExceptionDetail = false)
        {
            isError = true;
            message = ex.Message;

            if (allowExceptionDetail)
            {
                detail = ex.StackTrace;
                source = ex.Source;
            }
        }                
    }
}