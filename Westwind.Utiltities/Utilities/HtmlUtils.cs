using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using System.Globalization;
using System.Linq;
using Westwind.Utilities.Properties;

namespace Westwind.Utilities
{
    /// <summary>
    /// Html string and formatting utilities
    /// </summary>
    public static class HtmlUtils
    {
        /// <summary>
        /// Replaces and  and Quote characters to HTML safe equivalents.
        /// </summary>
        /// <param name="html">HTML to convert</param>
        /// <returns>Returns an HTML string of the converted text</returns>
        public static string FixHTMLForDisplay(string html)
        {
            html = html.Replace("<", "&lt;");
            html = html.Replace(">", "&gt;");
            html = html.Replace("\"", "&quot;");
            return html;
        }

        /// <summary>
        /// Strips HTML tags out of an HTML string and returns just the text.
        /// </summary>
        /// <param name="html">Html String</param>
        /// <returns></returns>
        public static string StripHtml(string html)
        {
            html = Regex.Replace(html, @"<(.|\n)*?>", string.Empty);
            html = html.Replace("\t", " ");
            html = html.Replace("\r\n", string.Empty);
            html = html.Replace("   ", " ");
            return html.Replace("  ", " ");
        }

        /// <summary>
        /// Fixes a plain text field for display as HTML by replacing carriage returns 
        /// with the appropriate br and p tags for breaks.
        /// </summary>
        /// <param name="htmlText">Input string</param>
        /// <returns>Fixed up string</returns>
        public static string DisplayMemo(string htmlText)
        {
            if (htmlText == null)
                return string.Empty;

            htmlText = htmlText.Replace("\r\n", "\r");
            htmlText = htmlText.Replace("\n", "\r");
            //HtmlText = HtmlText.Replace("\r\r","<p>");
            htmlText = htmlText.Replace("\r", "<br />\r\n");
            return htmlText;
        }

        /// <summary>
        /// Method that handles handles display of text by breaking text.
        /// Unlike the non-encoded version it encodes any embedded HTML text
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string DisplayMemoEncoded(string text)
        {
            if (text == null)
                return string.Empty;

            bool PreTag = false;
            if (text.Contains("<pre>"))
            {
                text = text.Replace("<pre>", "__pre__");
                text = text.Replace("</pre>", "__/pre__");
                PreTag = true;
            }


            // fix up line breaks into <br><p>
            text = DisplayMemo(HtmlEncode(text)); //HttpUtility.HtmlEncode(Text));

            if (PreTag)
            {
                text = text.Replace("__pre__", "<pre>");
                text = text.Replace("__/pre__", "</pre>");
            }

            return text;
        }

        /// <summary>
        /// HTML-encodes a string and returns the encoded string.
        /// </summary>
        /// <param name="text">The text string to encode. </param>
        /// <returns>The HTML-encoded text.</returns>
        public static string HtmlEncode(string text)
        {
            if (text == null)
                return string.Empty;

            StringBuilder sb = new StringBuilder(text.Length);

            int len = text.Length;
            for (int i = 0; i < len; i++)
            {
                switch (text[i])
                {

                    case '<':
                        sb.Append("&lt;");
                        break;
                    case '>':
                        sb.Append("&gt;");
                        break;
                    case '"':
                        sb.Append("&quot;");
                        break;
                    case '&':
                        sb.Append("&amp;");
                        break;
                    default:
                        if (text[i] > 159)
                        {
                            // decimal numeric entity
                            sb.Append("&#");
                            sb.Append(((int)text[i]).ToString(CultureInfo.InvariantCulture));
                            sb.Append(";");
                        }
                        else
                            sb.Append(text[i]);
                        break;
                }
            }
            return sb.ToString();
        }



        /// <summary>
        /// Create an Href HTML link
        /// </summary>
        /// <param name="text"></param>
        /// <param name="url"></param>
        /// <param name="target"></param>
        /// <param name="additionalMarkup"></param>
        /// <returns></returns>
        public static string Href(string text, string url, string target = null, string additionalMarkup = null)
        {
            if (url.StartsWith("~"))
                url = ResolveUrl(url);

            return "<a href=\"" + url + "\" " +
                (string.IsNullOrEmpty(target) ? string.Empty : "target=\"" + target + "\" ") +
                (string.IsNullOrEmpty(additionalMarkup) ? string.Empty : additionalMarkup) +
                ">" + text + "</a>";
        }

        /// <summary>
        /// Creates an HREF HTML Link
        /// </summary>
        /// <param name="url"></param>
        public static string Href(string url)
        {
            return Href(url, url, null, null);
        }

        /// <summary>
        /// Returns an IMG link as a string. If the image is null
        /// or empty a blank string is returned.
        /// </summary>
        /// <param name="imageUrl"></param>
        /// <param name="additionalMarkup">any additional attributes added to the element</param>
        /// <returns></returns>
        public static string ImgRef(string imageUrl, string additionalMarkup = null)
        {
            if (string.IsNullOrEmpty(imageUrl))
                return string.Empty;

            if (imageUrl.StartsWith("~"))
                imageUrl = ResolveUrl(imageUrl);

            string img = "<img src=\"" + imageUrl + "\" ";

            if (!string.IsNullOrEmpty("additionalMarkup"))
                img += additionalMarkup + " ";

            img += "/>";
            return img;
        }


        /// <summary>
        /// Resolves a URL based on the current HTTPContext
        /// 
        /// Note this method is added here internally only
        /// to support the HREF() method and ~ expansion
        /// on urls.
        /// </summary>
        /// <param name="originalUrl"></param>
        /// <returns></returns>
        internal static string ResolveUrl(string originalUrl)
        {
            if (string.IsNullOrEmpty(originalUrl))
                return string.Empty;

            // Absolute path - just return
            if (originalUrl.IndexOf("://") != -1)
                return originalUrl;

            // Fix up image path for ~ root app dir directory
            if (originalUrl.StartsWith("~"))
            {
                //return VirtualPathUtility.ToAbsolute(originalUrl);
                string newUrl = "";
                if (HttpContext.Current != null)
                {
                    newUrl = HttpContext.Current.Request.ApplicationPath +
                          originalUrl.Substring(1);
                    newUrl = newUrl.Replace("//", "/"); // must fix up for root path
                }
                else
                    // Not context: assume current directory is the base directory
                    throw new ArgumentException("Invalid URL: Relative URL not allowed.");

                // Just to be sure fix up any double slashes
                return newUrl;
            }

            return originalUrl;
        }


        /// <summary>
        /// Creates an Abstract from an HTML document. Strips the 
        /// HTML into plain text, then creates an abstract.
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns>
        public static string HtmlAbstract(string html, int length)
        {
            return StringUtils.TextAbstract(StripHtml(html), length);
        }


    }
}
