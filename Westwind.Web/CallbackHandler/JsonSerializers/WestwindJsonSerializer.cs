#region License
/*
 **************************************************************
 *  Author: Rick Strahl 
 *          © West Wind Technologies, 2008-2012
 *          http://www.west-wind.com/
 * 
 * Created: 09/04/2008
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


/**************************************************************
* WestwindJsonSerializer Class
**************************************************************
*  Author: Rick Strahl 
*          (c) West Wind Technologies
*          http://www.west-wind.com/
* 
* Created: 05/01/2005
* 
* Credits:
* Based on original work from Jason Diamond (Anthem.NET)
* 
* Thanks to Joe McLain for his re-work of ParseArray()
* and space parsing.
***************************************************************/

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

using Westwind.Utilities;
using Westwind.Web.Properties;
using System.Diagnostics;


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
    public class WestwindJsonSerializer : IJSONSerializer
    {
        /// <summary>
        /// The JavaScript base date on which all date time values
        /// in JavaScript are based. Offsets are given in milliseconds
        /// (ie. new Date(99990123) where the number is the offset in ms
        /// from the base date)
        /// </summary>
        static DateTime DAT_JAVASCRIPT_BASEDATE = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        Stack<object> RecursedObjectStack = new Stack<object>();

        /// <summary>
        /// Master instance of the JSONSerializer that the user interacts with
        /// Used to read option properties
        /// </summary>
        JSONSerializer masterSerializer = null;

        /// <summary>
        /// White space checking for char values
        /// </summary>
        const string STR_WHITESPACE = " \t\n\r";

        /// <summary>
        /// Internally held set of types traversed. Circular references are
        /// not serialized and return null
        /// </summary>
        private HashSet<object> _traversedTypes = new HashSet<object>();


        /// <summary>
        /// Encodes Dates as a JSON string value that is compatible
        /// with MS AJAX and is safe for JSON validators. If false
        /// serializes dates as new Date() expression instead.
        /// 
        /// The default is true.
        /// </summary>
        public JsonDateEncodingModes DateSerializationMode
        {
            get 
            {
                if (masterSerializer != null)
                    return masterSerializer.DateSerializationMode;
                return _DateSerialzationMode; 
            }
            set {
                if (masterSerializer != null)
                    masterSerializer.DateSerializationMode = value;
                _DateSerialzationMode = value; }
        }
        private JsonDateEncodingModes _DateSerialzationMode = JsonDateEncodingModes.ISO;


        /// <summary>
        /// Determines if there are line breaks inserted into the 
        /// JSON to make it more easily human readable.
        /// </summary>
        public bool FormatJsonOutput
        {
            get 
            {
                if (masterSerializer != null)
                    return masterSerializer.FormatJsonOutput;
                return _FormatJsonOutput; 
            }
            set 
            {
                if (masterSerializer != null)
                    masterSerializer.FormatJsonOutput = value;
                _FormatJsonOutput = value; 
            }
        }
        private bool _FormatJsonOutput = false;

        /// <summary>
        /// Determines whether fields are serialized
        /// </summary>
        public bool SerializeFields = false;


        /// <summary>
        ///  Force a master Serializer to be passed for settings
        /// </summary>
        /// <param name="serializer"></param>
        public WestwindJsonSerializer(JSONSerializer serializer)
        {
            masterSerializer = serializer;
            DateSerializationMode = serializer.DateSerializationMode;
            FormatJsonOutput = serializer.FormatJsonOutput;
            SerializeFields = JSONSerializer.SerializeFields;
        }

        /// <summary>
        /// Master Serializer contructor is preferred
        /// </summary>
        public WestwindJsonSerializer()
        {
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
            _traversedTypes.Clear();

            StringBuilder sb = new StringBuilder();
            WriteValue(sb, value);

            _traversedTypes.Clear();

            return sb.ToString();
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
            return ParseValueString(jsonText, valueType) as object;
        }

        /// <summary>
        /// Deserializes JSON string into a specified type value.
        /// </summary>
        /// <typeparam name="TType"></typeparam>
        /// <param name="jsonText"></param>
        /// <returns></returns>
        public TType Deserialize<TType>(string jsonText)
        {            
            return (TType) Deserialize(jsonText, typeof(TType));
        }


        #region Serialization Methods
        // This serioalization code is based on Jason Diamonds JSON parsing 
        // routines part of MyAjax.NET (aka Anthem).
        /// <summary>
        /// Serialization routine that takes any value and serializes
        /// it into JSON. 
        /// 
        /// Date formatting follows Microsoft ASP.NET AJAX format which
        /// represents dates as strings in the format of: "\/Date(231231231)\/"
        ///
        /// This code is based originally on Jason Diamond's JSON code
        /// in Anthem.NET although heavy modifications have been made.
        /// </summary>
        /// <param name="sb"></param>
        /// <param name="val"></param>
        public void WriteValue(StringBuilder sb, object val)
        {
            if (val == null || val == System.DBNull.Value)
            {
                sb.Append("null");
            }
            else if (val is string)
            {
                WriteString(sb, (string)val);
            }
            else if (val is bool)
            {
                sb.Append(val.ToString().ToLower());
            }
            else if (val is long ||
                     val is int ||
                     val is short ||
                     val is byte
                )
            {
                sb.Append(val);
            }
            else if (val is decimal)
            {
                sb.Append(((decimal)val).ToString(CultureInfo.InvariantCulture.NumberFormat));
            }
            else if (val is float)
            {
                sb.Append(((float)val).ToString(CultureInfo.InvariantCulture.NumberFormat));
            }
            else if (val is double)
            {
                sb.Append(((double)val).ToString(CultureInfo.InvariantCulture.NumberFormat));
            }
            else if (val is Single)
            {
                sb.Append(((Single)val).ToString(CultureInfo.InvariantCulture.NumberFormat));
            }
            else if (val.GetType().IsEnum)
            {
                sb.Append((int)val);
            }
            else if (val is DateTime)
            {
                WriteDate(sb, (DateTime)val);
            }
            else if (val is DataSet)
            {
                WriteDataSet(sb, val as DataSet);
            }
            else if (val is DataTable)
            {
                WriteDataTable(sb, val as DataTable);
            }
            else if (val is DataRow)
            {
                WriteDataRow(sb, val as DataRow);
            }
            else if (val is IDataReader)
            {
                WriteDataReader(sb, val as IDataReader);
            }
            else if (val is IDictionary)
            {
                WriteDictionary(sb, val as IDictionary);
            }       
            else if (val is IEnumerable)
            {
                WriteEnumerable(sb, val as IEnumerable);
            }
            else if (val is Guid)
                WriteObject(sb, val.ToString());
            else
            {
                WriteObject(sb, val);
            }
        }

        /// <summary>
        /// Writes a string as a JSON string
        /// </summary>
        /// <param name="sb"></param>
        /// <param name="s"></param>
        void WriteString(StringBuilder sb, string s)
        {
            sb.Append("\"");
            foreach (char c in s)
            {
                switch (c)
                {
                    case '\"':
                        sb.Append("\\\"");
                        break;
                    case '\\':
                        sb.Append("\\\\");
                        break;
                    case '\b':
                        sb.Append("\\b");
                        break;
                    case '\f':
                        sb.Append("\\f");
                        break;
                    case '\n':
                        sb.Append("\\n");
                        break;
                    case '\r':
                        sb.Append("\\r");
                        break;
                    case '\t':
                        sb.Append("\\t");
                        break;
                    default:
                        int i = (int)c;
                        if (i < 32 || i > 127)
                        {
                            sb.AppendFormat("\\u{0:X04}", i);
                        }
                        else
                        {
                            sb.Append(c);
                        }
                        break;
                }
            }
            sb.Append("\"");
        }

        /// <summary>
        /// Writes a date out as a JSON string into the string builder.
        /// 
        /// Data format is set based serialized based on the DateSerializationMode.         
        /// </summary>
        /// <param name="sb"></param>
        /// <param name="universalDate"></param>
        void WriteDate(StringBuilder sb, DateTime date)
        {
            double milliseconds = 0;

            // must always return dates in UTC
            DateTime universalDate = date.ToUniversalTime();

            //if (universalDate.CompareTo(DAT_JAVASCRIPT_BASEDATE) < 0)
            //{
            //    milliseconds = 0D;
            //    date = DAT_JAVASCRIPT_BASEDATE;
            //}
            //else
            //{
                // Convert to JavaScript specific milliseconds
                TimeSpan tspan = universalDate.Subtract(DAT_JAVASCRIPT_BASEDATE);
                milliseconds = Math.Floor(tspan.TotalMilliseconds);
            //}

            switch (DateSerializationMode)
            {
                case JsonDateEncodingModes.ISO:
                    sb.Append("\"" + date.ToUniversalTime().ToString("s") + "Z" + "\"");
                    break;
                case JsonDateEncodingModes.MsAjax:
                    sb.Append(@"""\/Date(");
                    sb.Append(milliseconds);

                    // Add Timezone                    
                    int offset = (TimeZone.CurrentTimeZone.GetUtcOffset(date).Hours * 100);
                    string offsetString = offset.ToString("0000").PadLeft(4, '0');
                    if (offset >= 0)
                        // negative value has minus but positive requires explicit +
                        sb.Append("+"); 
                    sb.Append(offsetString);

                    sb.Append(@")\/""");
                    break;

                case JsonDateEncodingModes.NewDateExpression:
                    sb.Append(@"new Date(");
                    sb.Append(milliseconds);
                    sb.Append(")");
                    break;
            }
        }

        void WriteDataSet(StringBuilder sb, DataSet ds)
        {
            sb.Append("{");
            foreach (DataTable table in ds.Tables)
            {
                sb.AppendFormat("\"{0}\":", table.TableName);
                WriteDataTable(sb, table);
                sb.Append(",");
                if (FormatJsonOutput)
                    sb.Append("\r\n");
            }
            // Remove the trailing comma.
            if (ds.Tables.Count > 0)
                StripComma(sb);
            
            sb.Append("}");
        }

        void WriteDataTable(StringBuilder sb, DataTable table)
        {
            sb.Append("{\"Rows\":[");
            foreach (DataRow row in table.Rows)
            {
                WriteDataRow(sb, row);
                sb.Append(",");
            }
            // Remove the trailing comma.
            if (table.Rows.Count > 0)
                sb.Length--;
            sb.Append("]}");
        }

        void WriteDataRow(StringBuilder sb, DataRow row)
        {
            sb.Append("{");
            foreach (DataColumn column in row.Table.Columns)
            {
                sb.AppendFormat("\"{0}\":", column.ColumnName);
                WriteValue(sb, row[column]);
                sb.Append(",");
                if (FormatJsonOutput)
                    sb.Append("\r\n");
            }
            if (row.Table.Rows.Count > 0)
                StripComma(sb);

            sb.Append("}");
        }


        void WriteDataReader(StringBuilder sb, IDataReader reader)
        {
            if (reader == null || reader.FieldCount == 0)
            {
                sb.Append("null");
                return;
            }

            int rowCount = 0;

            sb.Append("{\"Rows\":[\r\n");

            while (reader.Read())
            {
                sb.Append("{");
                
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    sb.Append("\"" + reader.GetName(i) + "\":") ;
                    WriteValue(sb,reader[i]);
                    sb.Append(",");
                    if (FormatJsonOutput)
                        sb.Append("\r\n");                    
                }
                // strip off trailing comma
                if (reader.FieldCount > 0)
                    StripComma(sb);

                sb.Append("},");
                if (FormatJsonOutput)
                    sb.Append("\r\n");
                
                rowCount++;
            }

            // remove trailing comma
            if (rowCount > 0)
                StripComma(sb);

            sb.Append("]}");
        }

       
        void WriteEnumerable(StringBuilder sb, IEnumerable enumerable)
        {
            if (enumerable == null)
            {
                sb.Append("null");
                return;
            }
            
            if (RecursedObjectStack.Contains(enumerable))
            {
                sb.Append("null");
                return;
            }
            RecursedObjectStack.Push(enumerable);

            bool hasItems = false;
            sb.Append("[");

            foreach (object val in enumerable)
            {
                WriteValue(sb, val);
                sb.Append(",");
                hasItems = true;
                if (FormatJsonOutput)
                    sb.Append("\r\n");
            }

            RecursedObjectStack.Pop();

            // Remove the trailing comma.
            if (hasItems && sb.Length > 0)
            {
                sb.Length--;
                if (FormatJsonOutput && sb.Length > 2)
                    sb.Length = sb.Length - 2; // remove line feed also
            }


            sb.Append("]");
        }

        /// <summary>
        /// Outputs any dictionary type that has a string key.
        /// Non string keys will cause an exception.
        /// </summary>
        /// <param name="sb"></param>
        /// <param name="dict"></param>
        void WriteDictionary(StringBuilder sb, IDictionary dict)
        {
            if (dict == null)
            {
                sb.Append("null");
                return;
            }
            //if (dict.Count == 0)
            //{
            //    sb.Append("[]");
            //    return;
            //}
            if (RecursedObjectStack.Contains(dict))
            {
                sb.Append("null");
                return;
            }
            RecursedObjectStack.Push(dict);

            bool hasItems = false;
            sb.Append("[");

            foreach (string key in dict.Keys)
            {
                object val = dict[key];

                sb.Append("{\"key\":");
                WriteValue(sb, key);
                
                sb.Append(",\"value\":");
                WriteValue(sb, val);
                sb.Append("},");

                hasItems = true;
                if (FormatJsonOutput)
                    sb.Append("\r\n");
            }

            RecursedObjectStack.Pop();

            // Remove the trailing comma.
            if (hasItems)
            {
                sb.Length--;
                if (FormatJsonOutput)
                    sb.Length = sb.Length - 2; // remove line feed also
            }

            sb.Append("]");
        }

        void WriteObject(StringBuilder sb, object obj)
        {
            if (obj == null)
            {    sb.Append("null");
                return;
            }

            Debug.WriteLine(obj.ToString());

            if (RecursedObjectStack.Contains(obj))
            {
                sb.Append("null");
                return;
            }
            RecursedObjectStack.Push(obj);

            Type objType = obj.GetType();
            MemberInfo[] members = objType.GetMembers(BindingFlags.Instance | BindingFlags.Public );
            sb.Append("{");
            bool hasMembers = false;
            Type type = null;

            // if we have a value type/struct
            // serialize fields
            if (objType.IsValueType)
                SerializeFields = true;

            foreach (MemberInfo member in members)
            {
                bool hasValue = false;
                object val = null;
                if ( SerializeFields && (member.MemberType & MemberTypes.Field) == MemberTypes.Field)
                {
                    FieldInfo field = (FieldInfo)member;
                    val = field.GetValue(obj);
                    type = field.FieldType;
                    hasValue = true;
                }
                else if ((member.MemberType & MemberTypes.Property) == MemberTypes.Property)
                {
                    PropertyInfo property = (PropertyInfo)member;
                    if (property.CanRead && property.GetIndexParameters().Length == 0)
                    {
                        val = property.GetValue(obj, null);
                        hasValue = true;
                        type = property.PropertyType;
                    }
                }

                // Skip over certain types
                //if (type ==  typeof(System.Data.Linq.Binary))
                //    continue;

                if (hasValue)
                {
                    sb.Append("\"");
                    sb.Append(member.Name);
                    sb.Append("\":");
                    WriteValue(sb, val);
                    sb.Append(",");
                    hasMembers = true;
                    if (FormatJsonOutput)
                        sb.Append("\r\n");
                }
            }

            RecursedObjectStack.Pop();

            if (hasMembers)
            {
                --sb.Length;
                if (FormatJsonOutput)
                    sb.Length = sb.Length - 2; // remove line feed also
            }
            sb.Append("}");
        }

        #endregion

        #region Deserialization methods
        /// <summary>
        /// Unescapes string encoded Unicode character in the format of \u03AF
        /// </summary>
        /// <param name="match"></param>
        /// <returns></returns>
        private string UnicodeEscapeMatchEvaluator(Match match)
        {
            // last 4 digits are hex value
            string hex = match.Value.Substring(2);
            char val = (char)ushort.Parse(hex, System.Globalization.NumberStyles.HexNumber);
            return val.ToString();
        }


        /// <summary>
        /// High level parsing method that takes a JSON string and tries to
        /// convert it to the appropriate type. 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="valueType"></param>
        /// <returns></returns>
        private object ParseValueString(string value, Type valueType)
        {
            if (value == "null")
                return null;

            if (valueType == typeof(string))
            {
                return ParseString(value);
            }

            // Most types are parsed as straight strings
            else if (valueType == typeof(decimal) ||
                     valueType == typeof(int) ||
                     valueType == typeof(float) ||
                     valueType == typeof(Single) ||
                     valueType == typeof(double) ||
                     valueType == typeof(long) ||
                     valueType == typeof(bool) ||
                     valueType == typeof(short) ||
                     valueType == typeof(byte))
            {
                return ParseNumber(value, valueType);
            }
            else if (valueType == typeof(DateTime))
            {
                return ParseDate(value);
            }
            else if ( valueType.IsArray || 
                     valueType.GetInterface("IList") != null || 
                     valueType.GetInterface("IDictionary") != null )
            {
                StringReader Reader = new StringReader(value);
                return ParseArray(Reader, valueType);
            }
            else if (valueType.IsEnum)
            {
                return Enum.Parse(valueType, value.Trim('\"'));
            }
            else if (valueType.IsClass)
            {
                StringReader Reader = new StringReader(value);
                return ParseObject(Reader, valueType, false);
            }

            return null;
        }

        private static Regex FindUnquotedStringRegEx = new Regex(@"(?<!\\)""");

        /// <summary>
        /// Parses a JSON string into a string value
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public string ParseString(string value)
        {                                    
            // actual value of null is not valid for 
            if (value == null)
                throw new ArgumentException(Resources.ERROR_INVALID_JSON_STRING);

            // null as a string is a valid value for a string
            if (value == "null")
                return null; 
            
            // Has to be at least 2 chars long and bracketed in quotes
            if (value.Length < 2 || (!value.StartsWith("\"") || !value.EndsWith("\"")))
                throw new ArgumentException(Resources.ERROR_INVALID_JSON_STRING);
            
            if (value == "\"\"")
                return string.Empty;
            
            // strip off leading and trailing quote chars
            value = value.Substring(1, value.Length - 2);
            
            // Check for strings NOT preceeded by a backslah - invalid
            if (FindUnquotedStringRegEx.IsMatch(value))
                throw new ArgumentException(Resources.ERROR_INVALID_JSON_STRING);

            // Escape the double escape characters in json ('real' backslash)  temporarily to alternate chars
            const string ESCAPE_ESCAPECHARS = @"^#^#";

            value = value.Replace(@"\\", ESCAPE_ESCAPECHARS);

            value = value.Replace(@"\r", "\r");
            value = value.Replace(@"\n", "\n");
            value = value.Replace(@"\""", "\"");            
            value = value.Replace(@"\t", "\t");
            value = value.Replace(@"\b", "\b");
            value = value.Replace(@"\f", "\f");

            if (value.Contains("\\u"))
                value = Regex.Replace(value, @"\\u....",
                                      new MatchEvaluator(UnicodeEscapeMatchEvaluator));

            // Convert escaped characters back to the actual backslash char 
            value = value.Replace(ESCAPE_ESCAPECHARS, "\\");

            return value;
        }

        public object ParseNumber(string value, Type valueType)
        {
            return ReflectionUtils.StringToTypedValue(value, valueType, CultureInfo.InvariantCulture);
        }
        public bool ParseNumber(string value, out object result)
        {
            result = null;

            result = ReflectionUtils.StringToTypedValue(value, result.GetType(), CultureInfo.InvariantCulture);

            if (result == null)
                return false;

            return true;
        }


        /// <summary>
        /// Parses Date Time values. Supports parsing values in various formats:
        /// 
        /// new Date()
        /// MS Ajax Date Form (\/Date(xxxx)\/)
        /// ISO Date Format
        /// </summary>
        /// <param name="jsonDate"></param>
        /// <returns></returns>
        private DateTime ParseDate(string jsonDate)
        {
            if (jsonDate == null)
                return DateTime.MinValue;

            DateTime date;
           
            // Strip off string wrapper since dates are embedded as strings
            // This should only be the case if a raw Date value is parsed
            // Inside of objects the value will be treated like a result string
            // and so lack the quotes.
            jsonDate = jsonDate.Trim('\"');

            try
            {
                // ASP.NET Ajax date serialization format (\/Date(ticks)\/)
                if (jsonDate.StartsWith(@"\/Date(", StringComparison.Ordinal))
                {
                    // json Date format is "\/Date(millisecondsSince1970)\/"
                    jsonDate = StringUtils.ExtractString(jsonDate, @"\/Date(", @")\/");

                    // Strip off timezone offset -/+  based on separator
                    // we'll read as UTC time so Timezone info will be 
                    // automatically reflected - safe to remove                                        
                    int at = jsonDate.LastIndexOf('-');
                    if (at < 1)
                        at = jsonDate.LastIndexOf('+');
                    if (at > 0)
                        jsonDate = jsonDate.Substring(0, at);

                    Double ticks = -1;
                    if (Double.TryParse(jsonDate,
                                        NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign | NumberStyles.Number,
                                        CultureInfo.InvariantCulture, out ticks))
                    {
                        return DAT_JAVASCRIPT_BASEDATE.AddMilliseconds(ticks).ToLocalTime();
                    }
                    return DateTime.MinValue;
                }

                // ISO Date format parsing
                if (jsonDate.Contains('T') && jsonDate.Contains('Z') && jsonDate.Contains(':'))
                {
                    date = DateTime.Parse(jsonDate, CultureInfo.InvariantCulture.DateTimeFormat, DateTimeStyles.RoundtripKind).ToLocalTime(); 
                    return date;
                    //return DateTime.ParseExact(jsonDate, CultureInfo.InvariantCulture.DateTimeFormat, DateTimeStyles.RoundtripKind).ToLocalTime(); 
                }

                // Try if standard Date parsing works
                if (DateTime.TryParse(jsonDate,CultureInfo.CurrentCulture.DateTimeFormat,
                                      DateTimeStyles.AllowWhiteSpaces | DateTimeStyles.AssumeLocal,
                                      out date))
                    return date;

                // Parse out what's between the date parens
                jsonDate = StringUtils.ExtractString(jsonDate, "new Date(", ")");

                // new Date() should never happen - error
                if (string.IsNullOrEmpty(jsonDate))
                    return DateTime.MinValue;

                // 1999,12,1,12,00,59
                if (jsonDate.Contains(","))
                {
                    string[] DateParts = jsonDate.Split(',');

                    return new DateTime(int.Parse(DateParts[0]),
                                                                int.Parse(DateParts[1]) + 1,
                                                                int.Parse(DateParts[2]),
                                                                int.Parse(DateParts[3]),
                                                                int.Parse(DateParts[4]),
                                                                int.Parse(DateParts[5]),
                                                                DateTimeKind.Utc).ToLocalTime();
                }
                // JavaScript .GetTime() style date based on milliseconds since 1970
                if (!jsonDate.Contains("-") && !jsonDate.Contains("/"))
                {
                    double ticks = 0;
                    if (double.TryParse(jsonDate, out ticks))
                    {
                        if (ticks == 0)
                            return DateTime.MinValue;

                        return DAT_JAVASCRIPT_BASEDATE.AddMilliseconds(ticks).ToLocalTime();
                    }
                    return DateTime.MinValue;
                }

                // Try to parse with UTC round trip format or invariant auto-detection
                jsonDate = jsonDate.Replace("\"", "");
                return DateTime.Parse(jsonDate, CultureInfo.InvariantCulture.DateTimeFormat, DateTimeStyles.RoundtripKind).ToLocalTime();
            }
            catch
            {
                return DateTime.MinValue;
            }
        }

        /// <summary>
        /// Parsing routine specific to parsing an object. Note the recursive flag which 
        /// allows skipping over prefix information.
        /// </summary>
        /// <param name="Reader"></param>
        /// <param name="ValueType"></param>
        /// <param name="RecursiveCall"></param>
        /// <returns></returns>
        private object ParseObject(StringReader Reader, Type ValueType, bool RecursiveCall)
        {            
            // {"Company":"West Wind Technologies","Count":2,"DollarTotal":111.22,"TestValue":true,"Entered":"\/Date(912112)\/"}            


            int Val = 0;
            char NextChar = '{';

            if (!RecursiveCall)
            {
                Val = Reader.Read();
                if (Val < 0)
                    return null;
                NextChar = (char)Val;

                // check for null return - non recursed 
                if (NextChar == 'n' && Reader.Peek() == 'u')
                    return null;

                // otherwise it needs to be an object ref
                if (NextChar != '{')
                    throw new InvalidOperationException("ParseObject(): JSON type " + ValueType.Name + " doesn't start with {.");
            }
            else
            {

                // if it's a recursive call the reader sits already on
                // the opening bracket so just set it but don't read from the reader.
                NextChar = '{';
            }

            char LastChar = '|';

            ParseStates State = ParseStates.None;

            string CurrentProperty = "";
            StringBuilder sb = new StringBuilder();
            NameValueCollection Properties = new NameValueCollection();

            object ResultObject = null;

            if (ValueType == null)
                ResultObject = null;
            else if (ValueType == typeof(string))
                ResultObject = string.Empty;
            else if (ValueType == typeof(DateTime))
                ResultObject = DateTime.MinValue;
            else if (ValueType == typeof(int) || ValueType == typeof(byte))
                ResultObject = 0;
            else if (ValueType == typeof(decimal))
                ResultObject = 0M;
            else if (ValueType == typeof(float) || ValueType == typeof(double))
                ResultObject = 0F;
            else
                // Objects start out null until we find the opening tag
                ResultObject = Activator.CreateInstance(ValueType);

            while (true)
            {
                LastChar = NextChar;
                Val = Reader.Read();
                if (Val == -1)
                    break;

                NextChar = (char)Val;

                // Check for Property Name first
                if (State == ParseStates.None &&
                    NextChar == '"')
                {
                    State = ParseStates.InPropertyName;
                    CurrentProperty = "";
                    continue;
                }


                // Check for end of Property name
                if (State == ParseStates.InPropertyName && NextChar == '"')
                {
                    State = ParseStates.InPropertyValueTransition;
                }
                // Collect property the property name chars as a string
                else if (State == ParseStates.InPropertyName)
                {
                    CurrentProperty += NextChar;
                }
                // Look for property/value separator and skip over
                else if (State == ParseStates.InPropertyValueTransition && NextChar == ':')
                    // Skip over the separator delimiter
                    continue;
                // Skip over any white space between properties/values
                else if (State == ParseStates.InPropertyValueTransition && STR_WHITESPACE.IndexOf(NextChar) >= 0)
                    // Skip over white space
                    continue;

                // Check for string and then skip over the string delimiter
                else if (State == ParseStates.InPropertyValueTransition && NextChar == '"')
                {
                    // Entering a string value
                    State = ParseStates.InStringValue;

                    // Have to append the leading quote
                    sb.Append(NextChar);
                }
                // Array Handling
                else if (State == ParseStates.InPropertyValueTransition && NextChar == '[')
                {
                    MemberInfo[] mi = ResultObject.GetType()
                                                  .GetMember(CurrentProperty,
                                                             BindingFlags.Instance | BindingFlags.Instance |
                                                             BindingFlags.GetField | BindingFlags.GetProperty |
                                                             BindingFlags.Public);

                    if (ResultObject == null)
                    { } // do nothing - leave at default
                    else if (mi == null || mi.Length < 1)
                        AssignProperty(ResultObject, CurrentProperty, null);
                    else
                    {
                        Type ObjectType = null;
                        if (mi[0].MemberType == MemberTypes.Field)
                            ObjectType = ((FieldInfo)mi[0]).FieldType;
                        else
                            ObjectType = ((PropertyInfo)mi[0]).PropertyType;

                        object Result = ParseArray(Reader, ObjectType /*, false */);

                        if (mi[0].MemberType == MemberTypes.Field)
                            ((FieldInfo)mi[0]).SetValue(ResultObject, Result);
                        else
                            if (((PropertyInfo)mi[0]).CanWrite)
                                ((PropertyInfo)mi[0]).SetValue(ResultObject, Result, null);

                        NextChar = ']';
                    }
                    State = ParseStates.None;
                }
                // Nested Object - recursively read characters
                else if (State == ParseStates.InPropertyValueTransition && NextChar == '{')
                {
                    State = ParseStates.InObject;


                    MemberInfo[] mi = ResultObject.GetType().GetMember(CurrentProperty, BindingFlags.Instance | BindingFlags.Instance | BindingFlags.GetField | BindingFlags.GetProperty | BindingFlags.Public);
                    if (mi == null || mi.Length < 1)
                        AssignProperty(ResultObject, CurrentProperty, null);
                    else
                    {
                        Type ObjectType = null;
                        if (mi[0].MemberType == MemberTypes.Field)
                            ObjectType = ((FieldInfo)mi[0]).FieldType;
                        else
                            ObjectType = ((PropertyInfo)mi[0]).PropertyType;

                        object Result = ParseObject(Reader, ObjectType, true);

                        if (mi[0].MemberType == MemberTypes.Field)
                            ((FieldInfo)mi[0]).SetValue(ResultObject, Result);
                        else
                            if (((PropertyInfo)mi[0]).CanWrite)
                                ((PropertyInfo)mi[0]).SetValue(ResultObject, Result, null);

                        NextChar = '}';
                    }
                    State = ParseStates.None;
                }
                else if (State == ParseStates.InPropertyValueTransition)
                {
                    State = ParseStates.InValue;
                    sb.Append(NextChar);
                }
                // deal with string value content
                else if (State == ParseStates.InStringValue)
                {
                    //if (NextChar == '"')
                    //    DateSerializationMode = JavaScriptDateModes.MsAjax;

                    // check for terminating quote
                    if (NextChar == '"' && LastChar != '\\')
                    {
                        sb.Append(NextChar); // add quote

                        // Assign the value to the string property
                        State = ParseStates.None;
                        AssignProperty(ResultObject, CurrentProperty, sb.ToString());
                        sb.Length = 0;
                    }
                    else
                        // just write out the next character
                        sb.Append(NextChar);
                }
                // Check for end of object - if found assign object
                else if (State == ParseStates.InValue && NextChar == '}')
                {
                    AssignProperty(ResultObject, CurrentProperty, sb.ToString());
                    sb.Length = 0;
                    State = ParseStates.EndOfObject;
                    return ResultObject;
                }
                // Check for end of value (,)
                else if (State == ParseStates.InValue && NextChar == ',')
                {
                    AssignProperty(ResultObject, CurrentProperty, sb.ToString());
                    sb.Length = 0;

                    // Skip over any white space after comma
                    while (Char.IsWhiteSpace((char)Reader.Peek()))
                        Reader.Read();

                    // next property or end of 
                    State = ParseStates.None;
                }
                // pick up any values
                else if (State == ParseStates.InValue)
                    sb.Append(NextChar);

                // this should never really fire, but just in case
                else if (State == ParseStates.EndOfObject || NextChar == '}')
                {
                    return ResultObject;
                }
            }

            return ResultObject;
        }

        /// <summary>
        /// Parses a array and IList subtype 
        /// 
        /// Supports only Objects as array items
        /// </summary>
        /// <param name="Reader"></param>
        /// <param name="ArrayType"></param>
        /// <returns></returns>
