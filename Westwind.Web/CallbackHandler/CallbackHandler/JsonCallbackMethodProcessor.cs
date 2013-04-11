#region License
/*
 **************************************************************
 *  Author: Rick Strahl 
 *          © West Wind Technologies, 2008 2011
 *          http://www.west-wind.com/
 * 
 * Created: 09/04/2008
 *
 * Permission is hereby granted, free of charge, to any person
 * obtaining a copy of this software and associated documentation
 * files (the "Software"), to deal in the Software without
 * restriction, including without limitation the rights to use,
 * copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the
 * Software is furnished to do so, subject to the following
 * conditions:
 * 
 * The above copyright notice and this permission notice shall be
 * included in all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
 * EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
 * OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
 * NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
 * HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
 * WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
 * FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
 * OTHER DEALINGS IN THE SOFTWARE.
 **************************************************************  
*/
#endregion

using System;
using System.Web;
using System.IO;

using Westwind.Web.JsonSerializers;
using System.Collections.Generic;
using Westwind.Utilities;
using System.Drawing;
using System.Drawing.Imaging;

namespace Westwind.Web
{

    /// <summary>
    /// This is the core implementation of the JSON callback method handler that 
    /// picks up POST data from the request and uses it to call the actual callback
    ///  method on the specified object and return the results back as JSON.
    /// 
    /// This processor is generic and can be used easily from anywhere that needs 
    /// to feed back JSON data from a method callback, simply calling the 
    /// ProcessCallbackMethod() with an object that contains methods that are 
    /// marked up with the [CallbackMethod] attribute.
    /// 
    /// For example, wwCallbackHandler simply forwards all processing like this:
    /// 
    /// &lt;&lt;code lang="C#"&gt;&gt;public void ProcessRequest(HttpContext 
    /// context)
    /// {
    ///     // Pass off to the worker Callback Processor
    ///     ICallbackMethodProcessor processor = new JsonCallbackMethodProcessor();
    /// 
    ///     // Process the inbound request and execute it on this
    ///     // Http Handler's methods
    ///     processor.ProcessCallbackMethodCall(this);
    /// }&lt;&lt;/code&gt;&gt;
    /// 
    /// This processor is expected to execute in an environment where 
    /// HttpContext.Current is available and where POST data is available to 
    /// describe the incoming parameter data and method to call.
    /// </summary>
    public class JsonCallbackMethodProcessor : ICallbackMethodProcessor
    {
        
        public JsonDateEncodingModes JsonDateEncoding
        {
          get { return _JsonDateEncoding; }
          set { _JsonDateEncoding = value; }
        }
        private JsonDateEncodingModes _JsonDateEncoding = JsonDateEncodingModes.ISO;


        /// <summary>
        /// JSONP method parameter value if provided
        /// </summary>
        protected internal string JsonPMethod
        {
            get { return _JsonPMethod; }
            set { _JsonPMethod = value; }
        }
        private string _JsonPMethod = null;



