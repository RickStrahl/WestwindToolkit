#region License
/*
 **************************************************************
 *  Author: Rick Strahl 
 *          © West Wind Technologies, 2008 - 2009
 *          http://www.west-wind.com/
 * 
 * Created: 09/08/2008
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
using System.Xml;
using Westwind.Utilities.Properties;


namespace Westwind.Utilities
{   
    /// <summary>
    /// String utility class that provides a host of string related operations
    /// </summary>
    public static class XmlUtils
    {
        /// <summary>
        /// Retrieves a result string from an XPATH query. Null if not found.
        /// </summary>
        /// <param name="node"></param>
        /// <param name="XPath"></param>
        /// <returns></returns>
        public static string GetXmlString(XmlNode node, string XPath, XmlNamespaceManager ns)
        {
            XmlNode selNode = node.SelectSingleNode(XPath,ns);
            if (selNode == null)
                return null;

            return selNode.InnerText;
        }

        /// <summary>
        /// Retrieves a result int value from an XPATH query. 0 if not found.
        /// </summary>
        /// <param name="node"></param>
        /// <param name="XPath"></param>
        /// <returns></returns>
        public static int GetXmlInt(XmlNode node, string XPath, XmlNamespaceManager ns)
        {
            string val = GetXmlString(node, XPath, ns);
            if (val == null)
                return 0;

            int result = 0;
            int.TryParse(val, out result);

            return result;
        }

        /// <summary>
        /// Retrieves a result bool from an XPATH query. false if not found.
        /// </summary>
        /// <param name="node"></param>
        /// <param name="XPath"></param>
        /// <returns></returns>
        public static bool GetXmlBool(XmlNode node, string XPath,XmlNamespaceManager ns)
        {
            string val = GetXmlString(node, XPath, ns);
            if (val == null)
                return false;

            if (val == "1" || val == "true" || val == "True")
                return true;

            return false;
        }

        /// <summary>
        /// Retrieves a result DateTime from an XPATH query. 1/1/1900  if not found.
        /// </summary>
        /// <param name="node"></param>
        /// <param name="XPath"></param>
        /// <returns></returns>
        public static DateTime GetXmlDateTime(XmlNode node, string XPath, XmlNamespaceManager ns)
        {
            DateTime dtVal = new DateTime(1900, 1, 1, 0, 0, 0);

            string val = GetXmlString(node, XPath, ns);
            if (val == null)
                return dtVal;

            try
            {
                dtVal = XmlConvert.ToDateTime(val,XmlDateTimeSerializationMode.Utc);
            }
            catch { }

            return dtVal;
        }

        /// <summary>
        /// Gets an attribute by name
        /// </summary>
        /// <param name="node"></param>
        /// <param name="attributeName"></param>
        /// <returns>value or null if not available</returns>
        public static string GetXmlAttributeString(XmlNode node, string attributeName)
        {
            XmlAttribute att = node.Attributes[attributeName];
            if (att == null)
                return null;

            return att.InnerText;
        }

        /// <summary>
        /// Returns an integer value from an attribute
        /// </summary>
        /// <param name="node"></param>
        /// <param name="attributeName"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static int GetXmlAttributeInt(XmlNode node, string attributeName, int defaultValue)
        {
            string val = GetXmlAttributeString(node, attributeName);
            if (val == null)
                return defaultValue;

            return XmlConvert.ToInt32(val);
        }

        

        /// <summary>
        /// Converts a .NET type into an XML compatible type - roughly
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string MapTypeToXmlType(Type type)
        {
            if (type == null)
                return null;

            if (type == typeof(string) || type == typeof(char) )
                return "string";
            if (type == typeof(int) || type== typeof(Int32) )
                return "integer";
            if (type == typeof(Int16) || type == typeof(byte) )
                return "short";
            if (type == typeof(long) || type == typeof(Int64) )
                return "long";
            if (type == typeof(bool))
                return "boolean";
            if (type == typeof(DateTime))
                return "datetime";
            
            if (type == typeof(float))
                return "float";
            if (type == typeof(decimal))
                return "decimal";
            if (type == typeof(double))
                return "double";
            if (type == typeof(Single))
                return "single";

            if (type == typeof(byte))
                return "byte";

            if (type == typeof(byte[]))
                return "base64Binary";        

            return null;

            // *** hope for the best
            //return type.ToString().ToLower();
        }


        public static Type MapXmlTypeToType(string xmlType)
        {
            xmlType = xmlType.ToLower();

            if (xmlType == "string")
                return typeof(string);
            if (xmlType == "integer")
                return typeof(int);
            if (xmlType == "long")
                return typeof(long);
            if (xmlType == "boolean")
                return typeof(bool);
            if (xmlType == "datetime")
                return typeof(DateTime);
            if (xmlType == "float")
                return typeof(float);
            if (xmlType == "decimal")
                return typeof(decimal);
            if (xmlType == "double")
                return typeof(Double);
            if (xmlType == "single")
                return typeof(Single);
               
            if (xmlType == "byte")
                return typeof(byte);                
            if (xmlType == "base64binary")
                return typeof(byte[]);
      

            // return null if no match is found
            // don't throw so the caller can decide more efficiently what to do 
            // with this error result
            return null;
        }

        
        /// <summary>
        /// Creates an Xml NamespaceManager for an XML document by looking
        /// at all of the namespaces defined on the document root element.
        /// </summary>
        /// <param name="doc">The XmlDom instance to attach the namespacemanager to</param>
        /// <param name="defaultNamespace">The prefix to use for prefix-less nodes (which are not supported if any namespaces are used in XmlDoc).</param>
        /// <returns></returns>
        public static XmlNamespaceManager CreateXmlNamespaceManager(XmlDocument doc, string defaultNamespace)
        {
            XmlNamespaceManager nsmgr = new XmlNamespaceManager(doc.NameTable);
            foreach (XmlAttribute attr in doc.DocumentElement.Attributes)
            {
                if (attr.Prefix == "xmlns")
                    nsmgr.AddNamespace(attr.LocalName, attr.Value);
                if (attr.Name == "xmlns")
                    // default namespace MUST use a prefix
                    nsmgr.AddNamespace(defaultNamespace, attr.Value);
            }

            return nsmgr;
        }

    }
}