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
using System.Data;
using System.Linq;
using System.Data.SqlClient;
using System.Reflection;
using System.Collections.Generic;
using System.Text;
using System.Configuration;
using Westwind.Utilities.Properties;
using System.Data.Common;

namespace Westwind.Utilities
{
    /// <summary>
    /// Utility library for common data operations.
    /// </summary>
    public static class DataUtils
    {        
        public const BindingFlags MemberAccess =
            BindingFlags.Public | BindingFlags.NonPublic |
            BindingFlags.Static | BindingFlags.Instance | BindingFlags.IgnoreCase;

        public const BindingFlags MemberPublicInstanceAccess =
            BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase;

        
        /// <summary>
        /// Generates a unique Id as a string of up to 16 characters.
        /// Based on a GUID and the size takes that subset of a the
        /// Guid's 16 bytes to create a string id.
        /// 
        /// String Id contains numbers and lower case alpha chars 36 total.
        /// 
        /// Sizes: 6 gives roughly 99.97% uniqueness. 
        ///        8 gives less than 1 in a million doubles.
        ///        16 will give full GUID strength uniqueness
        /// </summary>
        /// <returns></returns>
        /// <summary>
        public static string GenerateUniqueId(int stringSize = 8)
        {
            string chars = "abcdefghijkmnopqrstuvwxyz1234567890";
            StringBuilder result = new StringBuilder(stringSize);
            int count = 0;

            

            foreach (byte b in Guid.NewGuid().ToByteArray())
            {
                result.Append(chars[b % (chars.Length - 1)]);
                count++;
                if (count >= stringSize)
                    return result.ToString();
            }
            return result.ToString();
        }


        /// Generates a unique numeric ID. Generated off a GUID and
        /// returned as a 64 bit long value
        /// </summary>
        /// <returns></returns>
        public static long GenerateUniqueNumericId()
        {
            byte[] bytes = Guid.NewGuid().ToByteArray();            
            return BitConverter.ToInt64(bytes, 0);
        }

        static Random rnd = new Random();

        /// <summary>
        /// Returns a random integer in a range of numbers
        /// a single seed value.
        /// </summary>
        /// <param name="min">The minimum value</param>
        /// <param name="max">The max value *including* this value (unlike Random.Next() which doesn't include it)</param>
        /// <returns></returns>
        public static int GetRandomNumber(int min, int max)
        {
            return rnd.Next(min, max + 1);
        }

        /// <summary>
        /// Copies the content of a data row to another. Runs through the target's fields
        /// and looks for fields of the same name in the source row. Structure must mathc
        /// or fields are skipped.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public static bool CopyDataRow(DataRow source, DataRow target)
        {
            DataColumnCollection columns = target.Table.Columns;

            for (int x = 0; x < columns.Count; x++)
            {
                string fieldname = columns[x].ColumnName;

                try
                {
                    target[x] = source[fieldname];
                }
                catch { ;}  // skip any errors
            }

            return true;
        }

        public static void CopyObjectFromDataRow(DataRow row, object targetObject)
        {
            MemberInfo[] miT = targetObject.GetType().FindMembers(MemberTypes.Field | MemberTypes.Property, MemberAccess, null, null);
            foreach (MemberInfo Field in miT)
            {
                string Name = Field.Name;
                if (!row.Table.Columns.Contains(Name))
                    continue;

                if (Field.MemberType == MemberTypes.Field)
                {
                    ((FieldInfo)Field).SetValue(targetObject, row[Name]);
                }
                else if (Field.MemberType == MemberTypes.Property)
                {
                    ((PropertyInfo)Field).SetValue(targetObject, row[Name], null);
                }
            }
        }

        /// <summary>
        /// Copies the content of an object to a DataRow with matching field names.
        /// Both properties and fields are copied. If a field copy fails due to a
        /// type mismatch copying continues but the method returns false
        /// </summary>
        /// <param name="row"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public static bool CopyObjectToDataRow(DataRow row, object target)
        {
            bool result = true;

            MemberInfo[] miT = target.GetType().FindMembers(MemberTypes.Field | MemberTypes.Property, MemberAccess, null, null);
            foreach (MemberInfo Field in miT)
            {
                string name = Field.Name;
                if (!row.Table.Columns.Contains(name))
                    continue;

                try
                {
                    if (Field.MemberType == MemberTypes.Field)
                    {
                        row[name] = ((FieldInfo)Field).GetValue(target);
                    }
                    else if (Field.MemberType == MemberTypes.Property)
                    {
                        row[name] = ((PropertyInfo)Field).GetValue(target, null);
                    }
                }
                catch { result = false; }
            }

            return result;
        }

