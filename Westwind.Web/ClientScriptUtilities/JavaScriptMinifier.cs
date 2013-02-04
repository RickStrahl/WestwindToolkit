#region License
/* Originally written in 'C', this code has been converted to the C# language.
 * The author's copyright message is reproduced below.
 * All modifications from the original to C# are placed in the public domain.
 */

/* jsmin.c
 *   2007-05-22
 * 
 * Copyright (c) 2002 Douglas Crockford  (www.crockford.com)
 * 
 *  Modified by: Rick Strahl 
 *               © West Wind Technologies, 2008 2011
 *               http://www.west-wind.com/
 * 
 * Created: 09/04/2008
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

/* Originally written in 'C', this code has been converted to the C# language.
 * The author's copyright message is reproduced below.
 * All modifications from the original to C# are placed in the public domain.
 */

/* jsmin.c
   2007-05-22

Copyright (c) 2002 Douglas Crockford  (www.crockford.com)

Permission is hereby granted, free of charge, to any person obtaining a copy of
this software and associated documentation files (the "Software"), to deal in
the Software without restriction, including without limitation the rights to
use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies
of the Software, and to permit persons to whom the Software is furnished to do
so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

The Software shall be used for Good, not Evil.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

namespace Westwind.Web
{
    /// <summary>
    /// JavaScript minifier strips white space and comments from JavaScript 
    /// code. Based on Douglas Crockford's JavaScript Minifier with some modification
    /// to support string and StreamReader conversions.
    /// 
    /// This class can minify strings in memory or files and entire directories of
    /// disk files.
    /// 
    /// The MinifyDirectory() method can be used in the build process for VS or 
    /// can easily be used at application startup to automatically create minified
    /// script files for an application.
    /// </summary>
    public class JavaScriptMinifier
    {
        const int EOF = -1;

        StreamReader sr;
        StreamWriter sw;
        int theA;
        int theB;
        int theLookahead = EOF;

        
        /// <summary>
        /// Minifies a source file into a target file.
        /// </summary>
        /// <param name="sourceFile">Source file that is to be compressed</param>
        /// <param name="targetFile">Target file that is to contain the compressed output</param>
        public void Minify(string sourceFile, string targetFile)
        {            
            using (sr = new StreamReader(sourceFile))
            {
                using (sw = new StreamWriter(targetFile))
                {
                    jsmin();
                }
            }
        }

        /// <summary>
        /// Minifies a JavaScript code string into a minified string.
        /// </summary>
        /// <param name="sourceJavaScriptString">Input Javascript string to be minified</param>
        /// <returns></returns>
        public string MinifyString(string sourceJavaScriptString)
        {                         
            MemoryStream srcStream = new MemoryStream(Encoding.Default.GetBytes(sourceJavaScriptString));
            MemoryStream tgStream = new MemoryStream(8092);

            using (sr = new StreamReader(srcStream))
            {
                using (sw = new StreamWriter(tgStream))
                {
                    jsmin();
                }
            }

            return Encoding.Default.GetString(tgStream.ToArray());
        }

        /// <summary>
        /// Minifies all JavaScript files in a given directory and writes out the 
        /// minified files to a new file extensions (.min.js for example).
        /// 
        /// This method can be integrated into the build process, or as part of an 
        /// application's startup to always create new minified scripts as needed. 
        /// Scripts are only minified if the minified files don't already exist and are
        ///  older than the corresponding JavaScript file.
        /// 
        /// A common usage scenario is to call this static method from 
        /// Application_Start:
        /// 
        /// &lt;&lt;code lang="C#"&gt;&gt;void Application_Start(object sender, 
        /// EventArgs e)
        /// {
        ///     // creates minify scripts if don't exist or are changed
        ///     // NOTE: REQUIRES that IIS/ASP.NET Account has writes to write 
        /// here!
        ///     
        /// Westwind.Web.Controls.JavaScriptMinifier.MinifyDirectory(Server.MapPath("~/
        /// scripts"), ".min.js", true);
        /// }&lt;&lt;/code&gt;&gt;
        /// 
        /// to always ensure minified files are in sync with corresponding JavaScript 
        /// files.
        /// <seealso>Class JavaScriptMinifier                                                        </seealso>
        /// </summary>
        /// <param name="path">
        /// The path where files are to be minfied
        /// </param>
        /// <param name="minExtension">
        /// The extension for the minified files (ie. .min.js). Include leading dot!
        /// </param>
        /// <param name="recursive">
        /// Determines whether nested directories are also included
        /// </param>
        /// <remarks>
        /// Note that if you use this script from within an ASP.NET application it's 
        /// best to hook it to a Application_Start or a static constructor so it only 
        /// fires once.
        /// 
        /// When called from ASP.NET this routine REQUIRES that the server account that
        ///  ASP.NET/IIS AppPool runs under (NETWORK SERVICE by default) has rights to 
        /// write out the file to the folder specified. Otherwise an exception occurs.
        /// </remarks>
        public static void MinifyDirectory(string path, string minExtension, bool recursive)
        {
            JavaScriptMinifier min = new JavaScriptMinifier();

            minExtension = minExtension.ToLower();
            if (!minExtension.StartsWith("."))
                minExtension = "." + minExtension;


            string[] files = null;
            try
            {
                files = Directory.GetFiles(path);
            }
            catch
            {
                throw new InvalidOperationException("Invalid or inaccessible path to create Min Scripts in: " + path);
            }

            try
            {
                foreach (string file in files)
                {
                    string extension = Path.GetExtension(file).ToLower();

                    if (extension == ".js" && !file.EndsWith(minExtension))
                    {
                        string minFile = file.Replace(".js", minExtension);
                        DateTime fileAT = File.GetLastWriteTimeUtc(file);
                        DateTime minFileAT = File.GetLastWriteTimeUtc(minFile);

                        if (!File.Exists(minFile) || fileAT > minFileAT)
                            min.Minify(file, file.Replace(".js", minExtension));
                    }
                }

                if (recursive)
                {
                    string[] dirs = Directory.GetDirectories(path);
                    foreach (string dir in dirs)
                    {
                        if (!dir.StartsWith("."))
                            MinifyDirectory(dir, minExtension, true);
                    }
                }
            }
            catch
            {
                throw new AccessViolationException("Couldn't create Min Scripts in: " + path + ". Make sure ASP.NET has permissions to write in this path.");
            }
        }


        /* jsmin -- Copy the input to the output, deleting the characters which are
                insignificant to JavaScript. Comments will be removed. Tabs will be
                replaced with spaces. Carriage returns will be replaced with linefeeds.
                Most spaces and linefeeds will be removed.
        */
        void jsmin()
        {
            theA = '\n';
            action(3);
            while (theA != EOF)
            {
                switch (theA)
                {
                    case ' ':
                        {
                            if (isAlphanum(theB))
                            {
                                action(1);
                            }
                            else
                            {
                                action(2);
                            }
                            break;
                        }
                    case '\n':
                        {
                            switch (theB)
                            {
                                case '{':
                                case '[':
                                case '(':
                                case '+':
                                case '-':
                                    {
                                        action(1);
                                        break;
                                    }
                                case ' ':
                                    {
                                        action(3);
                                        break;
                                    }
                                default:
                                    {
                                        if (isAlphanum(theB))
                                        {
                                            action(1);
                                        }
                                        else
                                        {
                                            action(2);
                                        }
                                        break;
                                    }
                            }
                            break;
                        }
                    default:
                        {
                            switch (theB)
                            {
                                case ' ':
                                    {
                                        if (isAlphanum(theA))
                                        {
                                            action(1);
                                            break;
                                        }
                                        action(3);
                                        break;
                                    }
                                case '\n':
                                    {
                                        switch (theA)
                                        {
                                            case '}':
                                            case ']':
                                            case ')':
                                            case '+':
                                            case '-':
                                            case '"':
                                            case '\'':
                                                {
                                                    action(1);
                                                    break;
                                                }
                                            default:
                                                {
                                                    if (isAlphanum(theA))
                                                    {
                                                        action(1);
                                                    }
                                                    else
                                                    {
                                                        action(3);
                                                    }
                                                    break;
                                                }
                                        }
                                        break;
                                    }
                                default:
                                    {
                                        action(1);
                                        break;
                                    }
                            }
                            break;
                        }
                }
            }
        }
        /* action -- do something! What you do is determined by the argument:
                1   Output A. Copy B to A. Get the next B.
                2   Copy B to A. Get the next B. (Delete A).
                3   Get the next B. (Delete B).
           action treats a string as a single character. Wow!
           action recognizes a regular expression if it is preceded by ( or , or =.
        */
        void action(int d)
        {
            if (d <= 1)
            {
                put(theA);
            }
            if (d <= 2)
            {
                theA = theB;
                if (theA == '\'' || theA == '"')
                {
                    for (; ; )
                    {
                        put(theA);
                        theA = get();
                        if (theA == theB)
                        {
                            break;
                        }
                        if (theA <= '\n')
                        {
                            throw new Exception(string.Format("Error: JSMIN unterminated string literal: {0}\n", theA));
                        }
                        if (theA == '\\')
                        {
                            put(theA);
                            theA = get();
                        }
                    }
                }
            }
            if (d <= 3)
            {
                theB = next();
                if (theB == '/' && (theA == '(' || theA == ',' || theA == '=' ||
                                    theA == '[' || theA == '!' || theA == ':' ||
                                    theA == '&' || theA == '|' || theA == '?' ||
                                    theA == '{' || theA == '}' || theA == ';' ||
                                    theA == '\n'))
                {
                    put(theA);
                    put(theB);
                    for (; ; )
                    {
                        theA = get();
                        if (theA == '/')
                        {
                            break;
                        }
                        else if (theA == '\\')
                        {
                            put(theA);
                            theA = get();
                        }
                        else if (theA <= '\n')
                        {
                            throw new Exception(string.Format("Error: JSMIN unterminated Regular Expression literal : {0}.\n", theA));
                        }
                        put(theA);
                    }
                    theB = next();
                }
            }
        }
        /* next -- get the next character, excluding comments. peek() is used to see
                if a '/' is followed by a '/' or '*'.
        */
        int next()
        {
            int c = get();
            if (c == '/')
            {
                switch (peek())
                {
                    case '/':
                        {
                            for (; ; )
                            {
                                c = get();
                                if (c <= '\n')
                                {
                                    return c;
                                }
                            }
                        }
                    case '*':
                        {
                            get();
                            for (; ; )
                            {
                                switch (get())
                                {
                                    case '*':
                                        {
                                            if (peek() == '/')
                                            {
                                                get();
                                                return ' ';
                                            }
                                            break;
                                        }
                                    case EOF:
                                        {
                                            throw new Exception("Error: JSMIN Unterminated comment.\n");
                                        }
                                }
                            }
                        }
                    default:
                        {
                            return c;
                        }
                }
            }
            return c;
        }
        /* peek -- get the next character without getting it.
        */
        int peek()
        {
            theLookahead = get();
            return theLookahead;
        }
        /* get -- return the next character from stdin. Watch out for lookahead. If
                the character is a control character, translate it to a space or
                linefeed.
        */
        int get()
        {
            int c = theLookahead;
            theLookahead = EOF;
            if (c == EOF)
            {
                c = sr.Read();
            }
            if (c >= ' ' || c == '\n' || c == EOF)
            {
                return c;
            }
            if (c == '\r')
            {
                return '\n';
            }
            return ' ';
        }
        void put(int c)
        {
                sw.Write((char)c);
        }
        /* isAlphanum -- return true if the character is a letter, digit, underscore,
                dollar sign, or non-ASCII character.
        */
        bool isAlphanum(int c)
        {
            return ((c >= 'a' && c <= 'z') || (c >= '0' && c <= '9') ||
                (c >= 'A' && c <= 'Z') || c == '_' || c == '$' || c == '\\' ||
                c > 126);
        }


    }
}

