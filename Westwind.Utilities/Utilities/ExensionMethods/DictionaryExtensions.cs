using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Xml.Linq;
using Westwind.Utilities;
using System.Collections;
using System.Collections.Specialized;
using System.Xml.Serialization;
using System.Xml;

namespace Westwind.Utilities.Extensions
{
    /// <summary>
    /// Extends dictionary classes with XML 
    /// </summary>
    public static class DictionaryExtensions
    {

        /// <summary>
        /// Serializes the dictionary to an XML string
        /// </summary>
        /// <returns></returns>
        public static string ToXml(this IDictionary items, string root = "root")
        {
            var rootNode = new XElement(root);

            foreach (DictionaryEntry item in items)
            {
                string xmlType = XmlUtils.MapTypeToXmlType(item.Value.GetType());
                XAttribute typeAttr = null;

                // if it's a simple type use it
                if (!string.IsNullOrEmpty(xmlType))
                {
                    typeAttr = new XAttribute("type", xmlType);
                    rootNode.Add(
                        new XElement(item.Key as string,
                                     typeAttr,
                                     item.Value)
                        );
                }
                else
                {
                    // complex type use serialization
                    string xmlString = null;
                    if (SerializationUtils.SerializeObject(item.Value, out xmlString))
                    {
                        XElement el = XElement.Parse(xmlString);

                        rootNode.Add(
                            new XElement(item.Key as string,
                            new XAttribute("type", "___" + item.Value.GetType().FullName),
                            el));
                    }
                }
            }

            return rootNode.ToString();
        }

        /// <summary>
        /// Loads the dictionary from an Xml string
        /// </summary>
        /// <param name="xml"></param>
        public static void FromXml(this IDictionary items, string xml)
        {
            items.Clear();

            var root = XElement.Parse(xml);

            foreach (XElement el in root.Elements())
            {
                string typeString = null;

                var typeAttr = el.Attribute("type");
                if (typeAttr != null)
                    typeString = typeAttr.Value;

                string val = el.Value;


                if (!string.IsNullOrEmpty(typeString) && typeString != "string" && !typeString.StartsWith("__"))
                {
                    // Simple type we know how to convert
                    Type type = XmlUtils.MapXmlTypeToType(typeString);
                    if (type != null)
                        items.Add(el.Name.LocalName, ReflectionUtils.StringToTypedValue(val, type));
                    else
                        items.Add(el.Name.LocalName, val);
                }
                else if (typeString.StartsWith("___"))
                {
                    Type type = ReflectionUtils.GetTypeFromName(typeString.Substring(3));
                    object serializationUtilsDeSerializeObject = SerializationUtils.DeSerializeObject(el.Elements().First().CreateReader(), type);
                    items.Add(el.Name.LocalName, serializationUtilsDeSerializeObject);
                }
                else
                    // it's a string or unknown type
                    items.Add(el.Name.LocalName, val);
            }
        }
    }
}
