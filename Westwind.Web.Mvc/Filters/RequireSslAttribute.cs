using System;
using System.Configuration;
using System.Reflection;
using System.Web.Mvc;

namespace Westwind.Web.Mvc
{

    /// <summary>
    /// Allows for dynamically assigning the RequireSsl property at runtime
    /// either with an explicit boolean constant, a configuration setting,
    /// or a Reflection based 'delegate' 
    /// </summary>
    public class RequireSslAttribute : RequireHttpsAttribute
    {
        public bool RequireSsl { get; set; }


        /// <summary>
        /// Default constructor forces SSL required
        /// </summary>
        public RequireSslAttribute()
        {
            // Assign from App specific configuration object
            RequireSsl = true;
        }

        /// <summary>
        /// Injectable constructor that takes an IRequireSsl interface
        /// that can be injected.
        /// </summary>
        /// <param name="requireSsl">IRequireSsl implementation - injectable through your IOC container</param>
        public RequireSslAttribute(IRequireSsl requireSsl)
        {
            RequireSsl = requireSsl.RequireSsl;
        }

        /// <summary>
        /// Allows assignment of the SSL status via parameter
        /// </summary>
        /// <param name="requireSsl"></param>
        public RequireSslAttribute(bool requireSsl)
        {
            RequireSsl = requireSsl;
        }

        /// <summary>
        /// Allows invoking a method at runtime to check for a 
        /// value dynamically.
        /// 
        /// Note: The method called must be a static method
        /// </summary>
        /// <param name="typeName">Fully qualified type name on which the method to call exists</param>
        /// <param name="method">Static method on this type to invoke with no parameters</param>
        public RequireSslAttribute(Type type, string method)
        {
            var mi = type.GetMethod(method, BindingFlags.Static | BindingFlags.InvokeMethod | BindingFlags.Public);
            RequireSsl = (bool)mi.Invoke(type, null);
        }

        /// <summary>
        /// Looks for an appSetting key you specify and if it exists
        /// and is set to true or 1 which forces SSL.
        /// </summary>
        /// <param name="appSettingsKey"></param>
        public RequireSslAttribute(string appSettingsKey)
        {
            string key = ConfigurationManager.AppSettings[appSettingsKey] as string;
            RequireSsl = false;
            if (!string.IsNullOrEmpty(key))
            {
                key = key.ToLower();
                if (key == "true" || key == "1")
                    RequireSsl = true;
            }
        }


        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            if (filterContext != null &&
                RequireSsl &&
                !filterContext.HttpContext.Request.IsSecureConnection)
            {
                HandleNonHttpsRequest(filterContext);
            }
        }
    }

    public interface IRequireSsl
    {
        bool RequireSsl { get; }
    }
}