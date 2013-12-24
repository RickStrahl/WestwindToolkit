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
using System.Reflection;

namespace Westwind.Utilities.Configuration
{
    /// <summary>
    /// Configuration Provider interface that provides read and write services
    /// to various configuration storage mechanisms.
    /// 
    /// Used in conjunction with the <seealso cref="AppConfiguration"/> class. 
    /// A base implementation from which to inherit is provided in the
    /// <seealso cref="Westwind.Utilities.Configuration.ConfigurationProviderBase"/>  class.
    /// </summary>
    public interface IConfigurationProvider        
    {
        /// <summary>
        /// Holds an error message after a read or write operation
        /// failed.
        /// </summary>
        string ErrorMessage { get; set; }

        /// <summary>
        /// A comma delimited list of fields that are to be encrypted
        /// </summary>
        string PropertiesToEncrypt { get; set; }

        /// <summary>
        /// The encryption key used to encrypt fields in config objects
        /// </summary>
        string EncryptionKey { get; set; }

        /// <summary>
        /// Optional Section name that can be used to sub-segment in multi-config files
        /// </summary>
        string ConfigurationSection { get; set; }

        /// <summary>
        /// Reads configuration information into new configuration object instance
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        T Read<T>()
            where T : AppConfiguration, new();

        /// <summary>
        /// Reads configuration information into a provided config object instance
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        bool Read(AppConfiguration config);        
        T Read<T>(string xml)
            where T: AppConfiguration, new();

        /// <summary>
        /// Reads configuration information from an XML string (Xml Serialization format)
        /// into a provided config object instance
        /// </summary>
        /// <param name="config"></param>
        /// <param name="xml"></param>
        /// <returns></returns>
        bool Read(AppConfiguration config, string xml);

        /// <summary>
        /// Writes configuration information into a provided object instance
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        bool Write(AppConfiguration config);

        /// <summary>
        /// Writes configuration for a provided config object and returns
        /// the serialized data as a string.
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        string WriteAsString(AppConfiguration config);

        /// <summary>
        /// Encrypts fields in a config object as specified in the <seealso cref="FieldsToEncrypt"/> property.
        /// </summary>
        /// <param name="config"></param>
        void EncryptFields(AppConfiguration config);

        /// <summary>
        /// Decryptes the encyrpted fields in a config object a
        /// </summary>
        /// <param name="config"></param>
        void DecryptFields(AppConfiguration config);
    }

}
