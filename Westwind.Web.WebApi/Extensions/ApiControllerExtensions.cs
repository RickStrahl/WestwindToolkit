using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net;
using System.Net.Http;
using Westwind.Web.WebApi;


namespace System.Web.Http
{
    public static class ApiControllerExtensions
    {
        /// <summary>
        /// Throws a safe exception that results in a consistent error message being returned.
        /// </summary>
        /// <param name="statusCode"></param>
        /// <param name="message"></param>
        public static void ThrowHttpException(this ApiController controller, HttpStatusCode statusCode, string message)
        {            
            var resp = controller.Request.CreateResponse<ApiMessageError>(statusCode, new ApiMessageError(message));
            throw new HttpResponseException(resp);
        }

        /// <summary>
        /// Turn a regular exception into an ApiMessageError exception to display for the client.
        /// </summary>
        /// <param name="controller"></param>
        /// <param name="statusCode"></param>
        /// <param name="ex"></param>
        public static void ThrowHttpException(this ApiController controller, HttpStatusCode statusCode, Exception ex)
        {            
            var resp = controller.Request.CreateResponse<ApiMessageError>(statusCode, new ApiMessageError(ex));
            throw new HttpResponseException(resp);
        }

        /// <summary>
        /// Just returns an empty response with just the specified status code. No error message is sent.
        /// </summary>
        /// <param name="controller"></param>
        /// <param name="statusCode"></param>
        public static void ThrowHttpException(this ApiController controller, HttpStatusCode statusCode)
        {
            throw new HttpResponseException(statusCode);            
        }
    }
}
