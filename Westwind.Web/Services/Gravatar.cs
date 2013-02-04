using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;

namespace Westwind.Web.Services
{
    /// <summary>
    /// Implements the Gravatar API for retrieving a Gravatar image to display
    /// </summary>
    public static class Gravatar
    {
        public const string GravatarBaseUrl = "http://www.gravatar.com/avatar.php";

        /// <summary>
        /// Returns a Gravatar image url for an email address
        /// </summary>
        /// <param name="Email">Email address to display Gravatar for</param>
        /// <param name="Size">Size in pixels (square image) (80)</param>
        /// <param name="Rating">Parental Guidance rating of image (PG)</param>
        /// <param name="DefaultImageUrl">Url to image if no match is found. 
        ///  If not passed gravatar provides default image</param>
        public static string GetGravatarLink(string Email, int Size=80, 
                                             string Rating="PG", string DefaultImageUrl=null)
                                             
        {
            byte[] Hash = null;

            if (string.IsNullOrEmpty(Email))
                Hash = new byte[] { 0 };
            else
            {
                MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
                Hash = md5.ComputeHash(Encoding.ASCII.GetBytes(Email));
            }

            StringBuilder sb = new System.Text.StringBuilder();
            for (int x = 0; x < Hash.Length; x++)
            {
                sb.Append(Hash[x].ToString("x2"));
            }

            if (!string.IsNullOrEmpty(DefaultImageUrl))
                DefaultImageUrl = "&default=" + DefaultImageUrl;
            else
                DefaultImageUrl = "";

            return string.Format("{0}?gravatar_id={1}&size={2}&rating={3}{4}",
                                   Gravatar.GravatarBaseUrl, sb.ToString(), Size, Rating, DefaultImageUrl);
        }

        /// <summary>
        /// Returns a Gravatar Image Tag that can be directly embedded into
        /// an HTML document.
        /// </summary>
        /// <param name="Email">Email address to display Gravatar for</param>
        /// <param name="Size">Size in pixels (square image) (80)</param>
        /// <param name="Rating">Parental Guidance rating of image (PG)</param>
        /// <param name="ExtraImageAttributes">Any extra attributes to stick on the img tag</param>
        /// <param name="DefaultImageUrl">Url to image if no match is found. 
        ///  If not passed gravatar provides default image</param>
        /// <returns></returns>
        public static string GetGravatarImage(string Email, int Size=80, string Rating = "PG",
                                              string ExtraImageAttributes = null, 
                                              string DefaultImageUrl = null)
        {
            string Url = GetGravatarLink(Email, Size, Rating, DefaultImageUrl);
            return string.Format("<img src='{0}' {1}>", Url, ExtraImageAttributes, DefaultImageUrl);
        }

    }

}
