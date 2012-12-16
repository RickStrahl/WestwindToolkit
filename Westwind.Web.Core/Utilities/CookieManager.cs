using System;
using System.IO;
using System.Text;

using System.Web;

namespace Westwind.Web
{
	/// <summary>
	/// A generic Web Cookie handler class that can be used for a single 'UserId' in an
	/// application. This class manages all aspects of retrieving and setting of a cookie
	/// consistently. Typically all you'll need to do is call the GetId() method which 
	/// both returns existing cookies and ensures that the cookie gets set.
	/// 
	/// All methods of this class are static which is the reason why only a single Cookie
    /// can be managed at a time. The idea is that you can use this single cookie as an
    /// application global Cookie to track a user and then retrieve additional storage 
    /// information from other locations (like a database or session).
	/// </summary>
	public class StaticCookieManager 
	{
		public static string CookieName = "WestWindUser";
		public static int CookieTimeoutInMonths = 24;

		/// <summary>
		/// Writes the cookie into the response stream with the value passed. The value
		///  is always the UserId.
		/// <seealso>Class WebStoreCookie</seealso>
		/// </summary>
		/// <param name="String Value">
		/// Writes a value into the specified cookie.
		/// </param>
		/// <returns>Void</returns>
		public static void WriteCookie(string Value, bool NonPersistent) 
		{
			HttpCookie Cookie = new HttpCookie(CookieName,Value);

            SetCookiePath(Cookie,null);

			if (!NonPersistent)
				Cookie.Expires = DateTime.Now.AddMonths(CookieTimeoutInMonths);

			HttpContext.Current.Response.Cookies.Add(Cookie);
		}

		/// <summary>
		/// Writes the cookie into the response stream with the value passed. The value
		///  is always the UserId.
		/// <seealso>Class WebStoreCookie</seealso>
		/// </summary>
		/// <param name="String Value">
		/// Writes a value into the specified cookie.
		/// </param>
		/// <returns>Void</returns>
		public static void WriteCookie(string Value) 
		{
			WriteCookie(Value,false);
		}
	
		
		/// <summary>
		/// Removes the cookie by clearing it out and expiring it immediately.
		/// <seealso>Class WebStoreCookie</seealso>
		/// </summary>
		/// <returns>Void</returns>
		public static void Remove() 
		{
			HttpCookie Cookie =  HttpContext.Current.Request.Cookies[CookieName];
			if (Cookie != null) 
			{
                SetCookiePath(Cookie,null);

                Cookie.Expires = DateTime.Now.AddHours(-2);
				HttpContext.Current.Response.Cookies.Add( Cookie );
			}
		}

		/// <summary>
		/// Retrieves the user's Cookie. If the Cookie doesn't exist a new one is generated
		/// by hashing a new GUID value and writing the Cookie into the Response.
		/// <returns>Customers UserId</returns>
		public static string GetId() 
		{
			string UserId;

			// Check to see if we have a cookie we can use
			HttpCookie Cookie =  HttpContext.Current.Request.Cookies[CookieName];
			if (Cookie == null)
				UserId = null;
			else
				UserId = (string) Cookie.Value;

			if (UserId == null) 
			{
				// Generate a new ID
				UserId = Guid.NewGuid().ToString().GetHashCode().ToString("x");
				WriteCookie(UserId);
			}
			return UserId;
		}

		/// <summary>
		/// Determines whether the cookie exists
		/// </summary>
		/// <returns></returns>
		public static bool CookieExist() 
		{
			// Check to see if we have a cookie we can use
			HttpCookie loCookie =  HttpContext.Current.Request.Cookies[CookieName];
			if (loCookie == null)
				return false;
			
			return true;
		}

        /// <summary>
        /// Sets the Cookie Path
        /// </summary>
        /// <param name="Cookie"></param>
        private static void SetCookiePath(HttpCookie Cookie, string Path)
        {
            if (Path == null)
            {
                Path = HttpContext.Current.Request.ApplicationPath;
                if (Path != "/")
                    Cookie.Path = Path + "/";
                else
                    Cookie.Path = "/";
            }
        }
		
	}

	/// <summary>
	/// A generic Cookie class that manages an individual Cookie by localizing the
	/// cookie management into a single class. This means the Cookie's name and
	/// and timing is abstracted.
	/// 
	/// The GetId() method is the key method here which retrieves a Cookie Id.
	/// If the cookie exists it returns the value, otherwise it generates a new
	/// Id and creates the cookie with the specs of the class and
	/// 
	/// It's recommended you store this class as a static member off another
	/// object to have
	/// </summary>
	public class CookieManager 
	{
		/// <summary>
		/// The name of the Cookie that is used. This value should always be set or 
		/// overridden via the constructor.
		/// <seealso>Class wwCookie</seealso>
		/// </summary>
		public string CookieName = "WestWindUser";

