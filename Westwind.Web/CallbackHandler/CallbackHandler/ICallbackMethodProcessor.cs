using System;
using Westwind.Web.JsonSerializers;

namespace Westwind.Web
{
    /// <summary>
    /// Inteface that defines the Callback operations for handling method callbacks
    /// on the server.
    /// 
    /// This interface serves as an abstraction for potentially different implementations
    /// that use XML instead of JSON.
    /// </summary>
    public interface ICallbackMethodProcessor
    {
        JsonDateEncodingModes JsonDateEncoding { get; set; }

        /// <summary>
        /// Generic method that handles processing a Callback request by routing to
        /// a method in a provided target object.
        /// 
        /// </summary>
        /// <param name="target">The target object that is to be called. If null this is used</param>
        void ProcessCallbackMethodCall(object target);

        /// <summary>
        /// Returns an error response to the client from a callback. Code
        /// should exit after this call.
        /// </summary>
        /// <param name="ErrorMessage"></param>
        void WriteErrorResponse(string ErrorMessage, string stackTrace);
    }
}
