using System;
using System.Web.Http.Filters;
using System.Net;

using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Formatting;
using System.Web.Http;


namespace Westwind.Web.WebApi
{
    public class UnhandledExceptionFilter : ExceptionFilterAttribute
    {
        public override void OnException(HttpActionExecutedContext context)
        {
            HttpStatusCode status = HttpStatusCode.InternalServerError;

            var exType = context.Exception.GetType();

            if (exType == typeof(UnauthorizedAccessException))
                status = HttpStatusCode.Unauthorized;
            else if (exType == typeof(ArgumentException))
                status = HttpStatusCode.NotFound;

            var apiError = new ApiMessageError() 
            { message = context.Exception.Message };

            // create a new response and attach our ApiError object
            // which now gets returned on ANY exception result
            var errorResponse = context.Request.CreateResponse<ApiMessageError>(status, apiError);
            //var errorResponse = context.Request.CreateResponse(HttpStatusCode.BadRequest, 
            //                                context.Exception.GetBaseException().Message);
            context.Response = errorResponse;

            base.OnException(context);
        }
    }
}