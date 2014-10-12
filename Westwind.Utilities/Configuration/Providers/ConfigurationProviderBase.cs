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
using System.Reflection;
using Westwind.Utilities.Properties;

namespace Westwind.Utilities.Configuration
{
    /// <summary>
    /// Base Configuration Provider Implementation. This implementation provides
    /// for the basic layout of a provider and fields that implement the
    /// IConfigurationProvider interface.
    /// 
    /// The Read and Write methods must be overridden - all other methods and 
    /// fields are optional
    /// 
    /// </summary>
    public abstract class ConfigurationProviderBase<TAppConfiguration> : IConfigurationProvider
        where TAppConfiguration : AppConfiguration, new()
    {

        /// <summary>
        /// Displays error information when results fail.
        /// </summary>
        public virtual string ErrorMessage
        {
            get { return _ErrorMessage; }
            set { _ErrorMessage = value; }
        }
        private string _ErrorMessage = string.Empty;


        /// <summary>
        /// A comma delimiter list of property names that are 
        /// to be encrypted when persisted to the store
        /// </summary>
        public virtual string PropertiesToEncrypt
        {
            get { return _PropertiesToEncrypt; }
            set { _PropertiesToEncrypt = value; }
        }
        private string _PropertiesToEncrypt = string.Empty;

        /// <summary>
        /// The encryption key to encrypt the fields 
        /// set with FieldsToEncrypt
        /// </summary>
        public virtual string EncryptionKey
        {
            get { return _EncryptionKey; }
            set { _EncryptionKey = value; }
        }
        private string _EncryptionKey = "x@3|zg?4%ui*";

        /// <summary>
        /// Optional Section name that can differentiate groups of config
        /// values in multi-section files like Config files.
        /// </summary>
        public string ConfigurationSection {get; set; }

        
        /// <summary>
        /// Reads a configurations settings from the configuration store
        /// into a new existing instance.
        /// </summary>
        /// <typeparam name="T">Specific Config Settings Class</typeparam>
        /// <returns></returns>
        public abstract T Read<T>()
                where T : AppConfiguration, new();


        /// <summary>
        /// Reads configuration settings from the store into a passed
        /// instance of the configuration instance.
        /// </summary>
        /// <param name="config">Specific config settings class instance</param>
        /// <returns>true or false</returns>
        public abstract bool Read(AppConfiguration config);

        /// <summary>
        /// Writes the configuration settings from a specific instance
        /// into the configuration store.
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        public abstract bool Write(AppConfiguration config);

        /// <summary>
        /// Creates a new instance of the application object and retrieves
        /// configuration information from the provided string. String 
        /// should be in XML Serialization format or created by the WriteAsString 
        /// method.
        /// </summary>
        /// <typeparam name="T">Type of the specific configuration class</typeparam>
        /// <param name="xml">An xml string that contains XML Serialized serialization data</param>
        /// <returns>The deserialized instance or null on failure</returns>
        public virtual T Read<T>(string xml)
            where T : AppConfiguration, new()
        {
            if (string.IsNullOrEmpty((xml)))
            {                
                return null;
            }

            T result;
            try
            {
                result = SerializationUtils.DeSerializeObject(xml, typeof(T)) as T;
            }
            catch (Exception ex)
            {
                SetError(ex);
                return null;
            }
            if (result != null)
                DecryptFields(result);

            return result;
        }
        /// <summary>
        /// Reads data into configuration from an XML string into a passed 
        /// instance of the a configuration object.
        /// </summary>
        /// <param name="config">An instance of a custom configuration object</param>
        /// <param name="xml">Xml of serialized configuration instance.</param>
        /// <returns>true or false</returns>
        public virtual bool Read(AppConfiguration config, string xml)
        {
            TAppConfiguration newConfig = null;

            // if no data was passed leave the object
            // in its initial state.
            if (string.IsNullOrEmpty(xml))
                return true;

            try
            {
                newConfig = SerializationUtils.DeSerializeObject(xml, config.GetType()) as TAppConfiguration;
                if (newConfig == null)
                {
                    SetError(Resources.ObjectCouldNotBeDeserializedFromXml);
                    return false;
                }
            }
            catch (Exception ex)
            {
                SetError(ex);
                return false;
            }
            if (newConfig != null)
            {
                DecryptFields(newConfig);
                DataUtils.CopyObjectData(newConfig, config, "Provider,ErrorMessage");
                return true;
            }
            return false;
        }


        /// <summary>
        /// Writes the current configuration information to an
        /// XML string. String is XML Serialization format.
        /// </summary>
        /// <returns>xml string of serialized config object</returns>
        public virtual string WriteAsString(AppConfiguration config)
        {
            string xml = string.Empty;
            EncryptFields(config);

            try
            {
                SerializationUtils.SerializeObject(config, out xml, true);
            }
            catch (Exception ex)
            {
                SetError(ex);
                return string.Empty;
            }
            finally
            {
                DecryptFields(config);
            }

            return xml;
        }


        /// <summary>
        /// Encrypts all the fields in the current object based on the EncryptFieldList
        /// </summary>
        /// <returns></returns>
        public virtual void EncryptFields(AppConfiguration config)
        {
            if (string.IsNullOrEmpty(PropertiesToEncrypt))
                return;

            string encryptFieldList = "," + PropertiesToEncrypt.ToLower() + ",";
            string[] fieldTokens = encryptFieldList.Split(new char[1] {','}, StringSplitOptions.RemoveEmptyEntries);            

            foreach(string fieldName in fieldTokens)
            {
                // Encrypt the field if in list
                if (encryptFieldList.Contains("," + fieldName.ToLower() + ","))
                {
                    object val = string.Empty;
                    try
                    {
                       val = ReflectionUtils.GetPropertyEx(config, fieldName);
                    }
                    catch
                    {
                        throw new ArgumentException(string.Format("{0}: {1}",Resources.InvalidEncryptionPropertyName,fieldName));
                    }

                    // only encrypt string values
                    var strVal = val as string;
                    if (string.IsNullOrEmpty(strVal))
                        continue;

                    val = Encryption.EncryptString(strVal, EncryptionKey);
                    try
                    {
                        ReflectionUtils.SetPropertyEx(config, fieldName, val);
                    }
                    catch
                    {
                        throw new ArgumentException(string.Format("{0}: {1}", Resources.InvalidEncryptionPropertyName, fieldName));
                    }
                }
            }
        }

        /// <summary>
        /// Internally decryptes all the fields in the current object based on the EncryptFieldList
        /// </summary>
        /// <returns></returns>
        public virtual void DecryptFields(AppConfiguration config)
        {
            if (string.IsNullOrEmpty(PropertiesToEncrypt))
                return;

            string encryptFieldList = "," + PropertiesToEncrypt.ToLower() + ",";
            string[] fieldTokens = encryptFieldList.Split(new char[1] { ',' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string fieldName in fieldTokens)
            {
                // Encrypt the field if in list
                if (encryptFieldList.Contains("," + fieldName.ToLower() + ","))
                {
                    object val = string.Empty;
                    try
                    {
                        val = ReflectionUtils.GetPropertyEx(config, fieldName);
                    }
                    catch
                    {
                        throw new ArgumentException(string.Format("{0}: {1}", Resources.InvalidEncryptionPropertyName, fieldName));
                    }

                    // only encrypt string values
                    var strVal = val as string;
                    if (string.IsNullOrEmpty(strVal))
                        continue;

                    val = Encryption.DecryptString(strVal, EncryptionKey);
                    try
                    {
                        ReflectionUtils.SetPropertyEx(config, fieldName, val);
                    }
                    catch
                    {
                        throw new ArgumentException(string.Format("{0}: {1}", Resources.InvalidEncryptionPropertyName, fieldName));
                    }
                }
            }
        }



        /// <summary>
        /// Sets an error message when an error occurs
        /// </summary>
        /// <param name="message"></param>
        protected virtual void SetError(string message)
        {
            if (string.IsNullOrEmpty(message))
            {
                ErrorMessage = string.Empty;
                return;
            }

            ErrorMessage = message;
        }

        /// <summary>
        /// Writes an exception and innerexception message
        /// into the error message text
        /// </summary>
        /// <param name="ex"></param>
        protected virtual void SetError(Exception ex)
        {
            string message = ex.Message;
            if (ex.InnerException != null)
                message += " " + ex.InnerException.Message;
            SetError(message);
        }

        /// <summary>
        /// Helper method to create a new instance of the Configuration object.        
        /// </summary>
        /// <returns></returns>
        protected TAppConfiguration CreateConfigurationInstance()
        {
            return Activator.CreateInstance(typeof(TAppConfiguration)) as TAppConfiguration;
        }
    }

}