#if false        /// 
        private object ParseArray(StringReader Reader, Type ArrayType, bool RecursiveCall)
        {
            // Retrieve the type of child elements - Can be Array, Collection etc.
            Type ElementType = GetArrayType(ArrayType);

            // Start by parsing each of the items
            List<object> Items = new List<object>();

            char NextChar = '[';
            char LastChar = '|';
            ParseStates State = ParseStates.None;

            while (true)
            {
                LastChar = NextChar;
                int Val = Reader.Read();
                if (Val == -1)
                    break;
                NextChar = (char)Val;


                // Root array
                if (!RecursiveCall && State == ParseStates.None && NextChar == '[')
                { State = ParseStates.InPropertyValueTransition; }
                // Adding an object
                else if ((State == ParseStates.None || State == ParseStates.InPropertyValueTransition) && NextChar == '{')
                {
                    object ParsedObject = ParseObject(Reader, ElementType, true);
                    Items.Add(ParsedObject);
                    State = ParseStates.InPropertyValueTransition;
                }
                // Nested Array/List
                else if ((State == ParseStates.None || State == ParseStates.InPropertyValueTransition) && NextChar == '[')
                {
                    object ParsedObject = ParseArray(Reader, ElementType, true);
                    if (ParsedObject != null)
                        Items.Add(ParsedObject);

                    State = ParseStates.InPropertyValueTransition;
                }
                else if ((State == ParseStates.InPropertyValueTransition || State == ParseStates.None) && NextChar == ']')
                {
                    if (ArrayType.IsArray)
                    {
                        Array ElementArray = Activator.CreateInstance(ArrayType, Items.Count) as Array;
                        for (int i = 0; i < Items.Count; i++)
                        {
                            object Item = Activator.CreateInstance(ElementType);
                            ElementArray.SetValue(Items[i], i);
                        }
                        return ElementArray;
                    }
                    //else if (ArrayType is DataRow)
                    //{

                    //}
                    else if (ArrayType.GetInterface("IList") != null)
                    {
                        IList Col = Activator.CreateInstance(ArrayType) as IList;
                        foreach (object Item in Items)
                        {
                            Col.Add(Item);
                        }
                        return Col;
                    }
                }

            }

            return null;
        }
