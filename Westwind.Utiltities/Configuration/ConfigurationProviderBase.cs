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
            T result = null;
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

            try
            {
                newConfig = SerializationUtils.DeSerializeObject(xml, config.GetType(), false, true) as TAppConfiguration;
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

            MemberInfo[] mi = config.GetType().FindMembers(MemberTypes.Property | MemberTypes.Field,
               ReflectionUtils.MemberAccess, null, null);

            string encryptFieldList = "," + PropertiesToEncrypt.ToLower() + ",";
            foreach (MemberInfo Member in mi)
            {
                string FieldName = Member.Name.ToLower();

                // Encrypt the field if in list
                if (encryptFieldList.Contains("," + FieldName + ","))
                {
                    object val = string.Empty;

                    if (Member.MemberType == MemberTypes.Field)
                        val = ((FieldInfo)Member).GetValue(config);
                    else
                        val = ((PropertyInfo)Member).GetValue(config, null);

                    if (val == null || !(val is string))
                        continue;

                    var strVal = val as string;
                    if (string.IsNullOrEmpty(strVal))
                        continue;

                    val = Encryption.EncryptString(strVal, EncryptionKey);

                    if (Member.MemberType == MemberTypes.Field)
                        ((FieldInfo)Member).SetValue(config, val);
                    else
                        ((PropertyInfo)Member).SetValue(config, val, null);
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

            MemberInfo[] mi = config.GetType().FindMembers(MemberTypes.Property | MemberTypes.Field,
               ReflectionUtils.MemberAccess, null, null);

            string encryptFieldList = "," + PropertiesToEncrypt.ToLower() + ",";

            foreach (MemberInfo Member in mi)
            {
                string FieldName = Member.Name.ToLower();

                // Encrypt the field if in list
                if (encryptFieldList.IndexOf("," + FieldName + ",") > -1)
                {
                    object Value = string.Empty;

                    if (Member.MemberType == MemberTypes.Field)
                        Value = ((FieldInfo)Member).GetValue(config);
                    else
                        Value = ((PropertyInfo)Member).GetValue(config, null);

                    Value = Encryption.DecryptString((string)Value, EncryptionKey);

                    if (Member.MemberType == MemberTypes.Field)
                        ((FieldInfo)Member).SetValue(config, Value);
                    else
                        ((PropertyInfo)Member).SetValue(config, Value, null);

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
