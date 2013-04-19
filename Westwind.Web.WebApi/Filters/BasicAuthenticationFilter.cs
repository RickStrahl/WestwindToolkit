using System;
using System.Net;
using System.Net.Http;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace Westwind.Web.WebApi
{

/// <summary>
/// Generic Basic Authentication filter that checks for basic authentication
/// headers and challenges for authentication if no authentication is provided
/// Sets the Thread Principle with a GenericAuthenticationPrincipal.
/// 
/// You can override the OnAuthorize method for custom auth logic that
/// might be application specific.    
/// </summary>
/// <remarks>Always remember that Basic Authentication passes username and passwords
/// from client to server in plain text, so make sure SSL is used with basic auth
/// to encode the Authorization header on all requests (not just the login).
/// </remarks>
public class BasicAuthenticationFilter : AuthorizationFilterAttribute
{
    bool Active = true;


    public BasicAuthenticationFilter()
    { }

    /// <summary>
    /// Overriden constructor to allow explicit disabling of this
    /// filter's behavior. Pass false to disable (same as no filter
    /// but declarative)
    /// </summary>
    /// <param name="active"></param>
    public BasicAuthenticationFilter(bool active)
    {
        Active = active;
    }


    /// <summary>
    /// Override to Web API filter method to handle Basic Auth check
    /// </summary>
    /// <param name="actionContext"></param>
    public override void OnAuthorization(HttpActionContext actionContext)
    {
        if (Active)
        {
            var credentials = ParseAuthorizationHeader(actionContext);
            if (credentials == null)
            {
                Challenge(actionContext);
                return;
            }


            if (!OnAuthorizeUser(credentials.Username, credentials.Password, actionContext))
            {
                Challenge(actionContext);
                return;
            }
        }

        base.OnAuthorization(actionContext);
    }

    /// <summary>
    /// Base implementation for user authentication - you probably will
    /// want to override this method for application specific logic.
    /// 
    /// The base implementation merely checks for username and password
    /// present and set the Thread principal.
    /// 
    /// Override this method if you want to customize Authentication
    /// and store user data as needed in a Thread Principle or other
    /// Request specific storage.
    /// </summary>
    /// <param name="username"></param>
    /// <param name="password"></param>
    /// <param name="actionContext"></param>
    /// <returns></returns>
    protected virtual bool OnAuthorizeUser(string username, string password, HttpActionContext actionContext)
    {
        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            return false;

        var principal = new GenericPrincipal(new GenericIdentity(username, "Basic"), null);
            
        Thread.CurrentPrincipal = principal;

        // inside of ASP.NET this is requried!
        //if (HttpContext.Current != null)
        //    HttpContext.Current.User = principal;

        return true;
    }

    /// <summary>
    /// Parses the Authorization header and creates user credentials
    /// </summary>
    /// <param name="actionContext"></param>
    protected virtual BasicAuthCredentials ParseAuthorizationHeader(HttpActionContext actionContext)
    {
        string authHeader = null;
        var auth = actionContext.Request.Headers.Authorization;
        if (auth != null && auth.Scheme == "Basic")
            authHeader = auth.Parameter;

        if (string.IsNullOrEmpty(authHeader))
            return null;

        authHeader = Encoding.Default.GetString(Convert.FromBase64String(authHeader));

        var tokens = authHeader.Split(':');
        if (tokens.Length < 2)
            return null;

        return new BasicAuthCredentials()
        {
            Username = tokens[0],
            Password = tokens[1]
        };
    }


    /// <summary>
    /// Send the Authentication Challenge request
    /// </summary>
    /// <param name="message"></param>
    /// <param name="actionContext"></param>
    void Challenge(HttpActionContext actionContext)
    {
        var host = actionContext.Request.RequestUri.DnsSafeHost;
        actionContext.Response = actionContext.Request.CreateResponse(HttpStatusCode.Unauthorized);
        actionContext.Response.Headers.Add("WWW-Authenticate", string.Format("Basic realm=\"{0}\"", host));
    }

}


public class BasicAuthCredentials
{
    public string Username { get; set; }
    public string Password { get; set; }
}
}
