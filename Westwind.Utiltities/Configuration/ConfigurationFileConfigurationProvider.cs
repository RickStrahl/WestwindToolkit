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
using System.Collections.Specialized;
using System.Configuration;
using System.Reflection;
using System.Xml;
using System.Globalization;

namespace Westwind.Utilities.Configuration
{

    /// <summary>
    /// Reads and Writes configuration settings in .NET config files and 
    /// sections. Allows reading and writing to default or external files 
    /// and specification of the configuration section that settings are
    /// applied to.
    /// </summary>
    public class ConfigurationFileConfigurationProvider<TAppConfiguration> : ConfigurationProviderBase<TAppConfiguration>
        where TAppConfiguration: AppConfiguration, new()
    {

        /// <summary>
        /// Optional - the Configuration file where configuration settings are
        /// stored in. If not specified uses the default Configuration Manager
        /// and its default store.
        /// </summary>
        public string ConfigurationFile {get; set; }

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
            TAppConfig config = Activator.CreateInstance(typeof(TAppConfig), true) as TAppConfig;
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
                return Read(config,ConfigurationFile);

            Type typeWebConfig = config.GetType();
            MemberInfo[] Fields = typeWebConfig.GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty | BindingFlags.GetField);

            // Set a flag for missing fields
            // If we have any we'll need to write them out into .config
            bool missingFields = false;

            string fieldsToEncrypt = "," + PropertiesToEncrypt.ToLower() + ",";


            // Refresh the sections - req'd after write operations
            // sometimes sections don't want to re-read            
            if (string.IsNullOrEmpty(ConfigurationSection))
                ConfigurationManager.RefreshSection("appSettings");
            else
                ConfigurationManager.RefreshSection(ConfigurationSection);
                
            
            // Loop through all fields and properties                 
            foreach (MemberInfo Member in Fields)
            {
                string typeName = null;

                FieldInfo field = null;
                PropertyInfo property = null;
                Type fieldType = null;

                if (Member.MemberType == MemberTypes.Field)
                {
                    field = (FieldInfo)Member;
                    fieldType = field.FieldType;
                    typeName = fieldType.Name.ToLower();
                }
                else if (Member.MemberType == MemberTypes.Property)
                {
                    property = (PropertyInfo)Member;
                    fieldType = property.PropertyType;
                    typeName = fieldType.Name.ToLower();
                }
                else
                    continue;

                string fieldName = Member.Name.ToLower();

                // Error Message is an internal public property
                if (fieldName == "errormessage" || fieldName == "provider")
                    continue;

                string value = null;
                if (string.IsNullOrEmpty(ConfigurationSection))
                    value = ConfigurationManager.AppSettings[fieldName];
                else
                {
                    NameValueCollection Values =
                        (NameValueCollection) ConfigurationManager.GetSection(ConfigurationSection);
                    if (Values != null)
                        value = Values[fieldName];
                }

                if (value == null)
                {
                    missingFields = true;
                    continue;
                }

                // If we're encrypting decrypt any field that are encyrpted
                if (value != string.Empty && fieldsToEncrypt.IndexOf("," + fieldName + ",") > -1)
                    value = Encryption.DecryptString(value, EncryptionKey);

                try
                {
                    // Assign the value to the property
                    ReflectionUtils.SetPropertyEx(config, fieldName,
                        ReflectionUtils.StringToTypedValue(value, fieldType, CultureInfo.InvariantCulture));
                }
                catch { ;}
            }

            // We have to write any missing keys
            if (missingFields)
                Write(config);

            return true;
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
            MemberInfo[] Fields = typeWebConfig.GetMembers(BindingFlags.Public |
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

            string fieldsToEncrypt = "," + PropertiesToEncrypt.ToLower() + ",";

            foreach (MemberInfo Member in Fields)
            {
                FieldInfo Field = null;
                PropertyInfo Property = null;
                Type FieldType = null;
                string TypeName = null;

                if (Member.MemberType == MemberTypes.Field)
                {
                    Field = (FieldInfo)Member;
                    FieldType = Field.FieldType;
                    TypeName = Field.FieldType.Name.ToLower();
                }
                else if (Member.MemberType == MemberTypes.Property)
                {
                    Property = (PropertyInfo)Member;
                    FieldType = Property.PropertyType;
                    TypeName = Property.PropertyType.Name.ToLower();
                }
                else
                    continue;

                string Fieldname = Member.Name;
                if (Fieldname == "Provider" || Fieldname == "ErrorMessage")
                    continue;

                XmlNode Section = Dom.DocumentElement.SelectSingleNode(XmlNamespacePrefix + ConfigSection, XmlNamespaces);
                if (Section == null)
                {
                    Section = CreateConfigSection(Dom, ConfigurationSection);
                    Dom.DocumentElement.AppendChild(Section);
                }

                string Value = GetNamedValueFromXml(Dom, Fieldname, ConfigSection);
                if (Value == null)
                {
                    missingFields = true;
                    continue;
                }

                Fieldname = Fieldname.ToLower();

                // If we're encrypting decrypt any field that are encyrpted
                if (Value != string.Empty && fieldsToEncrypt.IndexOf("," + Fieldname + ",") > -1)
                    Value = Encryption.DecryptString(Value, EncryptionKey);

                // Assign the Property
                ReflectionUtils.SetPropertyEx(config, Fieldname,
                                     ReflectionUtils.StringToTypedValue(Value, FieldType, CultureInfo.InvariantCulture));
            }

            // We have to write any missing keys
            if (missingFields)
                Write(config);

            return true;
        }
        
        public override bool Write(AppConfiguration config)
        {
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
                MemberInfo[] Fields = typeWebConfig.GetMembers(BindingFlags.Instance | BindingFlags.GetField | BindingFlags.GetProperty | BindingFlags.Public);

                string fieldsToEncrypt = "," + PropertiesToEncrypt.ToLower() + ",";

                string ConfigSection = "appSettings";
                if (!string.IsNullOrEmpty(ConfigurationSection))
                    ConfigSection = ConfigurationSection;

                ConfigurationManager.RefreshSection(ConfigSection);

                foreach (MemberInfo Field in Fields)
                {
                    // If we can't find the key - write it out to the document
                    string Value = null;
                    object RawValue = null;
                    if (Field.MemberType == MemberTypes.Field)
                        RawValue = ((FieldInfo)Field).GetValue(config);
                    else if (Field.MemberType == MemberTypes.Property)
                        RawValue = ((PropertyInfo)Field).GetValue(config, null);
                    else
                        continue; // not a property or field

                    // Don't persist ErrorMessage property
                    if (Field.Name == "ErrorMessage" || Field.Name == "Provider")
                        continue;

                    Value = ReflectionUtils.TypedValueToString(RawValue, CultureInfo.InvariantCulture);

                    // Encrypt the field if in list
                    if (fieldsToEncrypt.IndexOf("," + Field.Name.ToLower() + ",") > -1)
                        Value = Encryption.EncryptString(Value, EncryptionKey);

                    XmlNode Node = Dom.DocumentElement.SelectSingleNode(
                        XmlNamespacePrefix + ConfigSection + "/" +
                        XmlNamespacePrefix + "add[@key='" + Field.Name + "']", XmlNamespaces);

                    if (Node == null)
                    {
                        // Create the node and attributes and write it
                        Node = Dom.CreateNode(XmlNodeType.Element, "add", Dom.DocumentElement.NamespaceURI);

                        XmlAttribute Attr2 = Dom.CreateAttribute("key");
                        Attr2.Value = Field.Name;
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


                    string XML = Node.OuterXml;

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
            }
            return true;
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


    }
}
