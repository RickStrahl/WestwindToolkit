#region License
/*
 **************************************************************
 *  Author: Rick Strahl 
 *          © West Wind Technologies, 2008 2011
 *          http://www.west-wind.com/
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
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Web.Routing;
using System.Reflection;

namespace Westwind.Web
{

    public enum PostBackModes
    {
        /// No Form data is posted (but there may still be some post state)
        None,

        /// <summary>
        /// No POST data is posted back to the server
        /// </summary>
        Get,
        /// <summary>
        /// Only standard POST data is posted back - ASP.NET Post stuff left out
        /// </summary>
        Post,
        /// <summary>
        /// Posts back POST data but skips ViewState and EventTargets
        /// </summary>
        PostNoViewstate,
        /// <summary>
        /// Posts only the method parameters and nothing else
        /// </summary>
        PostMethodParametersOnly
    }

    public enum JavaScriptCodeLocationTypes
    {
        /// <summary>
        /// Causes the Javascript code to be embedded into the page on every 
        /// generation. Fully self-contained.
        /// <seealso>Enumeration JavaScriptCodeLocationTypes</seealso>
        /// </summary>
        EmbeddedInPage,
        /// <summary>
        /// Keeps the .js file as an external file in the Web application. If this is 
        /// set you should set the &lt;&lt;%= TopicLink([ScriptLocation],[_1Q01F9K4D]) 
        /// %&gt;&gt; Property to point at the location of the file.
        /// 
        /// This option requires that you deploy the .js file with your application.
        /// <seealso>Enumeration JavaScriptCodeLocationTypes</seealso>
        /// </summary>
        ExternalFile,
        /// <summary>
        /// ASP.NET 2.0 option to generate a WebResource.axd call that feeds the .js 
        /// file to the client.
        /// <seealso>Enumeration JavaScriptCodeLocationTypes</seealso>
        /// </summary>
        WebResource,
        /// <summary>
        /// Don't include any script - assume the page owner will handle it all
        /// </summary>
        None
    }

    public enum ProxyClassGenerationModes
    {
        /// <summary>
        /// The proxy is generated inline of the page.
        /// </summary>
        Inline, 
        /// <summary>
        /// No proxy is generated at all
        /// </summary>
        None,
        /// <summary>
        /// Works only with CallbackHandler implementations
        /// that run as handlers at a distinct URL.
        /// JsonCallbacks.ashx/jsdebug
        /// </summary>
        jsdebug
    }


}
