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

//#define SupportWebRequestProvider

using System;
using System.Data;
using System.Data.SqlClient;
using System.Reflection;
using System.Text;
using System.Configuration;
using System.Data.Common;
using System.Data.OleDb;
using Westwind.Utilities.Properties;

namespace Westwind.Utilities.Data
{
    /// <summary>
    /// Basic low level Data Access Layer
    /// </summary>
    public class SqlDataAccess : DataAccessBase, IDisposable
    {
        public SqlDataAccess()
        {
            dbProvider = DbProviderFactories.GetFactory("System.Data.SqlClient"); 
        }

        public SqlDataAccess(string connectionString)
        {
            if (string.IsNullOrEmpty(connectionString))
                throw new InvalidOperationException(Resources.AConnectionStringMustBePassedToTheConstructor);

            if (!connectionString.Contains("="))
            {
                var connInfo = ConfigurationManager.ConnectionStrings[connectionString];
                if (connInfo != null)
                {
                    if (!string.IsNullOrEmpty(connInfo.ProviderName))
                        dbProvider = DbProviderFactories.GetFactory(connInfo.ProviderName);
                    else
                        dbProvider = DbProviderFactories.GetFactory("System.Data.SqlClient");

                    connectionString = connInfo.ConnectionString;
                }
                else
                    throw new InvalidOperationException(Resources.InvalidConnectionStringName);
            }
            else
                dbProvider = DbProviderFactories.GetFactory("System.Data.SqlClient");
    
            ConnectionString = connectionString;
        }
        public SqlDataAccess(string connectionString, string providerName)
        {
#if SupportWebRequestProvider
            // Explicitly load Web Request Provider so the provider
            // doesn't need to be registered
            if (providerName == "Westwind.Utilities. Wind Web Request Provider")
                dbProvider = WebRequestClientFactory.Instance;
            else
#endif
            dbProvider = DbProviderFactories.GetFactory(providerName);
            ConnectionString = connectionString;
        }



        /// <summary>
        /// Opens a Sql Connection based on the connection string.
        /// Called internally but externally accessible. Sets the internal
        /// _Connection property.
        /// </summary>
        /// <returns></returns>
        public override bool OpenConnection()
        {
            try
            {
                if (_Connection == null)
                {
                    if (ConnectionString.Contains("="))
                    {
                        _Connection = dbProvider.CreateConnection();
                        _Connection.ConnectionString = ConnectionString;
                    }
                    else
                    {
                        // Assume it's a connection string value
                        _Connection = dbProvider.CreateConnection();
                        try
                        {
                            _Connection.ConnectionString = ConfigurationManager.ConnectionStrings[ConnectionString].ConnectionString;
                        }
                        catch
                        {
                            this.SetError(Resources.InvalidConnectionString);
                            return false;
                        }
                    }
                }

                if (_Connection.State != ConnectionState.Open)
                    _Connection.Open();
            }
            catch (DbException ex)
            {
                SetError(ex.Message,ex.ErrorCode);                
                return false;
            }
            catch (Exception ex)
            {
                SetError(ex.GetBaseException().Message);                
                return false;
            }

            return true;
        }

        /// <summary>
        /// Creates a Command object and opens a connection
        /// </summary>
        /// <param name="ConnectionString"></param>
        /// <param name="sql"></param>
        /// <returns></returns>
        public override DbCommand CreateCommand(string sql, CommandType commandType, params DbParameter[] parameters)
        {
            SetError();

            DbCommand command = dbProvider.CreateCommand();           

            command.CommandType = commandType;
            command.CommandText = sql;

            try
            {
                if (Transaction != null)
                {
                    command.Transaction = Transaction;
                    command.Connection = Transaction.Connection;
                }
                else
                {
                    if (!OpenConnection())
                        return null;

                    command.Connection = _Connection;
                }
            }
            catch (DbException ex)
            {
                SetError(ex.Message,ex.ErrorCode);
                return null;
            }
            catch (Exception ex)
            {
                SetError(ex.GetBaseException().Message);
                return null;
            }

            if (parameters != null)
            {
                foreach (DbParameter Parm in parameters)
                {
                    command.Parameters.Add(Parm);
                }
            }

            return command;
        }
   


