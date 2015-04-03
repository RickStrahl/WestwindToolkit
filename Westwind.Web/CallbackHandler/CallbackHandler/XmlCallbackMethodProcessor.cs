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
using System.Collections;
using System.Reflection;
using System.IO;

using Westwind.Web.JsonSerializers;
using System.Collections.Generic;
using Westwind.Utilities;
using System.Drawing;
using System.Drawing.Imaging;
using Westwind.Web.Properties;
using System.Xml.Serialization;

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
    public class XmlCallbackMethodProcessor : ICallbackMethodProcessor
    {
        /// <summary>
        /// Implemented only for compatibility
        /// </summary>
         public JsonDateEncodingModes JsonDateEncoding { get; set; }

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

            CallbackMethodProcessorHelper helper = new CallbackMethodProcessorHelper(this);
            
            List<string> ParameterList = null;
            
            string contentType = Request.ContentType.ToLower();


            // check for Route Data method name
            if (string.IsNullOrEmpty(methodToCall) && target is CallbackHandler)
            {
                CallbackHandler chandler = target as CallbackHandler;
                if (chandler.RouteData != null)
                    methodToCall = ((CallbackHandlerRouteHandler)chandler.RouteData.RouteHandler).MethodName;
            }

            // Allow for a single XML object to be POSTed rather than POST variables
            if ( contentType.StartsWith(WebResources.STR_XmlContentType) )                
            {
                if (string.IsNullOrEmpty(methodToCall))
                    methodToCall = Request.Params["Method"];
             
                if (string.IsNullOrEmpty(methodToCall))
                {
                    WriteErrorResponse("No method to call specified.",null);
                    return;
                }

                // Pass a Parameter List with our JSON encoded parameters
                ParameterList = new List<string>();

                if (Request.ContentLength > 0L)
                {
                    // Pick up single unencoded JSON parameter
                    string singleParm = WebUtils.FormBufferToString();
                    
                    if (!string.IsNullOrEmpty(singleParm))
                        ParameterList.Add(singleParm);
                }
            }
            // Post AjaxMethodCallback style interface            
            else if (contentType.StartsWith(WebResources.STR_UrlEncodedContentType) && Request.Params["CallbackMethod"] != null)
                // Only pick up the method name - Parameters are parsed out of POST buffer during method calling
                methodToCall = Request.Params["CallbackMethod"];                
            else
            {
                if (string.IsNullOrEmpty(methodToCall)) 
                    methodToCall = Request.QueryString["Method"];

                if (string.IsNullOrEmpty(methodToCall))
                {
                    WriteErrorResponse("No method to call specified.",null);
                    return;
                }
            }
            
            object Result = null;
            string StringResult = null;
            CallbackMethodAttribute attr = new CallbackMethodAttribute();
            try
            {
                if (ParameterList != null)
                    // use the supplied parameter list
                    Result = helper.ExecuteMethod(methodToCall,target, ParameterList.ToArray(),
                                                  CallbackMethodParameterType.Xml, ref attr);
                else
                    // grab the info out of QueryString Values or POST buffer during parameter parsing 
                    // for optimization
                    Result = helper.ExecuteMethod(methodToCall, target, null, 
                                                  CallbackMethodParameterType.Xml,ref attr);
            }
            catch (Exception ex)
            {
                Exception ActiveException = null;
                if (ex.InnerException != null)
                    ActiveException = ex.InnerException;
                else
                    ActiveException = ex;

                WriteErrorResponse(ActiveException.Message,
                                  ( HttpContext.Current.IsDebuggingEnabled ? ex.StackTrace : null ) );
                return;
            }

            // Special return type handling: Stream, Bitmap, byte[] and raw string results
            // are converted and returned directly
            HandleSpecialReturnTypes(Result, attr, Request, Response);
            
            // Standard json formatting            
            try
            {
                SerializationUtils.SerializeObject(Result, out StringResult);
            }
            catch (Exception ex)
            {
                WriteErrorResponse(ex.Message, HttpContext.Current.IsDebuggingEnabled ? ex.StackTrace : null);
                return;
            }
            

            // Explicitly set the content type here
            Response.ContentType = WebResources.STR_XmlContentType;
                        
            Response.Write(StringResult);
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
        void HandleSpecialReturnTypes(object Result, CallbackMethodAttribute callbackAttribute, HttpRequest Request, HttpResponse Response)
        {
            string format = (Request.Params["ResultFormat"] ?? "").ToLower();

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
        public void WriteErrorResponse(string errorMessage, string stackTrace, int statusCode = 500)
        {
            CallbackErrorResponseMessage Error = new CallbackErrorResponseMessage(errorMessage);            
            Error.detail = stackTrace;
            Error.statusCode = statusCode;
            
            JSONSerializer Serializer = new JSONSerializer();
            string result = null;
            SerializationUtils.SerializeObject(Error, out result);

            HttpResponse Response = HttpContext.Current.Response;
            Response.ContentType = WebResources.STR_XmlContentType;

            Response.TrySkipIisCustomErrors = true;
            
            if (Response.StatusCode == 200)
                Response.StatusCode = statusCode;

            Response.Write(result);
            //HttpContext.Current.ApplicationInstance.CompleteRequest();
            Response.End();
        }
    }
}
