#region License
/*
 **************************************************************
 *  Author: Rick Strahl 
 *          © West Wind Technologies, 2011
 *          http://www.west-wind.com/
 * 
 * Created: 6/19/2011
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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.IO;
using System.Net;
using System.Diagnostics;

namespace Westwind.Utilities
{
    public static class ShellUtils
    {

        /// <summary>
        /// Uses the Shell Extensions to launch a program based on URL moniker or file name
        /// Basically a wrapper around ShellExecute
        /// </summary>
        /// <param name="url">Any URL Moniker that the Windows Shell understands (URL, Word Docs, PDF, Email links etc.)</param>
        /// <returns></returns>
        public static int GoUrl(string url)
        {
            string TPath = Path.GetTempPath();
           
            ProcessStartInfo info = new ProcessStartInfo();
            info.UseShellExecute = true;
            info.Verb = "Open";
            info.WorkingDirectory = TPath;
            info.FileName = url;

            Process process = new Process(); 
            process.StartInfo = info;
            process.Start();

            return 0;
        }


        /// <summary>
        /// Displays a string in in a browser as HTML. Optionally
        /// provide an alternate extension to display in the appropriate
        /// text viewer (ie. "txt" likely shows in NotePad)
        /// </summary>
        /// <param name="text"></param>
        /// <param name="extension"></param>
        /// <returns></returns>
        public static int ShowString(string text, string extension = null)
        {
            if (extension == null)
                extension = "htm";

            string File = Path.GetTempPath() + "\\__preview." + extension;
            StreamWriter sw = new StreamWriter(File, false, Encoding.Default);
            sw.Write(text);
            sw.Close();

            return GoUrl(File);
        }

        /// <summary>
        /// Shows a string as HTML
        /// </summary>
        /// <param name="htmlString"></param>
        /// <returns></returns>
        public static int ShowHtml(string htmlString)
        {
            return ShowString(htmlString, null);
        }

        /// <summary>
        /// Displays a large Text string as a text file in the
        /// systems' default text viewer (ie. NotePad)
        /// </summary>
        /// <param name="TextString"></param>
        /// <returns></returns>
        public static int ShowText(string TextString)
        {
            string File = Path.GetTempPath() + "\\__preview.txt";

            StreamWriter sw = new StreamWriter(File, false);
            sw.Write(TextString);
            sw.Close();

            return GoUrl(File);
        }

        /// <summary>
        /// Simple method to retrieve HTTP content from the Web quickly
        /// </summary>
        /// <param name="url">Url to access</param>        
        /// <returns>Http response text or null</returns>
        public static string HttpGet(string url)
        {
            string errorMessage;
            return HttpGet(url, out errorMessage);
        }

        /// <summary>
        /// Simple method to retrieve HTTP content from the Web quickly
        /// </summary>
        /// <param name="url">Url to access</param>
        /// <param name="errorMessage"></param>
        /// <returns></returns>
        public static string HttpGet(string url, out string errorMessage)
        {
            string responseText = string.Empty;
            errorMessage = null;

            WebClient Http = new WebClient();

            // Download the Web resource and save it into a data buffer.
            try
            {
                responseText = Http.DownloadString(url);                
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                return null;
            }

            return responseText;
        }


        /// <summary>
        /// Retrieves a buffer of binary data from a URL using
        /// a plain HTTP Get.
        /// </summary>
        /// <param name="url">Url to access</param>
        /// <returns>Response bytes or null on error</returns>
        public static byte[] HttpGetBytes(string url)
        {
            string errorMessage;
            return HttpGetBytes(url,out errorMessage);
        }

        /// <summary>
        /// Retrieves a buffer of binary data from a URL using
        /// a plain HTTP Get.
        /// </summary>
        /// <param name="url">Url to access</param>
        /// <param name="errorMessage">ref parm to receive an error message</param>
        /// <returns>response bytes or null on error</returns>
        public static byte[] HttpGetBytes(string url, out string errorMessage)
        {
            byte[] result = null;
            errorMessage = null;

            var Http = new WebClient();

            try
            {
                result = Http.DownloadData(url);
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                return null;
            }

            return result;
        }

    }
}
