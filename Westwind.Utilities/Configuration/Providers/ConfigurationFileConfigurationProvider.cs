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
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Xml;
using System.Globalization;
using Westwind.Utilities.Properties;

namespace Westwind.Utilities.Configuration
{

    /// <summary>
    /// Reads and Writes configuration settings in .NET config files and 
    /// sections. Allows reading and writing to default or external files 
    /// and specification of the configuration section that settings are
    /// applied to.
    /// </summary>
    public class ConfigurationFileConfigurationProvider<TAppConfiguration> :
        ConfigurationProviderBase<TAppConfiguration>
        where TAppConfiguration : AppConfiguration, new()
    {

        /// <summary>
        /// Optional - the Configuration file where configuration settings are
        /// stored in. If not specified uses the default Configuration Manager
        /// and its default store.
        /// </summary>
        public string ConfigurationFile { get; set; }

        /// <summary>
        /// Optional The Configuration section where settings are stored.
        /// If not specified the appSettings section is used.
        /// </summary>
        //public new string ConfigurationSection {get; set; }


        /// <summary>
        /// internal property used to ensure there are no multiple write
        /// operations at the same time
        /// </summary>
        private object syncWriteLock = new object();

        /// <summary>
        /// Internally used reference to the Namespace Manager object
        /// used to make sure we're searching the proper Namespace
        /// for the appSettings section when reading and writing manually
        /// </summary>
        private XmlNamespaceManager XmlNamespaces = null;

        //Internally used namespace prefix for the default namespace
        private string XmlNamespacePrefix = "ww:";


        /// <summary>
        /// Reads configuration settings into a new instance of the configuration object.
        /// </summary>
        /// <typeparam name="TAppConfig"></typeparam>
        /// <returns></returns>
        public override TAppConfig Read<TAppConfig>()
        {
            TAppConfig config = Activator.CreateInstance(typeof (TAppConfig), true) as TAppConfig;
            if (!Read(config))
                return null;

            return config;
        }

        /// <summary>
        /// Reads configuration settings from the current configuration manager. 
        /// Uses the internal APIs to write these values.
        /// </summary>
        /// <typeparam name="TAppConfiguration"></typeparam>
        /// <param name="config"></param>
        /// <returns></returns>
        public override bool Read(AppConfiguration config)
        {
            // Config reading from external files works a bit differently 
            // so use a separate method to handle it
            if (!string.IsNullOrEmpty(ConfigurationFile))
                return Read(config, ConfigurationFile);

            Type typeWebConfig = config.GetType();
            PropertyInfo[] properties =
                typeWebConfig.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty);

            // Set a flag for missing fields
            // If we have any we'll need to write them out into .config
            bool missingFields = false;

            // Refresh the sections - req'd after write operations
            // sometimes sections don't want to re-read            
            if (string.IsNullOrEmpty(ConfigurationSection))
                ConfigurationManager.RefreshSection("appSettings");
            else
                ConfigurationManager.RefreshSection(ConfigurationSection);

            NameValueCollection configManager;

            configManager = string.IsNullOrEmpty(ConfigurationSection)
                ? ConfigurationManager.AppSettings as NameValueCollection
                : ConfigurationManager.GetSection(ConfigurationSection) as NameValueCollection;            

            if (configManager == null)
            {
                Write(config);
                return true;
            }


            // Loop through all fields and properties                 
            foreach (PropertyInfo property in properties)
            {
                Type fieldType = property.PropertyType;                
                string fieldName = property.Name.ToLowerInvariant();

                // Error Message is an internal public property
                if (fieldName == "errormessage" || fieldName == "provider")
                    continue;

                if (!IsIList(fieldType))
                {
                    // Single value
                    string value = configManager[fieldName];

                    if (value == null)
                    {
                        missingFields = true;
                        continue;
                    }

                    try
                    {
                        // Assign the value to the property
                        ReflectionUtils.SetPropertyEx(config,property.Name,
                            StringToTypedValue(value, fieldType, CultureInfo.InvariantCulture));
                    }
                    catch
                    {
                    }
                }
                else
                {
                    // List Value
                    var list = Activator.CreateInstance(fieldType) as IList;
                    var elType = fieldType.GetElementType();
                    if (elType == null)
                    {
                        var generic = fieldType.GetGenericArguments();
                        if (generic != null && generic.Length > 0)
                            elType = generic[0];
                    }

                    int count = 1;
                    string value = string.Empty;

                    while (value != null)
                    {
                        value = configManager[fieldName + count];                        
                        if (value == null)
                            break;
                        list.Add(StringToTypedValue(value, elType, CultureInfo.InvariantCulture));
                        count++;
                    }

                    try
                    {
                        ReflectionUtils.SetPropertyEx(config, property.Name, list);
                    }
                    catch { }
                }
            }

            DecryptFields(config);

            // We have to write any missing keys
            if (missingFields)
                Write(config);

            return true;
        }