        /// <summary>
        /// Copies the content of one object to another. The target object 'pulls' properties of the first. 
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        public static void CopyObjectData(object source, Object target)
        {
            CopyObjectData(source, target, MemberAccess);
        }

        /// <summary>
        /// Copies the content of one object to another. The target object 'pulls' properties of the first. 
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        /// <param name="memberAccess"></param>
        public static void CopyObjectData(object source, Object target, BindingFlags memberAccess)
        {
            CopyObjectData(source, target, null, memberAccess);
        }

        /// <summary>
        /// Copies the content of one object to another. The target object 'pulls' properties of the first. 
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        /// <param name="excludedProperties"></param>
        public static void CopyObjectData(object source, Object target, string excludedProperties)
        {
             CopyObjectData(source, target,excludedProperties, MemberAccess);
        }

        /// <summary>
        /// Copies the data of one object to another. The target object 'pulls' properties of the first. 
        /// This any matching properties are written to the target.
        /// 
        /// The object copy is a shallow copy only. Any nested types will be copied as 
        /// whole values rather than individual property assignments (ie. via assignment)
        /// </summary>
        /// <param name="source">The source object to copy from</param>
        /// <param name="target">The object to copy to</param>
        /// <param name="excludedProperties">A comma delimited list of properties that should not be copied</param>
        /// <param name="memberAccess">Reflection binding access</param>
        public static void CopyObjectData(object source, object target, string excludedProperties = null, BindingFlags memberAccess = MemberAccess)
        {
            string[] excluded = null;
            if (!string.IsNullOrEmpty(excludedProperties))
                excluded = excludedProperties.Split(new char[1] { ',' }, StringSplitOptions.RemoveEmptyEntries);

            MemberInfo[] miT = target.GetType().GetMembers(memberAccess);
            foreach (MemberInfo Field in miT)
            {
                string name = Field.Name;

                // Skip over any property exceptions
                if (!string.IsNullOrEmpty(excludedProperties) &&
                    excluded.Contains(name))
                    continue;

                if (Field.MemberType == MemberTypes.Field)
                {
                    FieldInfo SourceField = source.GetType().GetField(name);
                    if (SourceField == null)
                        continue;

                    object SourceValue = SourceField.GetValue(source);
                    ((FieldInfo)Field).SetValue(target, SourceValue);
                }
                else if (Field.MemberType == MemberTypes.Property)
                {
                    PropertyInfo piTarget = Field as PropertyInfo;
                    PropertyInfo SourceField = source.GetType().GetProperty(name, memberAccess);
                    if (SourceField == null)
                        continue;

                    if (piTarget.CanWrite && SourceField.CanRead)
                    {
                        object SourceValue = SourceField.GetValue(source, null);
                        piTarget.SetValue(target, SourceValue, null);
                    }
                }
            }
        }


        /// <summary>
        /// Creates a list of a given type from all the rows in a DataReader.
        /// 
        /// Note this method uses Reflection so this isn't a high performance
        /// operation, but it can be useful for generic data reader to entity
        /// conversions on the fly and with anonymous types.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="reader">An open DataReader that's in position to read</param>
        /// <param name="fieldsToSkip">Optional - comma delimited list of fields that you don't want to update</param>
        /// <param name="piList">
        /// Optional - Cached PropertyInfo dictionary that holds property info data for this object.
        /// Can be used for caching hte PropertyInfo structure for multiple operations to speed up
        /// translation. If not passed automatically created.
        /// </param>
        /// <returns></returns>
        /// <remarks>DataReader is not closed by this method. Make sure you call reader.close() afterwards</remarks>
        public static List<T> DataReaderToObjectList<T>(IDataReader reader, string fieldsToSkip = null, Dictionary<string, PropertyInfo> piList = null)
            where T : new()
        {
            List<T> list = new List<T>();

            // Get a list of PropertyInfo objects we can cache for looping            
            if (piList == null)
            {
                piList = new Dictionary<string, PropertyInfo>();
                var props = typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.Public);
                foreach (var prop in props)
                    piList.Add(prop.Name.ToLower(), prop);
            }

            while (reader.Read())
            {
                T inst = new T(); 
                DataReaderToObject(reader, inst, fieldsToSkip, piList);
                list.Add(inst);
            }

            reader.Close();

            return list;
        }

