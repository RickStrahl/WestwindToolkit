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


using System.Data.SqlClient;
using Westwind.Utilities.Data;
using System.Data.Common;
using System;
//using System.Data.SqlServerCe;
namespace Westwind.Utilities.Configuration
{

    /// <summary>
    /// Reads and Writes configuration settings in .NET config files and 
    /// sections. Allows reading and writing to default or external files 
    /// and specification of the configuration section that settings are
    /// applied to.
    /// 
    /// This implementation doesn't support Read and Write operation that
    /// don't return a string value. Only Read(string) and WriteAsString()
    /// should be used to read and write string values.
    /// </summary>
    public class SqlServerConfigurationProvider<TAppConfiguration> : ConfigurationProviderBase<TAppConfiguration>
        where TAppConfiguration : AppConfiguration, new()
    {

        /// <summary>
        /// The raw SQL connection string or connectionstrings name
        /// for the database connection.
        /// </summary>
        public string ConnectionString
        {
            get { return _ConnectionString; }
            set { _ConnectionString = value; }
        }
        private string _ConnectionString = string.Empty;

        /// <summary>
        /// The data provider used to access the database
        /// </summary>
        public string ProviderName
        {
            get { return _ProviderName; }
            set { _ProviderName = value; }
        }
        private string _ProviderName = "System.Data.SqlClient";
        

        /// <summary>
        /// Table in the database that holds configuration data
        /// Table must have ID(int) and ConfigData (nText) fields
        /// </summary>
        public string Tablename
        {
            get { return _Tablename; }
            set { _Tablename = value; }
        }
        private string _Tablename = "ConfigurationSettings";


        /// <summary>
        /// The key of the record into which the config
        /// data is written. Defaults to 1.
        /// 
        /// If you need to read or write multiple different
        /// configuration records you have to change it on
        /// this provider before calling the Read()/Write()
        /// methods.
        /// </summary>
        public int Key
        {
            get { return _Key; }
            set { _Key = value; }
        }
        private int _Key = 1;
      

        /// <summary>
        /// Reads configuration data into a new instance from SQL Server
        /// that is returned.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public override T Read<T>()
        {
            using (SqlDataAccess data = new SqlDataAccess(ConnectionString, ProviderName))
            {
                string sql = "select * from [" + Tablename + "] where id=" + Key.ToString();

                DbDataReader reader = null;
                try
                {
                    DbCommand command = data.CreateCommand(sql);
                    if (command == null)
                    {
                        SetError(data.ErrorMessage);
                        return null;
                    }
                    reader = command.ExecuteReader();
                    if (reader == null)
                    {
                        SetError(data.ErrorMessage);
                        return null;
                    }
                }
                catch (SqlException ex)
                {
                    if (ex.Number == 208)
                    {

                        sql =
    @"CREATE TABLE [" + Tablename + @"]  
( [id] [int] , [ConfigData] [ntext] COLLATE SQL_Latin1_General_CP1_CI_AS)";
                        try
                        {
                            data.ExecuteNonQuery(sql);
                        }
                        catch
                        {
                            return null;
                        }

                        // try again if we were able to create the table 
                        return Read<T>();
                    }

                }
                catch (DbException dbEx)
                {
                    // SQL CE Table doesn't exist
                    if (dbEx.ErrorCode == -2147467259)
                    {
                        sql = String.Format(
                            @"CREATE TABLE [{0}] ( [id] [int] , [ConfigData] [ntext] )",
                            Tablename);
                        try
                        {
                            data.ExecuteNonQuery(sql);
                        }
                        catch (Exception ex2)
                        {
                            return null;
                        }

                        // try again if we were able to create the table 
                        var inst = Read<T>();

                        // if we got it write it to the db
                        Write(inst);

                        return inst;
                    }
                    return null;
                }
                catch (Exception ex)
                {
                    this.SetError(ex);
                    
                    if (reader != null)
                        reader.Close();
                    
                    data.CloseConnection();
                    return null;
                }


                string xmlConfig = null;

                if (reader.Read())
                    xmlConfig = (string)reader["ConfigData"];

                reader.Close();
                data.CloseConnection();

                if (string.IsNullOrEmpty(xmlConfig))
                {
                    T newInstance = new T();
                    newInstance.Provider = this;
                    return newInstance;
                }

                T instance = Read<T>(xmlConfig);

                return instance;
            }
        }

        /// <summary>
        /// Reads configuration data from Sql Server into an existing 
        /// instance updating its fields.
        /// </summary> 
        /// <param name="config"></param>
        /// <returns></returns>
        public override bool Read(AppConfiguration config)
        {
            TAppConfiguration newConfig = Read<TAppConfiguration>();
            if (newConfig == null)
                return false;

            DataUtils.CopyObjectData(newConfig, config,"Provider,ErrorMessage");
            return true;
        }


        public override bool Write(AppConfiguration config)
        {
            SqlDataAccess data = new SqlDataAccess(ConnectionString,ProviderName);

            string sql = String.Format(
                "Update [{0}] set ConfigData=@ConfigData where id={1}", 
                Tablename, Key);
            
            string xml = WriteAsString(config);

            int result = 0;
            try
            {
                result = data.ExecuteNonQuery(sql, data.CreateParameter("@ConfigData", xml));
            }
            catch
            {
                result = -1;
            }

            // try to create the table
            if (result == -1)
            {
                sql = String.Format(
            @"CREATE TABLE [{0}] ( [id] [int] , [ConfigData] [ntext] )",
            Tablename);
                try
                {
                    result = data.ExecuteNonQuery(sql);
                    if (result > -1)
                        result = 0;
                }
                catch (Exception ex)
                {
                    SetError(ex);
                    return false;
                }
            }

            // Check for missing record
            if (result == 0)
            {
                sql = "Insert [" + Tablename + "] (id,configdata) values (" + Key.ToString() + ",@ConfigData)";

                try
                {
                    result = data.ExecuteNonQuery(sql, data.CreateParameter("@ConfigData", xml));
                }
                catch (Exception ex)
                {
                    SetError(ex);
                    return false;
                }
                if (result == 0)
                {                   
                    return false;
                }
            }

            if (result < 0)
                return false;
            
            return true;
        }
    }
}