        bool IsIList(Type type)
        {            
            // Enumerable types explicitly supported as 'simple values'
            if (type == typeof(string) || type == typeof( byte[]) )
                return false;

            if (type.GetInterface("IList") != null)
                return true;

            return false;
        }




        /// <summary>
        /// Reads Configuration settings from an external file or explicitly from a file.
        /// Uses XML DOM to read values instead of using the native APIs.
        /// </summary>
        /// <typeparam name="TAppConfiguration"></typeparam>
        /// <param name="config">Configuration instance</param>
        /// <param name="filename">Filename to read from</param>
        /// <returns></returns>
        public override bool Read(AppConfiguration config, string filename)
        {
            Type typeWebConfig = config.GetType();
            PropertyInfo[] properties = typeWebConfig.GetProperties(BindingFlags.Public |
                                                           BindingFlags.Instance);

            // Set a flag for missing fields
            // If we have any we'll need to write them out 
            bool missingFields = false;

            XmlDocument Dom = new XmlDocument();

            try
            {
                Dom.Load(filename);
            }
            catch
            {
                // Can't open or doesn't exist - so try to create it
                if (!Write(config))
                    return false;

                // Now load again
                Dom.Load(filename);
            }

            // Retrieve XML Namespace information to assign default 
            // Namespace explicitly.
            GetXmlNamespaceInfo(Dom);


            string ConfigSection = ConfigurationSection;
            if (ConfigSection == string.Empty)
                ConfigSection = "appSettings";
            

            foreach (var property in properties)
            {                
                Type fieldType = null;
                string typeName = null;
    
                fieldType = property.PropertyType;
                typeName = property.PropertyType.Name.ToLower();
                
                string propertyName = property.Name;
                if (propertyName == "Provider" || propertyName == "ErrorMessage")
                    continue;

                XmlNode Section = Dom.DocumentElement.SelectSingleNode(XmlNamespacePrefix + ConfigSection, XmlNamespaces);
                if (Section == null)
                {
                    Section = CreateConfigSection(Dom, ConfigurationSection);
                    Dom.DocumentElement.AppendChild(Section);
                }

                string Value = GetNamedValueFromXml(Dom, propertyName, ConfigSection);
                if (Value == null)
                {
                    missingFields = true;
                    continue;
                }

                // Assign the Property
                ReflectionUtils.SetPropertyEx(config, propertyName,
                    StringToTypedValue(Value, fieldType, CultureInfo.InvariantCulture));
            }

            DecryptFields(config);

            // We have to write any missing keys
            if (missingFields)
                Write(config);

            return true;
        }


        public override bool Write(AppConfiguration config)
        {
            EncryptFields(config);

            lock (syncWriteLock)
            {
                // Load the config file into DOM parser
                XmlDocument Dom = new XmlDocument();

                string configFile = ConfigurationFile;
                if (string.IsNullOrEmpty(configFile))
                    configFile = AppDomain.CurrentDomain.SetupInformation.ConfigurationFile;

                try
                {
                    Dom.Load(configFile);
                }
                catch
                {
                    // Can't load the file - create an empty document
                    string Xml =
                   @"<?xml version='1.0'?>
		<configuration>
		</configuration>";

                    Dom.LoadXml(Xml);
                }

                // Load up the Namespaces object so we can 
                // reference the appropriate default namespace
                GetXmlNamespaceInfo(Dom);

                // Parse through each of hte properties of the properties
                Type typeWebConfig = config.GetType();
                PropertyInfo[] properties = typeWebConfig.GetProperties(BindingFlags.Instance | BindingFlags.GetProperty | BindingFlags.Public);
                
                string ConfigSection = "appSettings";
                if (!string.IsNullOrEmpty(ConfigurationSection))
                    ConfigSection = ConfigurationSection;

                // make sure we're getting the latest values before we write
                ConfigurationManager.RefreshSection(ConfigSection);

                foreach (var property in properties)
                {
                    // Don't persist ErrorMessage property
                    if (property.Name == "ErrorMessage" || property.Name == "Provider")
                        continue;

                    object rawValue = null;
                    rawValue = property.GetValue(config, null);
                    
                    string value = TypedValueToString(rawValue, CultureInfo.InvariantCulture);

                    if (value == "ILIST_TYPE")
                    {
                        var count = 0;
                        foreach (var item in rawValue as IList)
                        {
                            value = TypedValueToString(item, CultureInfo.InvariantCulture);
                            WriteConfigurationValue(property.Name + ++count, value, property, Dom, ConfigSection);
                        }
                    }
                    else
                    {
                            WriteConfigurationValue(property.Name, value, property, Dom, ConfigSection);
                    }
                } // for each

                try
                {
                    // this will fail if permissions are not there
                    Dom.Save(configFile);

                    ConfigurationManager.RefreshSection(ConfigSection);
                }
                catch
                {
                    return false;
                }
                finally
                {
                    DecryptFields(config);
                }
            }
            return true;
        }