        /// <summary>
        /// Generic method that handles processing a Callback request by routing to
        /// a method in a provided target object.
        /// 
        /// </summary>
        /// <param name="target">The target object that is to be called. If null this is used</param>
        public void ProcessCallbackMethodCall(object target, string methodToCall)
        {
            if (target == null)
                target = this;

            HttpRequest Request = HttpContext.Current.Request;
            HttpResponse Response = HttpContext.Current.Response;
            Response.Charset = null;

            // check for Route Data method name
            if (target is CallbackHandler)
            {
                var routeData = ((CallbackHandler)target).RouteData;                
                if (routeData != null)
                    methodToCall = ((CallbackHandlerRouteHandler)routeData.RouteHandler).MethodName;
            }

            CallbackMethodProcessorHelper helper = new CallbackMethodProcessorHelper(this);
            
            List<string> parameterList = null;
            
            string contentType = Request.ContentType.ToLower();

            // Allow for a single JSON object to be POSTed rather than POST variables
            if (contentType.StartsWith(WebResources.STR_JavaScriptContentType) ||
                contentType.StartsWith(WebResources.STR_JsonContentType))
            {
                if (string.IsNullOrEmpty(methodToCall))
                    methodToCall = Request.Params["Method"];

                if (string.IsNullOrEmpty(methodToCall))
                {
                    WriteErrorResponse("No method to call specified.",null);
                    return;
                }

                // Pass a Parameter List with our JSON encoded parameters
                parameterList = new List<string>();

                if (Request.ContentLength > 0L)
                {
                    // Pick up single unencoded JSON parameter
                    StreamReader sr = new StreamReader(Request.InputStream);
                    string singleParm = sr.ReadToEnd();
                    sr.Close();
                    sr.Dispose();

                    if (!string.IsNullOrEmpty(singleParm))
                        parameterList.Add(singleParm);
                }
            }
            // Post AjaxMethodCallback style interface            
            else if (contentType.StartsWith(WebResources.STR_UrlEncodedContentType) && Request.Params["CallbackMethod"] != null)
                // Only pick up the method name - Parameters are parsed out of POST buffer during method calling
                methodToCall = Request.Params["CallbackMethod"];                
            else
            {
                JsonPMethod = Request.QueryString["jsonp"] ?? Request.QueryString["callback"];

                // Check for querystring method parameterList
                if (string.IsNullOrEmpty(methodToCall)) 
                    methodToCall = Request.QueryString["Method"];               

                // No method - no can do
                if (string.IsNullOrEmpty(methodToCall))
                {
                    WriteErrorResponse("No method to call specified.",null);
                    return;
                }
            }

            // Explicitly set the content type here - set here so the method 
            // can override it if it so desires
            Response.ContentType = WebResources.STR_JsonContentType;
            
            object result = null;
            string stringResult = null;
            CallbackMethodAttribute attr = new CallbackMethodAttribute();
            try
            {
                if (parameterList != null)
                    // use the supplied parameter list
                    result = helper.ExecuteMethod(methodToCall,target, parameterList.ToArray(),
                                        CallbackMethodParameterType.Json,ref attr);
                else
                    // grab the info out of QueryString Values or POST buffer during parameter parsing 
                    // for optimization
                    result = helper.ExecuteMethod(methodToCall, target, null, 
                                                  CallbackMethodParameterType.Json, ref attr);
            }
            catch (Exception ex)
            {
                Exception activeException = DebugUtils.GetInnerMostException(ex);
                WriteErrorResponse(activeException.Message,
                                  ( HttpContext.Current.IsDebuggingEnabled ? ex.StackTrace : null ) );
                return;
            }

            // Special return type handling: Stream, Bitmap, byte[] and raw string results
            // are converted and returned directly
            HandleSpecialReturnTypes(result, attr, Request, Response);
            
            // Standard json formatting            
            try
            {
                JSONSerializer Serializer = new JSONSerializer();
                Serializer.DateSerializationMode = JsonDateEncoding;               

                // In debug mode show nicely formatted JSON 
                // In release normal packed JSON is used
                if (HttpContext.Current.IsDebuggingEnabled)
                    Serializer.FormatJsonOutput = true;

                stringResult = Serializer.Serialize(result);
            }
            catch (Exception ex)
            {
                WriteErrorResponse(ex.Message, HttpContext.Current.IsDebuggingEnabled ? ex.StackTrace : null);
                return;
            }
            
           
            if (!string.IsNullOrEmpty(JsonPMethod))            
                stringResult = JsonPMethod + "( " + stringResult + " );";
            
            Response.Write(stringResult);
            Response.End();
        }