        /// <summary>
        /// Creates an IEnumerable of T from an open DataReader instance.
        ///
        /// Note this method uses Reflection so this isn't a high performance
        /// operation, but it can be useful for generic data reader to entity
        /// conversions on the fly and with anonymous types.
        /// </summary>
        /// <param name="reader">An open DataReader that's in position to read</param>
        /// <param name="fieldsToSkip">Optional - comma delimited list of fields that you don't want to update</param>
        /// <param name="piList">
        /// Optional - Cached PropertyInfo dictionary that holds property info data for this object.
        /// Can be used for caching hte PropertyInfo structure for multiple operations to speed up
        /// translation. If not passed automatically created.
        /// </param>
        /// <returns></returns>
        public static IEnumerable<T> DataReaderToIEnumerable<T>(IDataReader reader, string fieldsToSkip = null, Dictionary<string, PropertyInfo> piList = null)            
            where T : new()
        {
            if (reader != null)
            {
                using (reader)
                {                   
                    // Get a list of PropertyInfo objects we can cache for looping            
                    if (piList == null)
                    {
                        piList = new Dictionary<string, PropertyInfo>();
                        var props = typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.Public);
                        foreach (var prop in props)
                            piList.Add(prop.Name.ToLower(), prop);
                    }

                    while (reader.Read())
                    {
                        T inst = new T();
                        DataReaderToObject(reader, inst, fieldsToSkip, piList);
                        yield return inst;
                    }
                    
                }
            }
        }

        /// <summary>
        /// Populates the properties of an object from a single DataReader row using
        /// Reflection by matching the DataReader fields to a public property on
        /// the object passed in. Unmatched properties are left unchanged.
        /// 
        /// You need to pass in a data reader located on the active row you want
        /// to serialize.
        /// 
        /// This routine works best for matching pure data entities and should
        /// be used only in low volume environments where retrieval speed is not
        /// critical due to its use of Reflection.
        /// </summary>
        /// <param name="reader">Instance of the DataReader to read data from. Should be located on the correct record (Read() should have been called on it before calling this method)</param>
        /// <param name="instance">Instance of the object to populate properties on</param>
        /// <param name="fieldsToSkip">Optional - A comma delimited list of object properties that should not be updated</param>
        /// <param name="piList">Optional - Cached PropertyInfo dictionary that holds property info data for this object</param>
        public static void DataReaderToObject(IDataReader reader, object instance, 
                                              string fieldsToSkip = null, 
                                              Dictionary<string,PropertyInfo> piList = null)
        {
            if (reader.IsClosed)
                throw new InvalidOperationException(Resources.DataReaderPassedToDataReaderToObjectCannot);

            if (string.IsNullOrEmpty(fieldsToSkip))
                fieldsToSkip = string.Empty;
            else
                fieldsToSkip = "," + fieldsToSkip + ",";

            fieldsToSkip = fieldsToSkip.ToLower();

            // create a dictionary of properties to look up
            // we can pass this in so we can cache the list once 
            // for a list operation 
            if (piList == null || piList.Count < 1)
            {
                if (piList == null)
                    piList = new Dictionary<string, PropertyInfo>();

                var props = instance.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);
                foreach (var prop in props)
                    piList.Add(prop.Name.ToLower(), prop);
            }

            for (int index = 0; index < reader.FieldCount; index++)
            {
                string name = reader.GetName(index).ToLower();
                if (piList.ContainsKey(name))
                {
                    var prop = piList[name];

                    // skip fields in skip list
                    if (fieldsToSkip.Contains("," + name + ","))
                        continue;

                    // find writable properties and assign
                    if ((prop != null) && prop.CanWrite)
                    {
                        var val = reader.GetValue(index);                        
                        prop.SetValue(instance, (val == DBNull.Value) ? null : val, null);
                    }
                }
            }

            return;
        }

        /// <summary>
        /// The default SQL date used by InitializeDataRowWithBlanks. Considered a blank date instead of null.
        /// </summary>
        public static DateTime MinimumSqlDate = DateTime.Parse("01/01/1900");

        /// <summary>
        /// Initializes a Datarow containing NULL values with 'empty' values instead.
        /// Empty values are:
        /// String - ""
        /// all number types - 0 or 0.00
        /// DateTime - Value of MinimumSqlData (1/1/1900 by default);
        /// Boolean - false
        /// Binary values and timestamps are left alone
        /// </summary>
        /// <param name="row">DataRow to be initialized</param>
        public static void InitializeDataRowWithBlanks(DataRow row)
        {
            DataColumnCollection loColumns = row.Table.Columns;

            for (int x = 0; x < loColumns.Count; x++)
            {
                if (!row.IsNull(x))
                    continue;

                string lcRowType = loColumns[x].DataType.Name;

                if (lcRowType == "String")
                    row[x] = string.Empty;
                else if (lcRowType.StartsWith("Int"))
                    row[x] = 0;
                else if (lcRowType == "Byte")
                    row[x] = 0;
                else if (lcRowType == "Decimal")
                    row[x] = 0.00M;
                else if (lcRowType == "Double")
                    row[x] = 0.00;
                else if (lcRowType == "Boolean")
                    row[x] = false;
                else if (lcRowType == "DateTime")
                    row[x] = DataUtils.MinimumSqlDate;

                // Everything else isn't handled explicitly and left alone
                // Byte[] most specifically

            }
        }

        /// <summary>
        /// Maps a SqlDbType to a .NET type
        /// </summary>
        /// <param name="sqlType"></param>
        /// <returns></returns>
        public static Type SqlTypeToDotNetType(SqlDbType sqlType)
        {
            if (sqlType == SqlDbType.NText || sqlType == SqlDbType.Text ||
                sqlType == SqlDbType.VarChar || sqlType == SqlDbType.NVarChar)
                return typeof(string);
            else if (sqlType == SqlDbType.Int)
                return typeof(Int32);
            else if (sqlType == SqlDbType.Decimal || sqlType == SqlDbType.Money)
                return typeof(decimal);
            else if (sqlType == SqlDbType.Bit)
                return typeof(Boolean);
            else if (sqlType == SqlDbType.DateTime || sqlType == SqlDbType.DateTime2)
                return typeof(DateTime);
            else if (sqlType == SqlDbType.Char || sqlType == SqlDbType.NChar)
                return typeof(char);
            else if (sqlType == SqlDbType.Float)
                return typeof(Single);
            else if (sqlType == SqlDbType.Real)
                return typeof(Double);
            else if (sqlType == SqlDbType.BigInt)
                return typeof(Int64);
            else if (sqlType == SqlDbType.Image)
                return typeof(byte[]);
            else if (sqlType == SqlDbType.SmallInt)
                return typeof(byte);

            throw new InvalidCastException("Unable to convert " + sqlType.ToString() + " to .NET type.");
        }

        /// <summary>
        /// Maps a DbType to a .NET native type
        /// </summary>
        /// <param name="sqlType"></param>
        /// <returns></returns>
        public static Type DbTypeToDotNetType(DbType sqlType)
        {
            if (sqlType == DbType.String || sqlType == DbType.StringFixedLength || sqlType == DbType.AnsiString)
                return typeof(string);
            else if (sqlType == DbType.Int16 || sqlType == DbType.Int32)
                return typeof(Int32);
            else if (sqlType == DbType.Int64)
                return typeof(Int64);
            else if (sqlType == DbType.Decimal || sqlType == DbType.Currency)
                return typeof(decimal);
            else if (sqlType == DbType.Boolean)
                return typeof(Boolean);
            else if (sqlType == DbType.DateTime || sqlType == DbType.DateTime2 || sqlType == DbType.Date)
                return typeof(DateTime);
            else if (sqlType == DbType.Single)
                return typeof(Single);
            else if (sqlType == DbType.Double)
                return typeof(Double);
            else if (sqlType == DbType.Binary)
                return typeof(byte[]);
            else if (sqlType == DbType.SByte || sqlType == DbType.Byte)
                return typeof(byte);
            else if (sqlType == DbType.Guid)
                return typeof(Guid);
            else if (sqlType == DbType.Binary)
                return typeof(byte[]);            

            throw new InvalidCastException("Unable to convert " + sqlType.ToString() + " to .NET type.");
        }

        /// <summary>
        /// Converts a .NET type into a DbType value
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static DbType DotNetTypeToDbType(Type type)
        {
            if (type == typeof(string))
                return DbType.String;
            else if (type == typeof(Int32))
                return DbType.Int32;
            else if (type == typeof(Int16))
                return DbType.Int16;
            else if (type == typeof(Int64))
                return DbType.Int64;
            else if (type == typeof(Guid))
                return DbType.Guid;
            else if (type == typeof(decimal))
                return DbType.Decimal;
            else if (type == typeof(double) || type == typeof(float))
                return DbType.Double;
            else if (type == typeof(Single))
                return DbType.Single;
            else if (type == typeof(bool) || type == typeof(Boolean))
                return DbType.Boolean;
            else if (type == typeof(DateTime))
                return DbType.DateTime;
            else if (type == typeof(DateTimeOffset))
                return DbType.DateTimeOffset;
            else if (type == typeof(byte))
                return DbType.Byte;
            else if (type == typeof(byte[]))
                return DbType.Binary;            

            throw new InvalidCastException(string.Format("Unable to cast {0} to a DbType",type.Name));
        }
        /// <summary>
        /// Converts a .NET type into a SqlDbType.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static SqlDbType DotNetTypeToSqlType(Type type)
        {
            if (type == typeof(string))
                return SqlDbType.NVarChar;
            else if (type == typeof(Int32))
                return SqlDbType.Int;
            else if (type == typeof(Int16))
                return SqlDbType.SmallInt;
            else if (type == typeof(Int64))
                return SqlDbType.BigInt;
            else if (type == typeof(Guid))
                return SqlDbType.UniqueIdentifier;
            else if (type == typeof(decimal))
                return SqlDbType.Decimal;
            else if (type == typeof(double) || type == typeof(float))
                return SqlDbType.Float;
            else if (type == typeof(Single))
                return SqlDbType.Float;
            else if (type == typeof(bool) || type == typeof(Boolean))
                return SqlDbType.Bit;
            else if (type == typeof(DateTime))
                return SqlDbType.DateTime;
            else if (type == typeof(DateTimeOffset))
                return SqlDbType.DateTimeOffset;
            else if (type == typeof(byte))
                return SqlDbType.SmallInt;
            else if (type == typeof(byte[]))
                return SqlDbType.Image;

            throw new InvalidCastException(string.Format("Unable to cast {0} to a DbType", type.Name));
        }

        #region Minimal Sql Data Access Function

        /// <summary>
        /// Creates a Command object and opens a connection
        /// </summary>
        /// <param name="ConnectionString"></param>
        /// <param name="Sql"></param>
        /// <returns></returns>
        public static SqlCommand GetSqlCommand(string ConnectionString, string Sql, params SqlParameter[] Parameters)
        {
            SqlCommand Command = new SqlCommand();
            Command.CommandText = Sql;

            try
            {
                if (!ConnectionString.Contains(';'))
                    ConnectionString = ConfigurationManager.ConnectionStrings[ConnectionString].ConnectionString;

                Command.Connection = new SqlConnection(ConnectionString);
                Command.Connection.Open();
            }
            catch
            {
                return null;
            }


            if (Parameters != null)
            {
                foreach (SqlParameter Parm in Parameters)
                {
                    Command.Parameters.Add(Parm);
                }
            }

            return Command;
        }

        /// <summary>
        /// Returns a SqlDataReader object from a SQL string.
        /// 
        /// Please ensure you close the Reader object
        /// </summary>
        /// <param name="ConnectionString"></param>
        /// <param name="Sql"></param>
        /// <param name="Parameters"></param>
        /// <returns></returns>
        public static SqlDataReader GetSqlDataReader(string ConnectionString, string Sql, params SqlParameter[] Parameters)
        {
            SqlCommand Command = GetSqlCommand(ConnectionString, Sql, Parameters);
            if (Command == null)
                return null;

            SqlDataReader Reader = null;
            try
            {
                Reader = Command.ExecuteReader();
            }
            catch
            {
                CloseConnection(Command);
                return null;
            }

            return Reader;
        }

        /// <summary>
        /// Returns a DataTable from a Sql Command string passed in.
        /// </summary>
        /// <param name="Tablename"></param>
        /// <param name="ConnectionString"></param>
        /// <param name="Sql"></param>
        /// <param name="Parameters"></param>
        /// <returns></returns>
        public static DataTable GetDataTable(string Tablename, string ConnectionString, string Sql, params SqlParameter[] Parameters)
        {
            SqlCommand Command = GetSqlCommand(ConnectionString, Sql, Parameters);
            if (Command == null)
                return null;

            SqlDataAdapter Adapter = new SqlDataAdapter(Command);

            DataTable dt = new DataTable(Tablename);

            try
            {
                Adapter.Fill(dt);
            }
            catch
            {
                return null;
            }
            finally
            {
                CloseConnection(Command);
            }

            return dt;
        }


        /// <summary>
        /// Closes a connection
        /// </summary>
        /// <param name="Command"></param>
        public static void CloseConnection(SqlCommand Command)
        {
            if (Command.Connection != null &&
                Command.Connection.State == ConnectionState.Open)
                Command.Connection.Close();
        }
        #endregion

    }
}