        private void WriteConfigurationValue(string keyName, string Value, MemberInfo Field, XmlDocument Dom, string ConfigSection)
        {
            XmlNode Node = Dom.DocumentElement.SelectSingleNode(
                XmlNamespacePrefix + ConfigSection + "/" +
                XmlNamespacePrefix + "add[@key='" + keyName + "']", XmlNamespaces);

            if (Node == null)
            {
                // Create the node and attributes and write it
                Node = Dom.CreateNode(XmlNodeType.Element, "add", Dom.DocumentElement.NamespaceURI);

                XmlAttribute Attr2 = Dom.CreateAttribute("key");
                Attr2.Value = keyName;
                XmlAttribute Attr = Dom.CreateAttribute("value");
                Attr.Value = Value;

                Node.Attributes.Append(Attr2);
                Node.Attributes.Append(Attr);

                XmlNode Parent = Dom.DocumentElement.SelectSingleNode(
                    XmlNamespacePrefix + ConfigSection, XmlNamespaces);

                if (Parent == null)
                    Parent = CreateConfigSection(Dom, ConfigSection);

                Parent.AppendChild(Node);
            }
            else
            {
                // just write the value into the attribute
                Node.Attributes.GetNamedItem("value").Value = Value;
            }
        }

        /// <summary>
        /// Returns a single value from the XML in a configuration file.
        /// </summary>
        /// <param name="Dom"></param>
        /// <param name="Key"></param>
        /// <param name="ConfigSection"></param>
        /// <returns></returns>
        protected string GetNamedValueFromXml(XmlDocument Dom, string Key, string ConfigSection)
        {
            XmlNode Node = Dom.DocumentElement.SelectSingleNode(
                   XmlNamespacePrefix + ConfigSection + @"/" +
                   XmlNamespacePrefix + "add[@key='" + Key + "']", XmlNamespaces);

            if (Node == null)
                return null;

            return Node.Attributes["value"].Value;
        }

        /// <summary>
        /// Used to load up the default namespace reference and prefix
        /// information. This is required so that SelectSingleNode can
        /// find info in 2.0 or later config files that include a namespace
        /// on the root element definition.
        /// </summary>
        /// <param name="Dom"></param>
        protected void GetXmlNamespaceInfo(XmlDocument Dom)
        {
            // Load up the Namespaces object so we can 
            // reference the appropriate default namespace
            if (Dom.DocumentElement.NamespaceURI == null || Dom.DocumentElement.NamespaceURI == string.Empty)
            {
                XmlNamespaces = null;
                XmlNamespacePrefix = string.Empty;
            }
            else
            {
                if (Dom.DocumentElement.Prefix == null || Dom.DocumentElement.Prefix == string.Empty)
                    XmlNamespacePrefix = "ww";
                else
                    XmlNamespacePrefix = Dom.DocumentElement.Prefix;

                XmlNamespaces = new XmlNamespaceManager(Dom.NameTable);
                XmlNamespaces.AddNamespace(XmlNamespacePrefix, Dom.DocumentElement.NamespaceURI);

                XmlNamespacePrefix += ":";
            }
        }

