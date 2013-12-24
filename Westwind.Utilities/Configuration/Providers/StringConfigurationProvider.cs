#region License
/*
 **************************************************************
 *  Author: Rick Strahl 
 *          © West Wind Technologies, 2009-2013
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
namespace Westwind.Utilities.Configuration
{

    /// <summary>
    /// Reads and Writes configuration settings from strings - which you manage
    /// yourself. Using strings for the configuration provider allows for easy
    /// storage into any non-supported configuration stores that you control
    /// through your code as long as it supports strings.   
    /// 
    /// The string provider is a real minimal implementation that only implements
    /// WriteAsString(config) and Read(string). It inherits all of its functionality
    /// from the base provider.
    /// </summary>
    public class StringConfigurationProvider<TAppConfiguration> : ConfigurationProviderBase<TAppConfiguration>
        where TAppConfiguration : AppConfiguration, new()
    {
        public string InitialStringData { get; set; }

        /// <summary>
        /// Reads from the InitialStringData string data
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public override T Read<T>()
        {
            return this.Read<T>(InitialStringData);
        }

        /// <summary>
        /// Reads configuration information into config from InitialStringData
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        public override bool Read(AppConfiguration config)
        {
            return Read(config, InitialStringData);
        }

        /// <summary>
        /// Not supported for StringConfiguration
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        public override bool Write(AppConfiguration config)
        {
            throw new NotImplementedException();
        }
    }
}