        /// <summary>
        /// Creates a Sql Parameter for the specific provider
        /// </summary>
        /// <param name="parameterName"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public override DbParameter CreateParameter(string parameterName, object value)
        {
            DbParameter parm = dbProvider.CreateParameter();
            parm.ParameterName = parameterName;
            if (value == null)
                value = DBNull.Value;
            parm.Value = value;
            return parm;
        }


        /// <summary>
        /// Executes a non-query command and returns the affected records
        /// </summary>
        /// <param name="Command">Command should be created with GetSqlCommand to have open connection</param>
        /// <param name="Parameters"></param>
        /// <returns></returns>
        public override int ExecuteNonQuery(DbCommand Command, params DbParameter[] Parameters)
        {
            SetError();

            int RecordCount = 0;

            foreach (DbParameter Parameter in Parameters)
            { Command.Parameters.Add(Parameter); }

            try
            {
                RecordCount = Command.ExecuteNonQuery();
                if (RecordCount == -1)
                    RecordCount = 0;
            }
            catch (DbException ex)
            {
                RecordCount = -1;
                SetError(ex);;
            }
            catch (Exception ex)
            {
                RecordCount = -1;
                SetError(ex);
            }
            finally
            {
                CloseConnection();
            }

            return RecordCount;
        }

        /// <summary>
        /// Executes a SQL Command object and returns a SqlDataReader object
        /// </summary>
        /// <param name="Command">Command should be created with GetSqlCommand and open connection</param>
        /// <param name="Parameters"></param>
        /// <returns></returns>
        /// <returns>A SqlDataReader. Make sure to call Close() to close the underlying connection.</returns>
        public override DbDataReader ExecuteReader(DbCommand Command, params DbParameter[] Parameters)
        {
            SetError();

            if (Command.Connection == null || Command.Connection.State != ConnectionState.Open)
            {
                if (!OpenConnection())
                    return null;

                Command.Connection = _Connection;
            }

            foreach (DbParameter Parameter in Parameters)
            {
                Command.Parameters.Add(Parameter);
            }

            DbDataReader Reader = null;
            try
            {
                Reader = Command.ExecuteReader();
            }
            catch (SqlException ex)
            {
                SetError(ex.Message, ex.Number);
                CloseConnection(Command);
                return null;
            }

            return Reader;
        }


  

        /// <summary>
        /// Returns a DataTable from a Sql Command string passed in.
        /// </summary>
        /// <param name="Tablename"></param>
        /// <param name="Command"></param>
        /// <param name="Parameters"></param>
        /// <returns></returns>
        public override DataTable ExecuteTable(string Tablename, DbCommand Command, params DbParameter[] Parameters)
        {
            SetError();

            foreach (DbParameter Parameter in Parameters)
            {
                Command.Parameters.Add(Parameter);
            }

            DbDataAdapter Adapter = dbProvider.CreateDataAdapter();
            Adapter.SelectCommand = Command;

            DataTable dt = new DataTable(Tablename);

            try
            {
                Adapter.Fill(dt);
            }
            catch (Exception ex)
            {
                SetError(ex.Message);
                return null;
            }
            finally
            {
                CloseConnection(Command);
            }

            return dt;
        }




    