#endif

#if true
        static string arrayTerminatorChar = "[{]";
        static char arrayItemTerminatorChar = ',';


        /// <summary>
        /// Parses a JSON array string to its underlying array type.
        /// Requires:
        /// - Array must be of a single element type (ie. an object, or string or decimal)
        /// </summary>
        /// <remarks>
        /// Re-written by Joe McLain with modifications by Rick Strahl
        /// 06/01/2008
        /// </remarks>
        /// <param name="reader"></param>
        /// <param name="arrayType"></param>
        /// <returns></returns>
        public object ParseArray(StringReader reader, Type arrayType)
        {
            StringBuilder sb = new StringBuilder();            

            int Val = reader.Peek();
            if (Val < 0)
                return null;

            char NextChar = (char)Val;

            // check for valid 
            if ('[' == Val)
            {
                // skip over the open Array tag
                reader.Read();
            }

            bool IsIList = arrayType.GetInterface("IList") != null;
            bool IsIDictionary = arrayType.GetInterface("IDictionary") != null;


            // Retrieve the type of child elements - Can be an object, another Array, Collection, a Value etc.
            //
            // note: unless 'ArrayType' is an array of arrays 'ElementType' will not be an array type
            // which is the reason we never want to immedatly recurse into ourself passing 'ElementType' as
            // the 'ArrayType' parameter (see above)            
            Type elementType = GetArrayType(arrayType);

            // Temporary storage for what gets parsed
            ArrayList items = new ArrayList();

            // Partial value storage
            sb.Length = 0;

            bool inQuote = false;

            // Start off in 'neutral'
            ParseStates State = ParseStates.None;

            // Set next char here so Lastchar will be set to the [
            NextChar = '[';

            // Loop through the characters
            while (true)
            {
                // Keep track of the last character
                char LastChar = NextChar;

                // Get the next input character
                Val = reader.Read();
                if (Val < 0)
                {
                    // Must check for null first
                    if (sb.ToString() == "null")
                        return null;

                    // Array failed to close
                    throw new InvalidOperationException("ParseArray(): the array failed to close.");
                }
                NextChar = (char)Val;

                // wait for array termination or end of content
                if ((arrayTerminatorChar.IndexOf(NextChar) >= 0 || NextChar == arrayItemTerminatorChar) && sb.Length > 0)
                {
                    // gathering chars for a value
                    //
                    if (inQuote)
                    {
                        //
                        // then we allow the character to pass through and become part of the value
                        //
                    }
                    else
                    {

                        // Process the value - ignore if null
                        string value = sb.ToString();

                        object item = ParseValueString(value, elementType);
                        if (null != item)
                        {
                            items.Add(item);
                        }

                        // Reset
                        sb.Length = 0;
                        State = ParseStates.InPropertyValueTransition;
                    }
                }

                //***
                // ParseStates.None indicates we're looking for some value (or object or array) or the end of the array
                // ParseStates.InPropertyValueTransition indicates we've parsed an object or another array
                // and now we're looking for a comma or the end of the array
                //
                // ParseStates.InValue says were gathering up characters for a value
                if (State == ParseStates.None && NextChar == '{')
                {

                    // Add an object - but only when ParseState.None, presumably ParseObject() will check
                    // for the correct type of object                    
                    // TODO: This doesn't work correctly currently - 
                    object ParsedObject = ParseObject(reader, elementType, true);
                    if (ParsedObject != null)
                        items.Add(ParsedObject);

                    // waiting on a comma or end of array
                    State = ParseStates.InPropertyValueTransition;
                }

                // Check for nested Array/List - only when ParseState.None
                else if (State == ParseStates.None && NextChar == '[')
                {
                    object ParsedObject = ParseArray(reader, elementType);
                    if (ParsedObject != null)
                        items.Add(ParsedObject);

                    // wait on a comma or end of array
                    State = ParseStates.InPropertyValueTransition;
                }
                // Check for end of arrayType
                else if ((State == ParseStates.None || State == ParseStates.InPropertyValueTransition) && NextChar == ']')
                {
                    if (arrayType.IsArray)
                    {
                        Array ElementArray = Activator.CreateInstance(arrayType, items.Count) as Array;
                        for (int i = 0; i < items.Count; i++)
                        {
                            ElementArray.SetValue(items[i], i);
                        }
                        return ElementArray;
                    }

                    // Not handling DataRow retrievals
                    //else if (ArrayType is DataRow)
                    //{
                    //}

                    else if (IsIList)
                    {
                        IList Col = Activator.CreateInstance(arrayType) as IList;
                        foreach (object Item in items)
                            Col.Add(Item);

                        return Col;
                    }
                    else if (IsIDictionary)
                    {
                        IDictionary dict = Activator.CreateInstance(arrayType) as IDictionary;
                        foreach (dynamic item in items)
                        {
                                            
                            dict.Add(item.key, item.value);
                        }

                        return dict;
                    }

                    // Not an array or IList
                    throw new InvalidOperationException("ParseArray(): Array type " + arrayType.Name + " is not an Array or an IList.");
                }

                // White space elimination between properties/values
                else if (State == ParseStates.InPropertyValueTransition)
                {
                    // Skip over white space
                    while (Char.IsWhiteSpace((char)reader.Peek()))
                        reader.Read();

                    // if we don't find a comma something's wrong
                    if (',' != NextChar)
                        throw new InvalidOperationException("ParseArray(): Comma (',') expected while parsing JSON array.");

                    // look for next property
                    State = ParseStates.None;
                }
                else
                {
                    // Check for next property
                    if (State == ParseStates.None)
                    {
                        // Skip over white space and check next character
                        if (Char.IsWhiteSpace(NextChar))
                            continue;

                        // Otherwise we must be in a non-delimited value
                        State = ParseStates.InValue;
                    }

                    // This code should never really fire, but it's a safeguard

                    // Handle quotes special - we won't need to skip the quote here
                    // because it's simply passed to ParseValueString()                    
                    if ('"' == NextChar && '\\' != LastChar)
                        inQuote = !inQuote;

                    // just append to value
                    sb.Append(NextChar);
                }
            }
        }