		/// <summary>
		/// The timeout of a persistent cookie.
		/// <seealso>Class wwCookie</seealso>
		/// </summary>
		public int CookieTimeoutInMonths = 48;

		public CookieManager() 
		{
		}

		public CookieManager(string NewCookieName) 
		{
			CookieName = NewCookieName;
		}

		/// <summary>
		/// Writes the cookie into the response stream with the value passed. The value
		/// is always the UserId.
		/// <seealso>Class WebStoreCookie</seealso>
		/// </summary>
		/// <param name="Value"></param>
		/// <param name="NonPersistent"></param>
		/// <returns>Void</returns>
		public void WriteCookie(string Value, bool NonPersistent) 
		{
			HttpCookie Cookie = new HttpCookie(CookieName,Value);

            SetCookiePath(Cookie,null);
            
			if (!NonPersistent)
				Cookie.Expires = DateTime.Now.AddMonths(CookieTimeoutInMonths);

			HttpContext.Current.Response.Cookies.Add(Cookie);
		}

		/// <summary>
		/// Writes the cookie into the response stream with the value passed. The value
		/// is always the UserId.
		/// <seealso>Class WebStoreCookie</seealso>
		/// </summary>
		/// <param name="Value"></param>
		/// <returns>Void</returns>
		public void WriteCookie(string Value) 
		{
			WriteCookie(Value,false);
		}
	
		
		/// <summary>
		/// Removes the cookie by clearing it out and expiring it immediately.
		/// <seealso>Class WebStoreCookie</seealso>
		/// </summary>
		/// <param name=""></param>
		/// <returns>Void</returns>
		public void Remove() 
		{
			HttpCookie Cookie =  HttpContext.Current.Request.Cookies[CookieName];
            
			if (Cookie != null) 
			{
                SetCookiePath(Cookie,null);
                Cookie.Expires = DateTime.Now.AddHours(-2);
				HttpContext.Current.Response.Cookies.Add( Cookie );
			}
		}

		/// <summary>
		/// This is the key method of this class that retrieves the value of the 
		/// cookie. This method is meant as retrieving an ID value. If the value 
		/// doesn't exist it is created and the cookie set and the value returned. If 
		/// the Cookie exists the value is retrieved and returned.
		/// <seealso>Class wwCookie</seealso>
		/// </summary>
		/// <param name=""></param>
		/// <returns>String</returns>
		public string GetId() 
		{
			string UserId;

			// Check to see if we have a cookie we can use
			HttpCookie Cookie =  HttpContext.Current.Request.Cookies[CookieName];
			if (Cookie == null)
				UserId = null;
			else
				UserId = (string) Cookie.Value;

			if (UserId == null) 
			{
				// Generate a new ID
				UserId = GenerateId();
				WriteCookie(UserId);
			}
			return UserId;
		}

		/// <summary>
		/// Method that generates the ID stored in the cookie. You can override
		/// this method in a subclass to handle custom or specific Id creation.
		/// </summary>
		/// <returns></returns>
		protected virtual string GenerateId() 
		{
			return Guid.NewGuid().ToString().GetHashCode().ToString("x");
		}

		/// <summary>
		/// Determines whether the cookie exists
		/// <seealso>Class wwCookie</seealso>
		/// </summary>
		/// <param name=""></param>
		/// <returns>Boolean</returns>
		public bool CookieExist() 
		{
			// Check to see if we have a cookie we can use
			HttpCookie Cookie =  HttpContext.Current.Request.Cookies[CookieName];
			if (Cookie == null)
				return false;
			
			return true;
		}

        /// <summary>
        /// Generic routine to create a cookie consistently and with the
        /// proper HTTP settings applied
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <param name="secure"></param>
        /// <returns></returns>
        public virtual HttpCookie CreateCookie(string name = null, string value = null, bool secure = true)
        {
            var cookie = new HttpCookie(name, value);            
            cookie.Secure = secure;
            cookie.HttpOnly = true;

            return cookie;
        }

        /// <summary>
        /// Sets the Cookie Path
        /// </summary>
        /// <param name="Cookie"></param>
        private static void SetCookiePath(HttpCookie Cookie,string Path)
        {
            Cookie.Path = "/";
            
            //if (Path == null)
            //{
            //    Path = HttpContext.Current.Request.ApplicationPath;
            //    if (Path != "/")
            //        Cookie.Path = Path + "/";
            //    else
            //        Cookie.Path = "/";
            //}
        }

	}
}
