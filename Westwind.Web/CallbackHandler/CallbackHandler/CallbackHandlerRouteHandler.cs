using System;
using System.Web;
using System.Web.Routing;
using System.Reflection;

namespace Westwind.Web
{
    /// <summary>
    /// Route handler that can create instances of CallbackHandler derived
    /// callback classes. The route handler tracks the method name and
    /// creates an instance of the service in a predictable manner
    /// </summary>
    /// <typeparam name="TCallbackHandler">CallbackHandler type</typeparam>
    public class CallbackHandlerRouteHandler : IRouteHandler
    {
        /// <summary>
        /// Method name that is to be called on this route.
        /// Set by the automatically generated RegisterRoutes 
        /// invokatio.n
        /// </summary>
        public string MethodName { get; set; }

        /// <summary>
        /// The type of the handler we're going to instantiate.
        /// Needed so we can semi-generically instantiate the
        /// handler and call the method on it.
        /// </summary>
        public Type CallbackHandlerType { get; set; }


        private Guid id = Guid.NewGuid();

        /// <summary>
        /// Constructor to pass in the two required components we
        /// need to create an instance of our handler. 
        /// </summary>
        /// <param name="methodName"></param>
        /// <param name="callbackHandlerType"></param>
        public CallbackHandlerRouteHandler(string methodName, Type callbackHandlerType)
        {
            MethodName = methodName;
            CallbackHandlerType = callbackHandlerType;
        }

        /// <summary>
        /// Retrieves an Http Handler based on the type specified in the constructor
        /// </summary>
        /// <param name="requestContext"></param>
        /// <returns></returns>
        IHttpHandler IRouteHandler.GetHttpHandler(RequestContext requestContext)
        {
            IHttpHandler handler = Activator.CreateInstance(CallbackHandlerType) as IHttpHandler;

            // If we're dealing with a Callback Handler
            // pass the RouteData for this route to the Handler
            if (handler is CallbackHandler)
                ((CallbackHandler)handler).RouteData = requestContext.RouteData;

            return handler;
        }

        /// <summary>
        /// Generic method to register all routes from a CallbackHandler
        /// that have RouteUrls defined on the [CallbackMethod] attribute
        /// </summary>
        /// <typeparam name="TCallbackHandler"></typeparam>
        /// <param name="routes"></param>
        public static void RegisterRoutes<TCallbackHandler>(RouteCollection routes)
        {
            // find all methods
            var methods = typeof(TCallbackHandler).GetMethods(BindingFlags.Instance | BindingFlags.Public);
            foreach (var method in methods)
            {
                var attrs = method.GetCustomAttributes(typeof(CallbackMethodAttribute), false);
                if (attrs.Length < 1)
                    continue;

                CallbackMethodAttribute attr = attrs[0] as CallbackMethodAttribute;
                if (string.IsNullOrEmpty(attr.RouteUrl))
                    continue;

                // Add the route
                routes.Add(method.Name,
                           new Route(attr.RouteUrl, new CallbackHandlerRouteHandler(method.Name, typeof(TCallbackHandler))));

            }

        }
    }
}
