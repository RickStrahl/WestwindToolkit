using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;
using Westwind.Web;
using Westwind.Web.JsonSerializers;

namespace WestwindToolkitMvcWeb
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            // Register Routes for CallbackHandler test service            
            CallbackHandlerRouteHandler.RegisterRoutes<WestwindToolkitMvcWeb.CallbackHandler>(RouteTable.Routes);
            JSONSerializer.DefaultJsonParserType = SupportedJsonParserTypes.JsonNet;

            //AreaRegistration.RegisterAllAreas();
            WebApiConfig.Register(GlobalConfiguration.Configuration);
            //BundleConfig.RegisterBundles(BundleTable.Bundles);

            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
        }
    }
}