#endif


        /// <summary>
        /// Returns the type of item type of the array/collection
        /// </summary>
        /// <param name="arrayType"></param>
        /// <returns></returns>
        private Type GetArrayType(Type arrayType)
        {
            if (arrayType.IsArray)
                return arrayType.GetElementType();

            if (arrayType == typeof(DataTable))
                return typeof(DataRow);

            if (arrayType.GetInterface("IList") != null)
            {
                MethodInfo Method = arrayType.GetMethod("Add");
                ParameterInfo Parameter = Method.GetParameters()[0];
                Type ResultType = Parameter.ParameterType;
                return ResultType;
            }
            
            if (arrayType.GetInterface("IDictionary") != null)
            {
                if (arrayType.IsGenericType)
                {
                    var keyType = arrayType.GetGenericArguments()[0];
                    var valueType = arrayType.GetGenericArguments()[1];

                    //Type nonGenericType = typeof(keyvalue<,>);
                    Type elementType = typeof(keyvalue<,>).MakeGenericType(keyType, valueType);

                    return elementType;
                }
            }        

            return null;
        }

        /// <summary>
        /// Strips a comma off a string builder. In Format mode
        /// ,\r\n are stripped
        /// </summary>
        /// <param name="sb"></param>
        private void StripComma(StringBuilder sb)
        {
             if (FormatJsonOutput)
                sb.Remove(sb.Length - 3, 3); // ,\r\n
             else    
                sb.Length--;  // ,
        }
         
        private enum ParseStates
        {
            None,
            InPropertyName,
            InPropertyValueTransition,
            InStringValue,
            InValue,
            InDate,
            InObject,
            EndOfObject
        }

        private void AssignProperty(object ResultObject, string Property, string Value)
        {
            if (ResultObject == null || string.IsNullOrEmpty(Property))
                return;

            MemberInfo[] mi = ResultObject.GetType().GetMember(Property,
                                                    BindingFlags.Instance |
                                                    BindingFlags.GetField | BindingFlags.GetProperty |
                                                    BindingFlags.IgnoreCase | BindingFlags.Public);
            if (mi == null || mi.Length < 1)
                return;

            if (mi[0].MemberType == MemberTypes.Field)
            {
                FieldInfo FInfo = mi[0] as FieldInfo;
                FInfo.SetValue(ResultObject, ParseValueString(Value, FInfo.FieldType));
            }
            else
            {
                PropertyInfo PInfo = mi[0] as PropertyInfo;
                if (PInfo.CanWrite)
                    PInfo.SetValue(ResultObject, ParseValueString(Value, PInfo.PropertyType), null);
            }
        }


        #endregion

    }

    internal class keyvalue<K,V>
    {
        public K key { get; set; }
        public V value { get; set; }
    }

}
