
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Collections;
using System.IO;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Reflection;
using System.Collections.Specialized;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Westwind.Utilities;

//using Newtonsoft.Json;


namespace Westwind.Web.JsonSerializers
{

    /// <summary>
    /// More text is a basic JSON serializer and deserializer that 
    /// deals with standard .NET types. Unlike the MS Ajax JSONSerializer
    /// parser this parser support serialization and deserialization without 
    /// explicit type markup in the JSON resulting in a simpler two-way model.
    /// 
    /// The inbound model for complex types is based on Reflection parsing
    /// of properties.
    /// </summary>
    internal class JsonNetJsonSerializer :IJSONSerializer
    {
        /// <summary>
        /// Master instance of the JSONSerializer that the user interacts with
        /// Used to read option properties
        /// </summary>
        JSONSerializer masterSerializer = null;


        /// <summary>
        /// ISO or MSAJAX serialization modes. Not supported with JSON.NET
        /// </summary>
        public JsonDateEncodingModes DateSerializationMode
        {
            get { return masterSerializer.DateSerializationMode; }
            set { masterSerializer.DateSerializationMode = value; }
        }        

        
        /// <summary>
        /// Determines if there are line breaks inserted into the 
        /// JSON to make it more easily human readable.
        /// </summary>
        public bool FormatJsonOutput
        {
            get { return masterSerializer.FormatJsonOutput; }
            set { masterSerializer.FormatJsonOutput = value; }
        }

        /// <summary>
        ///  Force a master Serializer to be passed for settings
        /// </summary>
        /// <param name="serializer"></param>
        public JsonNetJsonSerializer(JSONSerializer serializer)
        {
            masterSerializer = serializer;
        }

        private static JsonSerializer JsonNetJson;
        private static object JsonNetLock = new object();

        public dynamic CreateJsonNetInstance(bool forceReload = false)
        {
            if (!forceReload && JsonNetJson != null)
                return JsonNetJson;

            if (JsonNetJson == null)
            {
                lock (JsonNetLock)
                {
                    if (JsonNetJson == null)
                        JsonNetJson = new JsonSerializer();
                }
            }

            JsonNetJson.ObjectCreationHandling = ObjectCreationHandling.Replace;
            JsonNetJson.MissingMemberHandling = MissingMemberHandling.Ignore;
            JsonNetJson.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;

            JsonNetJson.Converters.Add(new StringEnumConverter());
            
            return JsonNetJson;
        }

        public JsonTextWriter CreateJsonNetWriter(StringWriter sw)
        {
            var writer = new JsonTextWriter(sw);

            if (FormatJsonOutput)
                writer.Formatting = Formatting.Indented;
            else
                writer.Formatting = Formatting.None;

            return writer;
        }

        /// <summary>
        /// Serializes a .NET object reference into a JSON string.
        /// 
        /// The serializer supports:
        /// &lt;&lt;ul&gt;&gt;
        /// &lt;&lt;li&gt;&gt; All simple types
        /// &lt;&lt;li&gt;&gt; POCO objects and hierarchical POCO objects
        /// &lt;&lt;li&gt;&gt; Arrays
        /// &lt;&lt;li&gt;&gt; IList based collections
        /// &lt;&lt;li&gt;&gt; DataSet
        /// &lt;&lt;li&gt;&gt; DataTable
        /// &lt;&lt;li&gt;&gt; DataRow
        /// &lt;&lt;/ul&gt;&gt;
        /// 
        /// The serializer works off any .NET type - types don't have to be explicitly 
        /// serializable.
        /// 
        /// DataSet/DataTable/DataRow parse into a special format that is essentially 
        /// array based of just the data. These objects can be serialized but cannot be
        ///  passed back in via deserialization.
        /// <seealso>Class JSONSerializer</seealso>
        /// </summary>
        /// <param name="value">
        /// The strongly typed value to parse
        /// </param>
        public string Serialize(object value)
        {
            Type type = value.GetType();

            var json = CreateJsonNetInstance();

            StringWriter sw = new StringWriter();
            var writer = CreateJsonNetWriter(sw);
            writer.QuoteChar = '"';

            JsonNetJson.Serialize(writer, value);
            
            string output = sw.ToString();
            writer.Close();             

            return output;
        }

        /// <summary>
        /// Takes a JSON string and attempts to create a .NET object from this  
        /// structure. An input type is required and any type that is serialized to  
        /// must support a parameterless constructor.
        /// 
        /// The de-serializer instantiates each object and runs through the properties
        /// 
        /// The deserializer supports &lt;&lt;ul&gt;&gt; &lt;&lt;li&gt;&gt; All simple 
        /// types &lt;&lt;li&gt;&gt; Most POCO objects and Hierarchical POCO objects 
        /// &lt;&lt;li&gt;&gt; Arrays and Object Arrays &lt;&lt;li&gt;&gt; IList based 
        /// collections &lt;&lt;/ul&gt;&gt;
        /// 
        /// Note that the deserializer doesn't support DataSets/Tables/Rows like the  
        /// serializer as there's no type information available from the client to  
        /// create these objects on the fly.
        /// <seealso>Class JSONSerializer</seealso>
        /// </summary>
        /// <param name="JSONText">
        /// A string of JSON text passed from the client.
        /// </param>
        /// <param name="valueType">
        /// The type of the object that is to be created from the JSON text.
        /// </param>
        public object Deserialize(string jsonText, Type valueType)
        {
            CreateJsonNetInstance();

            StringReader sr = new StringReader(jsonText);
            object result;
            using (var reader = new JsonTextReader(sr))
            {
                result = JsonNetJson.Deserialize(reader, valueType);
            }

            return result;
        }

        

        //public TType Deserialize<TType>(string jsonText) 
        //{
            
        //    System.Web.Script.Serialization.JavaScriptSerializer ser 
        //            = new System.Web.Script.Serialization.JavaScriptSerializer();

        //    return (TType) ser.Deserialize<TType>(jsonText);
        //}



    }



} 