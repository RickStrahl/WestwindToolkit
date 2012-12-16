
using System.Web;
namespace Westwind.Web.Mvc
{
    public class BaseViewModel
    {
        public ErrorDisplay ErrorDisplay = null;
        public UserState UserState = null;
        public string baseUrl = null;
        public string PageTitle = null;
        public PagingDetails Paging = null;

        public BaseViewModel()
        {
            if (HttpContext.Current != null)
                baseUrl = HttpContext.Current.Request.ApplicationPath;
        }
    }
}
