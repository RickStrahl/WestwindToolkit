using System;
using System.Web;
using System.Reflection;
using Westwind.Web.JsonSerializers;
using Westwind.Utilities;
using System.Web.Routing;
using Westwind.Web.Controls.Properties;
using System.Net;

namespace Westwind.Web
{
    public enum CallbackMethodParameterType
    {
        Json,
        Xml
    }


    /// <summary>
    /// This class provides helper services to the CallbackProcessor classes with 
    /// the generic services that deal with method execution and parsing POST 
    /// parameters which should be independent of the specific implementation 
    /// (JSON, XML etc.).
    /// 
    /// Extracted here so other Callback Processors can be created more easily 
    /// later on, using other request formats (Xml etc.)
    /// </summary>
    internal class CallbackMethodProcessorHelper
    {
        private ICallbackMethodProcessor Processor = null;

        

        public CallbackMethodProcessorHelper(ICallbackMethodProcessor processor)
        {
            Processor = processor;
        }

        /// <summary>
        /// Executes the requested method. 
        /// to the proper types for execution.
        /// </summary>
        /// <param name="Method"></param>
        /// <param name="methodParameters">An array of the string json parameters to </param>
        /// <param name="target">The object to call the method on</param>
        /// <param name="parameters">An array of json Strings that make up the parameters for the method call. This value can be null in which case parms are parsed out of GET QueryString or POST values</param>
        /// <param name="callbackMethodAttribute">An optional instance of an CallbackAttribute that is set by this method</param>
        /// <returns>the result of the method execution</returns>
        internal object ExecuteMethod(string method, object target, string[] parameters, 
                                      CallbackMethodParameterType paramType,  
                                      ref CallbackMethodAttribute callbackMethodAttribute)
        {
            HttpRequest Request = HttpContext.Current.Request;
            HttpResponse Response = HttpContext.Current.Response;

            object Result = null;

            // Stores parsed parameters (from string JSON or QUeryString Values)
            object[] adjustedParms = null;

            Type PageType = target.GetType();
            MethodInfo MI = PageType.GetMethod(method, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (MI == null)
                throw new InvalidOperationException(Resources.InvalidServerMethod);

            object[] methods = MI.GetCustomAttributes(typeof(CallbackMethodAttribute), false);
            if (methods.Length < 1)
                throw new InvalidOperationException(Resources.ServerMethodIsNotAccessibleDueToMissing);

            if (callbackMethodAttribute != null)
                callbackMethodAttribute = methods[0] as CallbackMethodAttribute;

            // Check for supported HTTP Verbs
            if (callbackMethodAttribute.AllowedHttpVerbs != HttpVerbs.All && 
                !string.IsNullOrEmpty(Request.HttpMethod))
            {                                
                HttpVerbs val = HttpVerbs.None;
                Enum.TryParse<HttpVerbs>(Request.HttpMethod,out val);
                if (val == HttpVerbs.None || !callbackMethodAttribute.AllowedHttpVerbs.HasFlag(val))
                {
                    Response.StatusCode = (int) HttpStatusCode.MethodNotAllowed;
                    Response.StatusDescription = string.Format(Resources.NotAllowedMethodExpectsVerb,
                                                               callbackMethodAttribute.AllowedHttpVerbs);
                    throw new UnauthorizedAccessException(Response.StatusDescription);
                }
            }

            ParameterInfo[] parms = MI.GetParameters();

            JSONSerializer serializer = new JSONSerializer();

            RouteData routeData = null;
            if (target is CallbackHandler)
                routeData = ((CallbackHandler)target).RouteData;
          
            int parmCounter = 0;
            adjustedParms = new object[parms.Length];
            foreach (ParameterInfo parameter in parms)
            {
                // Retrieve parameters out of QueryString or POST buffer
                if (parameters == null)
                {
                    // look for parameters in the route
                    if (routeData != null)
                    {
                        string parmString = routeData.Values[parameter.Name] as string;
                        adjustedParms[parmCounter] = ReflectionUtils.StringToTypedValue(parmString, parameter.ParameterType);
                    }
                    // GET parameter are parsed as plain string values - no JSON encoding
                    else if (HttpContext.Current.Request.HttpMethod == "GET")
                    {
                        // Look up the parameter by name
                        string parmString = Request.QueryString[parameter.Name];
                        adjustedParms[parmCounter] = ReflectionUtils.StringToTypedValue(parmString, parameter.ParameterType);
                    }
                    // POST parameters are treated as methodParameters that are JSON encoded
                    else
                        if (paramType == CallbackMethodParameterType.Json)
                           //string newVariable = methodParameters.GetValue(parmCounter) as string;
                            adjustedParms[parmCounter] = serializer.Deserialize(Request.Params["parm" + (parmCounter + 1).ToString()], parameter.ParameterType);
                        else
                            adjustedParms[parmCounter] = SerializationUtils.DeSerializeObject(                                
                                Request.Params["parm" + (parmCounter + 1).ToString()], 
                                parameter.ParameterType);
                }
                else
                    if (paramType == CallbackMethodParameterType.Json)
                        adjustedParms[parmCounter] = serializer.Deserialize(parameters[parmCounter], parameter.ParameterType);
                    else
                        adjustedParms[parmCounter] = SerializationUtils.DeSerializeObject(parameters[parmCounter], parameter.ParameterType);

                parmCounter++;
            }

            Result = MI.Invoke(target, adjustedParms);

            return Result;
        }
    }
}
