using System.Security.Principal;

namespace Westwind.Web.WebApi
{
    /// <summary>
    /// Custom Identity that adds a password captured by basic authentication
    /// to allow for an auth filter to do custom authorization
    /// </summary>
    public class BasicAuthenticationIdentity : GenericIdentity
    {
        public BasicAuthenticationIdentity(string name, string password)
            : base(name, "Basic")
        {
            this.Password = password;
        }

        /// <summary>
        /// Basic Auth Password for custom authentication
        /// </summary>
        public string Password { get; set; }
    }
}
