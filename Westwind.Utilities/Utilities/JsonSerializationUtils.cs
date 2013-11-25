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
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Diagnostics;

namespace Westwind.Utilities
{

    /// <summary>
    /// JSON Serialization helper class that uses JSON.NET.
    /// This class serializes JSON to and from string and 
    /// files on disk.
    /// </summary>
    /// <remarks>
    /// JSON.NET is loaded dynamically at runtime to avoid hard 
    /// linking the Newtonsoft.Json dll. Make sure that your
    /// project includes a reference to JSON.NET when using this
    /// class.
    /// </remarks>
    public static class JsonSerializationUtils
    {
        static dynamic JsonNet = null;
        static object SyncLock = new Object();
        static Type FormattingType = null;
        static Type JsonTextReaderType = null;
        static Type JsonTextWriterType = null;


        /// <summary>
        /// Serializes an object to an XML string. Unlike the other SerializeObject overloads
        /// this methods *returns a string* rather than a bool result!
        /// </summary>
        /// <param name="value">Value to serialize</param>
        /// <param name="throwExceptions">Determines if a failure throws or returns null</param>
        /// <returns>
        /// null on error otherwise the Xml String.         
        /// </returns>
        /// <remarks>
        /// If null is passed in null is also returned so you might want
        /// to check for null before calling this method.
        /// </remarks>
        public static string Serialize(object value, bool throwExceptions = false, bool formatJsonOutput = false)
        {
            string jsonResult = null;
            Type type = value.GetType();
            dynamic writer = null;
            try
            {
                dynamic json = CreateJsonNet(throwExceptions);

                StringWriter sw = new StringWriter();

                writer = Activator.CreateInstance(JsonTextWriterType, sw);

                if (formatJsonOutput)
                    writer.Formatting = (dynamic)Enum.Parse(FormattingType, "Indented");

                writer.QuoteChar = '"';
                json.Serialize(writer, value);

                jsonResult = sw.ToString();
                writer.Close();
            }
            catch (Exception ex)
            {
                if (throwExceptions)
                    throw ex;

                jsonResult = null;
            }
            finally
            {
                if (writer != null)
                    writer.Close();
            }

            return jsonResult;
        }

        /// <summary>
        /// Serializes an object instance to a JSON file.
        /// </summary>
        /// <param name="value">the value to serialize</param>
        /// <param name="fileName">Full path to the file to write out with JSON.</param>
        /// <param name="throwExceptions">Determines whether exceptions are thrown or false is returned</param>
        /// <param name="formatJsonOutput">if true pretty-formats the JSON with line breaks</param>
        /// <returns>true or false</returns>        
        public static bool SerializeToFile(object value, string fileName, bool throwExceptions = false, bool formatJsonOutput = false)
        {
            dynamic writer = null;
            FileStream fs = null;
            try
            {
                Type type = value.GetType();

                var json = CreateJsonNet(throwExceptions);
                if (json == null)
                    return false;

                fs = new FileStream(fileName, FileMode.Create);
                var sw = new StreamWriter(fs, Encoding.UTF8);

                writer = Activator.CreateInstance(JsonTextWriterType, sw);
                if (formatJsonOutput)
                    writer.Formatting = (dynamic)Enum.Parse(FormattingType, "Indented");

                writer.QuoteChar = '"';
                json.Serialize(writer, value);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("JsonSerializer Serialize error: " + ex.Message);
                if (throwExceptions)
                    throw;
                return false;
            }
            finally
            {
                if (writer != null)
                    writer.Close();
                if (fs != null)
                    fs.Close();
            }

            return true;
        }


        public static object Deserialize(string jsonText, Type type, bool throwExceptions = false)
        {
            dynamic json = CreateJsonNet(throwExceptions);
            if (json == null)
                return null;

            object result = null;
            dynamic reader = null;
            try
            {
                StringReader sr = new StringReader(jsonText);
                reader = Activator.CreateInstance(JsonTextReaderType, sr);
                result = json.Deserialize(reader, type);
                reader.Close();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("JsonSerializer Deserialize error: " + ex.Message);
                if (throwExceptions)
                    throw;

                return null;
            }
            finally
            {
                if (reader != null)
                    reader.Close();
            }

            return result;
        }


        /// <summary>
        /// Deserializes an object from file and returns a reference.
        /// </summary>
        /// <param name="fileName">name of the file to serialize to</param>
        /// <param name="objectType">The Type of the object. Use typeof(yourobject class)</param>
        /// <param name="binarySerialization">determines whether we use Xml or Binary serialization</param>
        /// <param name="throwExceptions">determines whether failure will throw rather than return null on failure</param>
        /// <returns>Instance of the deserialized object or null. Must be cast to your object type</returns>
        public static object DeserializeFromFile(string fileName, Type objectType, bool throwExceptions = false)
        {
            dynamic json = CreateJsonNet(throwExceptions);
            if (json == null)
                return null;

            object result = null;
            dynamic reader = null;
            FileStream fs = null;

            try
            {
                fs = new FileStream(fileName, FileMode.Open, FileAccess.Read);
                var sr = new StreamReader(fs, Encoding.UTF8);
                reader = Activator.CreateInstance(JsonTextReaderType, sr);
                result = json.Deserialize(reader, objectType);
                reader.Close();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("JsonNetSerialization Deserialization Error: " + ex.Message);
                if (throwExceptions)
                    throw;

                return null;
            }
            finally
            {
                if (reader != null)
                    reader.Close();

                if (fs != null)
                    fs.Close();
            }

            return result;
        }

        /// <summary>
        /// Dynamically creates an instance of JSON.NET
        /// </summary>
        /// <param name="throwExceptions">If true throws exceptions otherwise returns null</param>
        /// <returns>Dynamic JsonSerializer instance</returns>
        public static dynamic CreateJsonNet(bool throwExceptions = true)
        {
            if (JsonNet != null)
                return JsonNet;

            lock (SyncLock)
            {
                if (JsonNet != null)
                    return JsonNet;

                // Try to create instance
                dynamic json = ReflectionUtils.CreateInstanceFromString("Newtonsoft.Json.JsonSerializer");

                if (json == null)
                {
                    try
                    {
                        var ass = AppDomain.CurrentDomain.Load("Newtonsoft.Json");
                        json = ReflectionUtils.CreateInstanceFromString("Newtonsoft.Json.JsonSerializer");
                    }
                    catch (Exception ex)
                    {
                        if (throwExceptions)
                            throw;
                        return null;
                    }
                }

                if (json == null)
                    return null;

                FormattingType = ReflectionUtils.GetTypeFromName("Newtonsoft.Json.Formatting");
                JsonTextReaderType = ReflectionUtils.GetTypeFromName("Newtonsoft.Json.JsonTextReader");
                JsonTextWriterType = ReflectionUtils.GetTypeFromName("Newtonsoft.Json.JsonTextWriter");
                json.ReferenceLoopHandling =
                    (dynamic)ReflectionUtils.GetStaticProperty("Newtonsoft.Json.ReferenceLoopHandling", "Ignore");

                // Enums as strings in JSON
                dynamic enumConverter = ReflectionUtils.CreateInstanceFromString("Newtonsoft.Json.Converters.StringEnumConverter");
                json.Converters.Add(enumConverter);

                JsonNet = json;
            }

            return JsonNet;
        }
    }
}