        /// <summary>
        /// Creates a Configuration section and also creates a ConfigSections section for new 
        /// non appSettings sections.
        /// </summary>
        /// <param name="dom"></param>
        /// <param name="configSection"></param>
        /// <returns></returns>
        private XmlNode CreateConfigSection(XmlDocument dom, string configSection)
        {

            // Create the actual section first and attach to document
            XmlNode AppSettingsNode = dom.CreateNode(XmlNodeType.Element,
                configSection, dom.DocumentElement.NamespaceURI);

            XmlNode Parent = dom.DocumentElement.AppendChild(AppSettingsNode);

            // Now check and make sure that the section header exists
            if (configSection != "appSettings")
            {
                XmlNode ConfigSectionHeader = dom.DocumentElement.SelectSingleNode(XmlNamespacePrefix + "configSections",
                                XmlNamespaces);
                if (ConfigSectionHeader == null)
                {
                    // Create the node and attributes and write it
                    XmlNode ConfigSectionNode = dom.CreateNode(XmlNodeType.Element,
                             "configSections", dom.DocumentElement.NamespaceURI);

                    // Insert as first element in DOM
                    ConfigSectionHeader = dom.DocumentElement.InsertBefore(ConfigSectionNode,
                             dom.DocumentElement.ChildNodes[0]);
                }

                // Check for the Section
                XmlNode Section = ConfigSectionHeader.SelectSingleNode(XmlNamespacePrefix + "section[@name='" + configSection + "']",
                        XmlNamespaces);

                if (Section == null)
                {
                    Section = dom.CreateNode(XmlNodeType.Element, "section",
                             null);

                    XmlAttribute Attr = dom.CreateAttribute("name");
                    Attr.Value = configSection;
                    XmlAttribute Attr2 = dom.CreateAttribute("type");
                    Attr2.Value = "System.Configuration.NameValueSectionHandler,System,Version=1.0.3300.0, Culture=neutral, PublicKeyToken=b77a5c561934e089";
                    XmlAttribute Attr3 = dom.CreateAttribute("requirePermission");
                    Attr3.Value = "false";
                    Section.Attributes.Append(Attr);
                    Section.Attributes.Append(Attr3);
                    Section.Attributes.Append(Attr2);
                    ConfigSectionHeader.AppendChild(Section);
                }
            }

            return Parent;
        }


        /// <summary>
        /// Converts a type to string if possible. This method supports an optional culture generically on any value.
        /// It calls the ToString() method on common types and uses a type converter on all other objects
        /// if available
        /// </summary>
        /// <param name="rawValue">The Value or Object to convert to a string</param>
        /// <param name="culture">Culture for numeric and DateTime values</param>
        /// <param name="unsupportedReturn">Return string for unsupported types</param>
        /// <returns>string</returns>
        static string TypedValueToString(object rawValue, CultureInfo culture = null, string unsupportedReturn = null)
        {
            if (rawValue == null)
                return string.Empty;

            if (culture == null)
                culture = CultureInfo.CurrentCulture;

            Type valueType = rawValue.GetType();
            string returnValue = null;

            if (valueType == typeof(string))
                returnValue = rawValue as string;
            else if (valueType == typeof(int) || valueType == typeof(decimal) ||
                valueType == typeof(double) || valueType == typeof(float) || valueType == typeof(Single))
                returnValue = string.Format(culture.NumberFormat, "{0}", rawValue);
            else if (valueType == typeof(DateTime))
                returnValue = string.Format(culture.DateTimeFormat, "{0}", rawValue);
            else if (valueType == typeof(bool) || valueType == typeof(Byte) || valueType.IsEnum)
                returnValue = rawValue.ToString();
            else if (valueType == typeof (byte[]))
                returnValue = Convert.ToBase64String(rawValue as byte[]);
            else if (valueType == typeof(Guid?))
            {
                if (rawValue == null)
                    returnValue = string.Empty;
                else
                    return rawValue.ToString();
            }
            else if (rawValue is IList)
                return "ILIST_TYPE";
            else
            {
                // Any type that supports a type converter
                TypeConverter converter = TypeDescriptor.GetConverter(valueType);
                if (converter != null && converter.CanConvertTo(typeof(string)) && converter.CanConvertFrom(typeof(string)))
                    returnValue = converter.ConvertToString(null, culture, rawValue);
                else
                {
                    // Last resort - just call ToString() on unknown type
                    if (!string.IsNullOrEmpty(unsupportedReturn))
                        returnValue = unsupportedReturn;
                    else
                        returnValue = rawValue.ToString();
                }
            }

            return returnValue;
        }

