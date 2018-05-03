using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using Westwind.Utilities;

namespace Westwind.Web.Services
{
    /// <summary>
    /// Class that holds a few static string definitions for
    /// embedding Share on Google+ and Twitter
    /// </summary>
    public static class ShareButtons
    {

        /// <summary>
        /// Places a Google+  +1 and Share button in the page
        /// </summary>
        /// <param name="url">The Url to share. If not provided the current page is used</param>
        /// <param name="buttonSize">small,medium,standard,tall</param>
        /// <returns></returns>
        public static string GooglePlusPlusOneButton(string url = null, 
                                                     string buttonSize = "medium", 
                                                     int width = -1)
        {
            if (!string.IsNullOrEmpty(url))
                url = " href=\"" + url + "\"";
            else
                url = string.Empty;

            string linkWidth = string.Empty;
            if (width != -1)
                linkWidth=" width=\"" + width.ToString() + "\"";
            
            return 
                @"
<g:plusone size="""  + buttonSize.ToString() +   "\"" + url + " " + linkWidth  +  @"""></g:plusone>
<script type=""text/javascript"">
  (function() {
    var po = document.createElement('script'); po.type = 'text/javascript'; po.async = true;
    po.src = 'https://apis.google.com/js/plusone.js';
    var s = document.getElementsByTagName('script')[0]; s.parentNode.insertBefore(po, s);
  })();
</script>
";
        }
        

        /// <summary>
        /// Inserts a Tweet button to share tweet on Twitter with an image link
        /// </summary>
        /// <param name="text">The text to present</param>
        /// <param name="twitterShareAccount"></param>
        /// <returns></returns>
        public static string ShareOnTwitter(string text, string twitterShareAccount=null, string url = null, string hashTag = null)
        {
            string format =
@"<a href=""https://twitter.com/share"" class=""twitter-share-button"" data-text=""{0}"" data-via=""{1}"" data-lang=""en"" data-hashtags=""{3}"" data-url=""{2}"">Tweet</a>
<script>!function(d,s,id){{var js,fjs=d.getElementsByTagName(s)[0];if(!d.getElementById(id)){{js=d.createElement(s);js.id=id;js.src=""//platform.twitter.com/widgets.js"";fjs.parentNode.insertBefore(js,fjs);}}}}(document,""script"",""twitter-wjs"");</script>";

            return string.Format(format,text,twitterShareAccount,url,hashTag);
        }

        public static string ShareOnFacebook(string url, string text = null)
        {
            
            var baseUrl = WebUtils.ResolveUrl("~/");

            //url = $"https://www.facebook.com/dialog/feed?app_id={appid}&display=popup&caption={text}&link={url}&redirect_uri={url}";
            url = $"https://www.facebook.com/sharer/sharer.php?u={url}&display=page";
            string link =
$@"<a href=""{url}"" target=""_blank"">
      <img src=""{baseUrl}images/shareonfacebook.png"" style='height: 20px;' />
</a>";
            
            return link;
        }

        public static string ShareOnFacebookFull(string url)
        {
            return null;
            
        }
    }

    public enum GooglePlusOneButtonSizes
    {
        Small15,
        Medium20,
        Standard24,
        Tall60
    }
}
    