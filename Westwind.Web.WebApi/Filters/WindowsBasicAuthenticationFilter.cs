using System.Web.Http.Controllers;

using Westwind.Web.WebApi.Security;

namespace Westwind.Web.WebApi.Filters
{
    /// <summary>
    /// Basic Authentication using Windows 
    /// </summary>
    public class WindowsBasicAuthenticationFilter : BasicAuthenticationFilter
    {
        protected override bool OnAuthorizeUser(string username, string password, HttpActionContext actionContext)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                return false;

            var ret = (Win32Logon.Login(username, password) == LoginStatus.Success);
            return ret;
        }
    }
}