        /// <summary>
        /// Turns a string into a typed value generically.
        /// Explicitly assigns common types and falls back
        /// on using type converters for unhandled types.         
        /// 
        /// Common uses: 
        /// * UI -&gt; to data conversions
        /// * Parsers
        /// <seealso>Class ReflectionUtils</seealso>
        /// </summary>
        /// <param name="sourceString">
        /// The string to convert from
        /// </param>
        /// <param name="targetType">
        /// The type to convert to
        /// </param>
        /// <param name="culture">
        /// Culture used for numeric and datetime values.
        /// </param>
        /// <returns>object. Throws exception if it cannot be converted.</returns>
        static object StringToTypedValue(string sourceString, Type targetType, CultureInfo culture = null)
        {
            object result = null;

            bool isEmpty = string.IsNullOrEmpty(sourceString);

            if (culture == null)
                culture = CultureInfo.CurrentCulture;

            if (targetType == typeof(string))
                result = sourceString;
            else if (targetType == typeof(Int32) || targetType == typeof(int))
            {
                if (isEmpty)
                    result = 0;
                else
                    result = Int32.Parse(sourceString, NumberStyles.Any, culture.NumberFormat);
            }
            else if (targetType == typeof(Int64))
            {
                if (isEmpty)
                    result = (Int64)0;
                else
                    result = Int64.Parse(sourceString, NumberStyles.Any, culture.NumberFormat);
            }
            else if (targetType == typeof(Int16))
            {
                if (isEmpty)
                    result = (Int16)0;
                else
                    result = Int16.Parse(sourceString, NumberStyles.Any, culture.NumberFormat);
            }
            else if (targetType == typeof(decimal))
            {
                if (isEmpty)
                    result = 0M;
                else
                    result = decimal.Parse(sourceString, NumberStyles.Any, culture.NumberFormat);
            }
            else if (targetType == typeof(DateTime))
            {
                if (isEmpty)
                    result = DateTime.MinValue;
                else
                    result = Convert.ToDateTime(sourceString, culture.DateTimeFormat);
            }
            else if (targetType == typeof(byte))
            {
                if (isEmpty)
                    result = 0;
                else
                    result = Convert.ToByte(sourceString);
            }
            else if (targetType == typeof(double))
            {
                if (isEmpty)
                    result = 0F;
                else
                    result = Double.Parse(sourceString, NumberStyles.Any, culture.NumberFormat);
            }
            else if (targetType == typeof(Single))
            {
                if (isEmpty)
                    result = 0F;
                else
                    result = Single.Parse(sourceString, NumberStyles.Any, culture.NumberFormat);
            }
            else if (targetType == typeof(bool))
            {
                if (!isEmpty &&
                    sourceString.ToLower() == "true" || sourceString.ToLower() == "on" || sourceString == "1")
                    result = true;
                else
                    result = false;
            }
            else if (targetType == typeof(Guid))
            {
                if (isEmpty)
                    result = Guid.Empty;
                else
                    result = new Guid(sourceString);
            }
            else if (targetType.IsEnum)
                result = Enum.Parse(targetType, sourceString);
            else if (targetType == typeof (byte[]))
                result = Convert.FromBase64String(sourceString);
            else if (targetType.Name.StartsWith("Nullable`"))
            {
                if (sourceString.ToLower() == "null" || sourceString == string.Empty)
                    result = null;
                else
                {
                    targetType = Nullable.GetUnderlyingType(targetType);
                    result = StringToTypedValue(sourceString, targetType);
                }
            }
            else
            {
                // Check for TypeConverters or FromString static method
                TypeConverter converter = TypeDescriptor.GetConverter(targetType);
                if (converter != null && converter.CanConvertFrom(typeof (string)))
                    result = converter.ConvertFromString(null, culture, sourceString);
                else
                {
                    // Try to invoke a static FromString method if it exists
                    try
                    {
                        var mi = targetType.GetMethod("FromString");
                        if (mi != null)
                        {
                            return mi.Invoke(null, new object[1] {sourceString});
                        }
                    }
                    catch
                    {
                        // ignore error and assume not supported 
                    }

                    Debug.Assert(false, string.Format("Type Conversion not handled in StringToTypedValue for {0} {1}",
                        targetType.Name, sourceString));
                    throw (new InvalidCastException(Resources.StringToTypedValueValueTypeConversionFailed +
                                                    targetType.Name));
                }
            }

            return result;
        }

    }


}