        /// <summary>
        /// Returns a DataTable from a Sql Command string passed in.
        /// </summary>
        /// <param name="Tablename"></param>
        /// <param name="Command"></param>
        /// <param name="Parameters"></param>
        /// <returns></returns>
        public override DataSet ExecuteDataSet(DataSet dataSet, string Tablename, DbCommand Command, params DbParameter[] Parameters)
        {
            SetError();

            if (dataSet == null)
                dataSet = new DataSet();

            DbDataAdapter Adapter = dbProvider.CreateDataAdapter();
            Adapter.SelectCommand = Command;

            if (ExecuteWithSchema)
                Adapter.MissingSchemaAction = MissingSchemaAction.AddWithKey;

            foreach (DbParameter parameter in Parameters)
            {
                Command.Parameters.Add(parameter);  
            }

            DataTable dt = new DataTable(Tablename);

            if (dataSet.Tables.Contains(Tablename))
                dataSet.Tables.Remove(Tablename);

            try
            {                
                Adapter.Fill(dataSet, Tablename);                
            }
            catch (Exception ex)
            {
                SetError(ex.Message);
                return null;
            }
            finally
            {
                CloseConnection(Command);
            }

            return dataSet;
        }

        public override DataSet ExecuteDataSet(string Tablename, DbCommand Command, params DbParameter[] Parameters)
        {
            return ExecuteDataSet(null, Tablename, Command, Parameters);
        }

        /// <summary>
        /// Executes a command and returns a scalar value from it
        /// </summary>
        /// <param name="SqlCommand">A SQL Command object</param>
        /// <returns>value or null on failure</returns>
        public override object ExecuteScalar(DbCommand Command, params DbParameter[] Parameters)
        {
            SetError();
            
            foreach (DbParameter Parameter in Parameters)
            {
                Command.Parameters.Add(Parameter);
            }

            object Result = null;
            try
            {
                Result = Command.ExecuteScalar();
            }
            catch (SqlException ex)
            {
                SetError(ex.Message,ex.Number);
            }
            finally
            {
                CloseConnection();
            }

            return Result;
        }

        /// <summary>
        /// Executes a Sql command and returns a single value from it.
        /// </summary>
        /// <param name="Sql">Sql string to execute</param>
        /// <param name="Parameters">Any named SQL parameters</param>
        /// <returns>Result value or null. Check ErrorMessage on Null if unexpected</returns>
        public override object ExecuteScalar(string Sql, params DbParameter[] Parameters)
        {
            SetError();

            DbCommand Command = CreateCommand(Sql, Parameters);
            if (Command == null)
                return null;

            return ExecuteScalar(Command);
        }


        /// <summary>
        /// Closes a connection
        /// </summary>
        /// <param name="Command"></param>
        public override void CloseConnection(DbCommand Command)
        {
            if (Transaction != null)
                return;

            if (Command.Connection != null &&
                Command.Connection.State == ConnectionState.Open)
                Command.Connection.Close();

            _Connection = null;
        }

        /// <summary>
        /// Closes an active connection. If a transaction is pending the 
        /// connection is held open.
        /// </summary>
        public override void CloseConnection()
        {
            if (Transaction != null)
                return;

            if (_Connection != null &&
                _Connection.State == ConnectionState.Open)
                _Connection.Close();

            _Connection = null;
        }


        /// <summary>
        /// Sql 2005 specific semi-generic paging routine
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="pageSize"></param>
        /// <param name="page"></param>
        /// <param name="sortOrderFields"></param>
        /// <param name="Parameters"></param>
        /// <returns></returns>
        public override DbCommand CreatePagingCommand(string sql, int pageSize, int page, string sortOrderFields, params DbParameter[] Parameters)
        {
            int pos = sql.IndexOf("select ", 0, StringComparison.OrdinalIgnoreCase);
            if (pos == -1)
            {
                SetError("Invalid Command for paging. Must start with select and followed by field list");
                return null;
            }
            sql = StringUtils.ReplaceStringInstance(sql, "select", string.Empty, 1, true);

            string NewSql = string.Format(
            @"
select * FROM 
   (SELECT ROW_NUMBER() OVER (ORDER BY @OrderByFields) as __No,{0}) __TQuery
where __No > (@Page-1) * @PageSize and __No < (@Page * @PageSize + 1)
", sql);

            return CreateCommand(NewSql,
                            CreateParameter("@PageSize", pageSize),
                            CreateParameter("@Page", page),
                            CreateParameter("@OrderByFields", sortOrderFields));

        }





    }
}