        /// <summary>
        /// This method handles special return types from callback methods
        /// by examining the return type or the ResultFormat query string.
        /// 
        /// Checks are performed for:
        /// Stream, Bitmap, byte[] and raw string output
        /// </summary>
        /// <param name="Result"></param>
        /// <param name="callbackAttribute"></param>
        /// <param name="Request"></param>
        /// <param name="Response"></param>
        void HandleSpecialReturnTypes(object Result, CallbackMethodAttribute callbackAttribute, 
                                      HttpRequest Request, HttpResponse Response)
        {
            string format = (Request.Params["Format"] ?? "").ToLower();
            //string format = (Request.Params["ResultFormat"] ?? "").ToLower();

            // [Callback(ContentType="text/html"] override
            if (!string.IsNullOrEmpty(callbackAttribute.ContentType))
                Response.ContentType = callbackAttribute.ContentType;
           

            // Stream data is just sent back RAW as is. Method code should set ContentType
            if (Result is Stream)
            {
                Stream stream = Result as Stream;
                FileUtils.CopyStream(stream, Response.OutputStream, 4092);
                stream.Close();

                Response.End();                
            }
            else if (Result is byte[])
            {
                Response.BinaryWrite(Result as byte[]);                
                Response.End();                
            }
            else if (Result is Bitmap)
            {
                Bitmap bm = Result as Bitmap;
                ImageFormat imageFormat = ImageFormat.Png;                

                // if no content type was explicitly specified use bitmap's internal format (loaded from disk or default)
                if (string.IsNullOrEmpty(callbackAttribute.ContentType))
                    Response.ContentType = WebUtils.ImageFormatToContentType(bm.RawFormat);
                else
                    imageFormat = WebUtils.ImageFormatFromContentType(callbackAttribute.ContentType);

                if (imageFormat == ImageFormat.Png)
                {
                    // pngs are special and require reloading
                    // or else they won't write to OutputStream
                    bm = new Bitmap(Result as Bitmap);
                    MemoryStream ms = new MemoryStream();
                    bm.Save(ms, imageFormat);
                    ms.WriteTo(Response.OutputStream);
                    ms.Dispose();
                }
                else
                    bm.Save(Response.OutputStream, imageFormat);

                bm.Dispose();
                ((Bitmap)Result).Dispose();

                Response.End();             
            }
            // Raw string result option eith via querystring or CallbackMethod Attribute
            else if ((format == "string" || callbackAttribute.ReturnAsRawString) && Result.GetType() == typeof(string))
            {
                if (!string.IsNullOrEmpty(callbackAttribute.ContentType))
                    Response.ContentType = callbackAttribute.ContentType;

                Response.Write(Result as string);
                Response.End();
            }
        }


        /// <summary>
        /// Generic method that handles processing a Callback request by routing to
        /// a method in a provided target object.
        /// 
        /// This version doesn't pass in the method name but retrieves it from the
        /// POST data or query string.
        /// </summary>
        /// <param name="target"></param>
        public void ProcessCallbackMethodCall(object target)
        {
            ProcessCallbackMethodCall(target, null);
        }

        /// <summary>
        /// Returns an error response to the client from a callback. Code
        /// should exit after this call.
        /// </summary>
        /// <param name="ErrorMessage"></param>
        public void WriteErrorResponse(string errorMessage, string stackTrace)
        {
            CallbackException Error = new CallbackException();
            Error.message = errorMessage;
            Error.isCallbackError = true;
            Error.stackTrace = stackTrace;

            JSONSerializer Serializer = new JSONSerializer( SupportedJsonParserTypes.JavaScriptSerializer);            
            string result = Serializer.Serialize(Error);

            if (!string.IsNullOrEmpty(JsonPMethod))
                result = JsonPMethod + "( " + result + " );";

            HttpResponse Response = HttpContext.Current.Response;
            Response.ContentType = WebResources.STR_JsonContentType;

            Response.TrySkipIisCustomErrors = true;
            
            // override status code but only if it wasn't set already
            if (Response.StatusCode == 200)
                Response.StatusCode = 500;
            
            Response.Write(result);
            
            Response.End();
        }
    }
}
