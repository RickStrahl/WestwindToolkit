#region License
/*
 **************************************************************
 *  Author: Rick Strahl 
 *          © West Wind Technologies, 2009
 *          http://www.west-wind.com/
 * 
 * Created: 09/12/2009
 *
 * Permission is hereby granted, free of charge, to any person
 * obtaining a copy of this software and associated documentation
 * files (the "Software"), to deal in the Software without
 * restriction, including without limitation the rights to use,
 * copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the
 * Software is furnished to do so, subject to the following
 * conditions:
 * 
 * The above copyright notice and this permission notice shall be
 * included in all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
 * EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
 * OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
 * NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
 * HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
 * WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
 * FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
 * OTHER DEALINGS IN THE SOFTWARE.
 **************************************************************  
*/
#endregion

using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using System.Globalization;

namespace Westwind.Utilities
{

    /// <summary>
    /// Internally used class that is used to expand links in text
    /// strings.
    /// </summary>
    public class UrlParser
    {
        internal string Target = string.Empty;
        internal bool ParseFormattedLinks = false;

        /// <summary>
        /// Expands links into HTML hyperlinks inside of text or HTML.
        /// </summary>
        /// <param name="text">The text to expand</param>
        /// <param name="target">Target frame where links are displayed</param>
        /// <param name="parseFormattedLinks">Allows parsing of links in the following format [text|www.site.com]</param>
        /// <returns></returns>
        public static string ExpandUrls(string text, string target = null, bool parseFormattedLinks = false)
        {
            if (target == null)
                target = string.Empty;

            UrlParser Parser = new UrlParser();
            Parser.Target = target;
            Parser.ParseFormattedLinks = parseFormattedLinks;

            return Parser.ExpandUrlsInternal(text);
        }

        /// <summary>
        /// Expands links into HTML hyperlinks inside of text or HTML.
        /// </summary>
        /// <param name="text">The text to expand</param>    
        /// <returns></returns>
        private string ExpandUrlsInternal(string text)
        {
            MatchEvaluator matchEval = null;
            string pattern = null;
            string updated = null;


            // Expand embedded hyperlinks
            System.Text.RegularExpressions.RegexOptions options =
                                                                  RegexOptions.Multiline |
                                                                  RegexOptions.IgnoreCase;
            if (ParseFormattedLinks)
            {
                pattern = @"\[(.*?)\|(.*?)]";

                matchEval = new MatchEvaluator(ExpandFormattedLinks);
                updated = Regex.Replace(text, pattern, matchEval, options);
            }
            else
                updated = text;

            pattern = @"([""'=]|&quot;)?(http://|ftp://|https://|www\.|ftp\.[\w]+)([\w\-\.,@?^=%&amp;:/~\+#]*[\w\-\@?^=%&amp;/~\+#])";

            matchEval = new MatchEvaluator(ExpandUrlsRegExEvaluator);
            updated = Regex.Replace(updated, pattern, matchEval, options);



            return updated;
        }

        /// <summary>
        /// Internal RegExEvaluator callback
        /// </summary>
        /// <param name="M"></param>
        /// <returns></returns>
        private string ExpandUrlsRegExEvaluator(System.Text.RegularExpressions.Match M)
        {
            string Href = M.Value; // M.Groups[0].Value;

            // if string starts within an HREF don't expand it
            if (Href.StartsWith("=") ||
                Href.StartsWith("'") ||
                Href.StartsWith("\"") ||
                Href.StartsWith("&quot;"))
                return Href;

            string Text = Href;

            if (Href.IndexOf("://") < 0)
            {
                if (Href.StartsWith("www."))
                    Href = "http://" + Href;
                else if (Href.StartsWith("ftp"))
                    Href = "ftp://" + Href;
                else if (Href.IndexOf("@") > -1)
                    Href = "mailto:" + Href;
            }

            string Targ = !string.IsNullOrEmpty(Target) ? " target='" + Target + "'" : string.Empty;

            return "<a href='" + Href + "'" + Targ +
                    ">" + Text + "</a>";
        }

        private string ExpandFormattedLinks(System.Text.RegularExpressions.Match M)
        {
            //string Href = M.Value; // M.Groups[0].Value;

            string Text = M.Groups[1].Value;
            string Href = M.Groups[2].Value;

            if (Href.IndexOf("://") < 0)
            {
                if (Href.StartsWith("www."))
                    Href = "http://" + Href;
                else if (Href.StartsWith("ftp"))
                    Href = "ftp://" + Href;
                else if (Href.IndexOf("@") > -1)
                    Href = "mailto:" + Href;
                else
                    Href = "http://" + Href;
            }

            string Targ = !string.IsNullOrEmpty(Target) ? " target='" + Target + "'" : string.Empty;

            return "<a href='" + Href + "'" + Targ +
                    ">" + Text + "</a>";
        }



    }
}
