using System.Web.Mvc;
using System.Web.Routing;

namespace AlbumViewerAngular
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {            
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);


            AlbumViewerBusiness.App.Configuration.ApplicationRootPath = Context.ApplicationInstance.Server.MapPath("~/");
        }
    }
}
