#region License
//#define SupportWebRequestProvider
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
using System.Reflection;
using System.Text;
using System.Configuration;
using System.Data.Common;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using Westwind.Utilities.Properties;

namespace Westwind.Utilities.Data
{
    /// <summary>
    /// Basic low level Data Access Layer
    /// </summary>
    [DebuggerDisplay("{ErrorMessage}")]
    public abstract class DataAccessBase : IDisposable
    {
        protected DataAccessBase()
        {
            dbProvider = DbProviderFactories.GetFactory("System.Data.SqlClient"); 
        }

        protected DataAccessBase(string connectionString)
        {
            if (string.IsNullOrEmpty(connectionString))
                throw new InvalidOperationException(Resources.AConnectionStringMustBePassedToTheConstructor);
            
            if (!connectionString.Contains("="))
            {
                // it's a connection string entry
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
        protected DataAccessBase(string connectionString, string providerName)
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
        /// The internally used dbProvider
        /// </summary>
        public DbProviderFactory dbProvider = null;
        
        /// <summary>
        /// An error message if a method fails
        /// </summary>
        public virtual string ErrorMessage
        {
            get { return _ErrorMessage; }
            set { _ErrorMessage = value; }
        }
        private string _ErrorMessage = string.Empty;

        /// <summary>
        /// Optional error number returned by failed SQL commands
        /// </summary>
        public int ErrorNumber
        {
            get { return _ErrorNumber; }
            set { _ErrorNumber = value; }
        }
        private int _ErrorNumber = 0;

        /// <summary>
        /// The prefix used by the provider
        /// </summary>
        public string ParameterPrefix
        {
            get { return _ParameterPrefix; }
            set { _ParameterPrefix = value; }
        }
        private string _ParameterPrefix = "@";
        

        /// <summary>
        /// ConnectionString for the data access component
        /// </summary>
        public virtual string ConnectionString
        {
            get { return _ConnectionString; }
            set { 


                _ConnectionString = value; 
            }
        }
        private string _ConnectionString = string.Empty;


        /// <summary>
        /// A SQL Transaction object that may be active. You can 
        /// also set this object explcitly
        /// </summary>
        public virtual DbTransaction Transaction
        {
            get { return _Transaction; }
            set { _Transaction = value; }
        }
        private DbTransaction _Transaction = null;


        /// <summary>
        /// The SQL Connection object used for connections
        /// </summary>
        public virtual DbConnection Connection
        {
            get { return _Connection; }
            set { _Connection = value; }
        }
        protected DbConnection _Connection = null;

        /// <summary>
        /// Determines whether extended schema information is returned for 
        /// queries from the server. Useful if schema needs to be returned
        /// as part of DataSet XML creation 
        /// </summary>
        public virtual bool ExecuteWithSchema
        {
            get { return _ExecuteWithSchema; }
            set { _ExecuteWithSchema = value; }
        }
        private bool _ExecuteWithSchema = false;


        /// <summary>
        /// Opens a Sql Connection based on the connection string.
        /// Called internally but externally accessible. Sets the internal
        /// _Connection property.
        /// </summary>
        /// <returns></returns>
        /// <summary>
        /// Opens a Sql Connection based on the connection string.
        /// Called internally but externally accessible. Sets the internal
        /// _Connection property.
        /// </summary>
        /// <returns></returns>
        public virtual bool OpenConnection()
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
                        // it's a connection string entry
                        var connInfo = ConfigurationManager.ConnectionStrings[ConnectionString];
                        if (connInfo != null)
                        {
                            if (dbProvider == null)
                            {
                                if (!string.IsNullOrEmpty(connInfo.ProviderName))
                                    dbProvider = DbProviderFactories.GetFactory(connInfo.ProviderName);
                                else
                                    dbProvider = DbProviderFactories.GetFactory("System.Data.SqlClient");
                            }
                            ConnectionString = connInfo.ConnectionString;
                        }
                        else
                        {
                            SetError(Resources.InvalidConnectionString);
                            return false;
                        }
                    }
                }

                if (_Connection.State != ConnectionState.Open)
                    _Connection.Open();
            }
            catch (DbException ex)
            {
                SetError(ex.Message, ex.ErrorCode);
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
        /// <param name="ConnectionString">Connection string or ConnnectionString configuration name</param>
        /// <param name="sql">Sql string to create</param>
        /// <param name="commandType">Type of command to create</param>
        /// <param name="parameters">Parameter values that map to @0,@1 or DbParameter objects created with CreateParameter()</param>
        /// <returns></returns>
        public virtual DbCommand CreateCommand(string sql, CommandType commandType, params object[] parameters)
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
                SetError(ex.Message, ex.ErrorCode);
                return null;
            }
            catch (Exception ex)
            {
                SetError(ex.GetBaseException().Message);
                return null;
            }

            if (parameters != null)
                AddParameters(command,parameters);
            

            return command;
        }

        /// <summary>
        /// Creates a Command object and opens a connection
        /// </summary>
        /// <param name="ConnectionString">Connection String or Connection String Entry from config file</param>
        /// <param name="sql">Sql string to execute</param>
        /// <returns>Parameters. Either values mapping to @0,@1,@2 etc. or DbParameter objects created with CreateParameter()</returns>
        public virtual DbCommand CreateCommand(string sql, params object[] parameters)
        {
            return CreateCommand(sql, CommandType.Text, parameters);
        }

        /// <summary>
        /// Adds parameters to a DbCommand instance. Parses value and DbParameter parameters
        /// properly into the command's Parameters collection.
        /// </summary>
        /// <param name="command"></param>
        /// <param name="parameters"></param>
        protected void AddParameters(DbCommand command, object[] parameters)
        {
            if (parameters != null)
            {
                var parmCount = 0;
                foreach (var parameter in parameters)
                {
                    if (parameter is DbParameter)
                        command.Parameters.Add(parameter);
                    else
                    {
                        var parm = CreateParameter(ParameterPrefix + parmCount, parameter);
                        command.Parameters.Add(parm);
                        parmCount++;
                    }
                }
            }

        }

        /// <summary>
        /// Used to create named parameters to pass to commands or the various
        /// methods of this class.
        /// </summary>
        /// <param name="parameterName"></param>
        /// <param name="value"></param>
        /// <param name="dbType"></param>
        /// <returns></returns>
        public virtual DbParameter CreateParameter(string parameterName, object value)
        {
            DbParameter parm = dbProvider.CreateParameter();
            parm.ParameterName = parameterName;
            if (value == null)
                value = DBNull.Value;
            parm.Value = value;
            return parm;
        }


        /// <summary>
        /// Used to create named parameters to pass to commands or the various
        /// methods of this class.
        /// </summary>
        /// <param name="parameterName"></param>
        /// <param name="value"></param>
        /// <param name="dbType"></param>
        /// <returns></returns>
        public virtual DbParameter CreateParameter(string parameterName, object value, ParameterDirection parameterDirection = ParameterDirection.Input)
        {
            DbParameter parm = CreateParameter(parameterName, value);
            parm.Direction = parameterDirection;
            return parm;
        }

        /// <summary>
        /// Used to create named parameters to pass to commands or the various
        /// methods of this class.
        /// </summary>
        /// <param name="parameterName"></param>
        /// <param name="value"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public virtual DbParameter CreateParameter(string parameterName, object value, int size)
        {
            DbParameter parm = CreateParameter(parameterName, value);
            parm.Size = size;
            return parm;
        }

        /// <summary>
        /// Used to create named parameters to pass to commands or the various
        /// methods of this class.
        /// </summary>
        /// <param name="parameterName"></param>
        /// <param name="value"></param>
        /// <param name="dbType"></param>
        /// <returns></returns>
        public virtual DbParameter CreateParameter(string parameterName, object value, DbType type)
        {
            DbParameter parm = CreateParameter(parameterName, value);
            parm.DbType = type;
            return parm;
        }

        /// <summary>
        /// Used to create named parameters to pass to commands or the various
        /// methods of this class.
        /// </summary>
        /// <param name="parameterName"></param>
        /// <param name="value"></param>
        /// <param name="type"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public virtual DbParameter CreateParameter(string parameterName, object value, DbType type, int size)
        {
            DbParameter parm = CreateParameter(parameterName, value);
            parm.DbType = type;
            parm.Size = size;
            return parm;
        }

        /// <summary>
        /// Executes a non-query command and returns the affected records
        /// </summary>
        /// <param name="Command">Command should be created with GetSqlCommand to have open connection</param>
        /// <param name="Parameters"></param>
        /// <returns></returns>
        public virtual int ExecuteNonQuery(DbCommand Command)
        {
            SetError();

            int RecordCount = 0;

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
        /// Executes a command that doesn't return any data. The result
        /// returns the number of records affected or -1 on error.
        /// </summary>
        /// <param name="sql">SQL statement as a string</param>
        /// <param name="parameters">Any number of SQL named parameters</param>
        /// <returns></returns>
        /// <summary>
        /// Executes a command that doesn't return a data result. You can return
        /// output parameters and you do receive an AffectedRecords counter.
        /// </summary>        
        public virtual int ExecuteNonQuery(string sql, params object[] parameters)
        {
            DbCommand command = CreateCommand(sql,parameters);
            if (command == null)
                return -1;

            return ExecuteNonQuery(command);
        }


        /// <summary>
        /// Executes a SQL Command object and returns a SqlDataReader object
        /// </summary>
        /// <param name="command">Command should be created with GetSqlCommand and open connection</param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        /// <returns>A SqlDataReader. Make sure to call Close() to close the underlying connection.</returns>
        //public abstract DbDataReader ExecuteReader(DbCommand Command, params DbParameter[] Parameters)
        public virtual DbDataReader ExecuteReader(DbCommand command, params object[] parameters)
        {
            SetError();

            if (command.Connection == null || command.Connection.State != ConnectionState.Open)
            {
                if (!OpenConnection())
                    return null;

                command.Connection = _Connection;
            }

            AddParameters(command, parameters);

            DbDataReader Reader = null;
            try
            {
                Reader = command.ExecuteReader();
            }
            catch (DbException ex)
            {
                SetError(ex.GetBaseException().Message);
                CloseConnection(command);
                return null;
            }

            return Reader;
        }

        /// <summary>
        /// Executes a SQL command against the server and returns a DbDataReader
        /// </summary>
        /// <param name="sql">Sql String</param>
        /// <param name="parameters">Any SQL parameters </param>
        /// <returns></returns>
        public virtual DbDataReader ExecuteReader(string sql, params object[] parameters)
        {
            DbCommand command = CreateCommand(sql, parameters);
            if (command == null)
                return null;

            return ExecuteReader(command);
        }


        /// <summary>
        /// Executes a SQL statement and creates an object list using
        /// Reflection.
        /// 
        /// Not very efficient but provides an easy way to retrieve
        /// </summary>
        /// <typeparam name="T">Entity type to create from DataReader data</typeparam>
        /// <param name="sql">Sql string to execute</param>        
        /// <param name="parameters">DbParameters to fill the SQL statement</param>
        /// <returns>List of objects</returns>
        public virtual List<T> ExecuteReader<T>(string sql, params object[] parameters)            
            where T : class, new()
        {
            var reader = ExecuteReader(sql, parameters);
            if (reader == null)
                return null;

            var result = DataUtils.DataReaderToObjectList<T>(reader,null);
            reader.Close();
            return result;
        }

        /// <summary>
        /// Executes a SQL statement and creates an object list using
        /// Reflection.
        /// 
        /// Not very efficient but provides an easy way to retrieve
        /// </summary>
        /// <typeparam name="T">Entity type to create from DataReader data</typeparam>
        /// <param name="sql">Sql string to execute</param>        
        /// <param name="propertiesToExclude">Comma delimited list of properties that are not to be updated</param>
        /// <param name="parameters">DbParameters to fill the SQL statement</param>
        /// <returns>List of objects</returns>
        public virtual List<T> ExecuteReader<T>(string sql, string propertiesToExclude, params object[] parameters)            
            where T: class, new()
        {
            var reader = this.ExecuteReader(sql, parameters);
            if (reader == null)
                return null;

            var result = DataUtils.DataReaderToObjectList<T>(reader, propertiesToExclude);
            reader.Close();

            return result;
        }

        /// <summary>
        /// Return a list of entities that are matched to an object
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public virtual List<T> Query<T>(string sql, params object[] parameters)
            where T: class, new()
        {
            return ExecuteReader<T>(sql, null, parameters);
        }

        public virtual List<T> Query<T>(string sql, string propertiesToExclude, params object[] parameters)
            where T: class, new()
        {
            return ExecuteReader<T>(sql, propertiesToExclude, parameters);
        }

        /// <summary>
        /// Executes a Sql statement and returns a dynamic DataReader instance 
        /// that exposes each field as a property
        /// </summary>
        /// <param name="sql">Sql String to executeTable</param>
        /// <param name="parameters">Array of DbParameters to pass</param>
        /// <returns></returns>
        public virtual dynamic ExecuteDynamicDataReader(string sql, params object[] parameters)
        {
            var reader = ExecuteReader(sql, parameters);
            return new DynamicDataReader(reader);
        }

        /// <summary>
        /// Executes a SQL statement and creates an object list using
        /// Reflection.
        /// 
        /// Not very efficient but provides an easy way to retrieve
        /// </summary>
        /// <typeparam name="T">Entity type to create from DataReader data</typeparam>
        /// <param name="sql">Sql string to execute</param>        
        /// <param name="parameters">DbParameters to fill the SQL statement</param>
        /// <returns>List of objects</returns>
        public virtual List<T> ExecuteReader<T>(DbCommand sqlCommand, params object[] parameters)
            where T : class, new()
        {
            var reader = ExecuteReader(sqlCommand, parameters);
            return DataUtils.DataReaderToObjectList<T>(reader, null);
        }


        /// <summary>
        /// Returns a DataTable from a Sql Command string passed in.
        /// </summary>
        /// <param name="tablename"></param>
        /// <param name="command"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public virtual DataTable ExecuteTable(string tablename, DbCommand command, params object[] parameters)
        {
            SetError();

            AddParameters(command, parameters);

            DbDataAdapter Adapter = dbProvider.CreateDataAdapter();
            Adapter.SelectCommand = command;

            DataTable dt = new DataTable(tablename);

            try
            {
                Adapter.Fill(dt);
            }
            catch (Exception ex)
            {
                SetError(ex.GetBaseException().Message);
                return null;
            }
            finally
            {
                CloseConnection(command);
            }

            return dt;
        }

        /// <summary>
        /// Returns a DataTable from a Sql Command string passed in.
        /// </summary>
        /// <param name="Tablename"></param>
        /// <param name="ConnectionString"></param>
        /// <param name="Sql"></param>
        /// <param name="Parameters"></param>
        /// <returns></returns>
        public virtual DataTable ExecuteTable(string Tablename, string Sql, params object[] Parameters)
        {
            SetError();

            DbCommand Command = CreateCommand(Sql, Parameters);
            if (Command == null)
                return null;

            return ExecuteTable(Tablename, Command);
        }


        /// <summary>
        /// Returns a DataSet/DataTable from a Sql Command string passed in. 
        /// </summary>
        /// <param name="Tablename">The name for the table generated or the base names</param>
        /// <param name="Command"></param>
        /// <param name="Parameters"></param>
        /// <returns></returns>
        public virtual DataSet ExecuteDataSet(string Tablename, DbCommand Command, params object[] Parameters)
        {
            return ExecuteDataSet(null, Tablename, Command, Parameters);
        }

        /// <summary>
        /// Executes a SQL command against the server and returns a DataSet of the result
        /// </summary>
        /// <param name="command"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public virtual DataSet ExecuteDataSet(string tablename, string sql, params object[] parameters)
        {
            return ExecuteDataSet(tablename, CreateCommand(sql), parameters);
        }


        /// <summary>
        /// Returns a DataSet from a Sql Command string passed in.
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="command"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>        
        public virtual DataSet ExecuteDataSet(DataSet dataSet, string tableName, DbCommand command, params object[] parameters)
        {
            SetError();

            if (dataSet == null)
                dataSet = new DataSet();

            DbDataAdapter Adapter = dbProvider.CreateDataAdapter();
            Adapter.SelectCommand = command;

            if (ExecuteWithSchema)
                Adapter.MissingSchemaAction = MissingSchemaAction.AddWithKey;

            AddParameters(command, parameters);

            DataTable dt = new DataTable(tableName);

            if (dataSet.Tables.Contains(tableName))
                dataSet.Tables.Remove(tableName);

            try
            {
                Adapter.Fill(dataSet, tableName);
            }
            catch (Exception ex)
            {
                SetError(ex.Message);
                return null;
            }
            finally
            {
                CloseConnection(command);
            }

            return dataSet;
        }

        /// <summary>
        /// Returns a DataTable from a Sql Command string passed in.
        /// </summary>
        /// <param name="tablename"></param>
        /// <param name="Command"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public virtual DataSet ExecuteDataSet(DataSet dataSet, string tablename, string sql, params object[] parameters)
        {
            DbCommand Command = CreateCommand(sql, parameters);
            if (Command == null)
                return null;

            return ExecuteDataSet(dataSet, tablename, Command);
        }




        /// <summary>
        /// Executes a command and returns a scalar value from it
        /// </summary>
        /// <param name="SqlCommand">A SQL Command object</param>
        /// <returns>value or null on failure</returns>        
        public virtual object ExecuteScalar(DbCommand command, params object[] parameters)
        {
            SetError();

            AddParameters(command, parameters);

            object Result = null;
            try
            {
                Result = command.ExecuteScalar();
            }
            catch (DbException ex)
            {
                SetError(ex.GetBaseException().Message);
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
        public virtual object ExecuteScalar(string sql, params object[] parameters)
        {
            SetError();

            DbCommand command = CreateCommand(sql, parameters);
            if (command == null)
                return null;

            return ExecuteScalar(command, null);
        }

        /// <summary>
        /// Closes a connection
        /// </summary>
        /// <param name="Command"></param>
        public virtual void CloseConnection(DbCommand Command)
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
        public virtual void CloseConnection()
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
        public virtual DbCommand CreatePagingCommand(string sql, int pageSize, int page, string sortOrderFields, params object[] Parameters)
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

        /// <summary>
        /// Executes a long SQL script that contains batches (GO commands). This code
        /// breaks the script into individual commands and captures all execution errors.
        /// 
        /// If ContinueOnError is false, operations are run inside of a transaction and
        /// changes are rolled back. If true commands are accepted even if failures occur
        /// and are not rolled back.
        /// </summary>
        /// <param name="Script"></param>
        /// <param name="ScriptIsFile"></param>
        /// <returns></returns>
        public bool RunSqlScript(string Script, bool ContinueOnError, bool ScriptIsFile)
        {
            SetError();

            if (ScriptIsFile)
            {
                try
                {
                    Script = File.ReadAllText(Script);
                }
                catch (Exception ex)
                {
                    SetError(ex.Message);
                    return false;
                }
            }

            string[] ScriptBlocks = System.Text.RegularExpressions.Regex.Split(Script + "\r\n", "GO\r\n");
            string Errors = "";

            if (!ContinueOnError)
                BeginTransaction();

            foreach (string Block in ScriptBlocks)
            {
                if (string.IsNullOrEmpty(Block.TrimEnd()))
                    continue;

                if (ExecuteNonQuery(Block) == -1)
                {
                    Errors = ErrorMessage + "\r\n";
                    if (!ContinueOnError)
                    {
                        RollbackTransaction();
                        return false;
                    }
                }
            }

            if (!ContinueOnError)
                CommitTransaction();

            if (string.IsNullOrEmpty(Errors))
                return true;

            ErrorMessage = Errors;
            return false;
        }

        #region Generic Entity features
        /// <summary>
        /// Generic routine to retrieve an object from a database record
        /// The object properties must match the database fields.
        /// </summary>
        /// <param name="entity">The object to update</param>
        /// <param name="command">Database command object</param>
        /// <param name="fieldsToSkip"></param>
        /// <returns></returns>
        public virtual bool GetEntity(object entity, DbCommand command, string fieldsToSkip = null)
        {
            SetError();

            if (string.IsNullOrEmpty(fieldsToSkip))
                fieldsToSkip = string.Empty;

            DbDataReader reader = ExecuteReader(command);
            if (reader == null)
                return false;

            if (!reader.Read())
            {
                reader.Close();
                CloseConnection(command);
                return false;
            }

            DataUtils.DataReaderToObject(reader, entity, fieldsToSkip);

            reader.Close();
            CloseConnection();

            return true;
        }

        /// <summary>
        /// Retrieves a single record and returns it as an entity
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="sql"></param>
        /// <param name="fieldsToSkip"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public bool GetEntity(object entity, string sql, object[] parameters)
        {
            return GetEntity(entity, CreateCommand(sql,parameters),null);
        }



        /// <summary>
        /// Generic routine to return an Entity that matches the field names of a 
        /// table exactly.
        /// </summary>
        /// <param name="Entity"></param>
        /// <param name="Table"></param>
        /// <param name="KeyField"></param>
        /// <param name="KeyValue"></param>
        /// <param name="FieldsToSkip"></param>
        /// <returns></returns>
        public virtual bool GetEntity(object Entity, string Table, string KeyField, object KeyValue, string FieldsToSkip = null)
        {
            SetError();

            DbCommand Command = CreateCommand("select * from " + Table + " where [" + KeyField + "]=" + ParameterPrefix + "Key",
                                                    CreateParameter(ParameterPrefix + "Key", KeyValue));
            if (Command == null)
                return false;

            return GetEntity(Entity, Command, FieldsToSkip);
        }

        /// <summary>
        /// Returns a single
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="keyValue"></param>
        /// <param name="tableName"></param>
        /// <param name="keyField"></param>
        /// <returns></returns>
        public virtual T Find<T>(object keyValue, string tableName,string keyField)
            where T: class,new()
        {
            T obj = new T();
            if (obj == null)
                return null;

            if (!GetEntity(obj, tableName, keyField, keyValue, null))
                return null;

            return obj;
        }

        /// <summary>
        /// Returns a single object retrieved from data
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public virtual T Find<T>(string sql, params object[] parameters)
            where T : class,new()
        {
            T obj = new T();
            if (!GetEntity(obj, sql, parameters))
                return null;

            return obj;
        }

        /// <summary>
        /// Returns a single object retrieved from data
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public virtual T Find<T>(string sql, string fieldsToSkip, params object[] parameters)
            where T : class,new()
        {
            T obj = new T();
            if (!GetEntity(obj, CreateCommand( sql, parameters),fieldsToSkip))
                return null;

            return obj;
        }

        /// <summary>
        /// Updates an entity object that has matching fields in the database for each
        /// public property. Kind of a poor man's quick entity update mechanism.
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="table"></param>
        /// <param name="keyField"></param>
        /// <param name="fieldsToSkip"></param>
        /// <returns></returns>
        public virtual bool UpdateEntity(object entity, string table, string keyField, string fieldsToSkip = null)
        {
            this.SetError();

            if (string.IsNullOrEmpty(fieldsToSkip))
                fieldsToSkip = string.Empty;
            else
                fieldsToSkip = "," + fieldsToSkip.ToLower() + ",";

            DbCommand Command = CreateCommand(string.Empty);

            Type ObjType = entity.GetType();

            StringBuilder sb = new StringBuilder();
            sb.Append("update " + table + " set ");

            PropertyInfo[] Properties = ObjType.GetProperties(BindingFlags.Instance | BindingFlags.Public);
            foreach (PropertyInfo Property in Properties)
            {
                if (!Property.CanRead)
                    continue;

                string Name = Property.Name;

                if (fieldsToSkip.IndexOf("," + Name.ToLower() + ",") > -1)
                    continue;

                object Value = Property.GetValue(entity, null);

                sb.Append(" [" + Name + "]=" + this.ParameterPrefix +  Name + ",");

                Command.Parameters.Add(CreateParameter(ParameterPrefix + Name, Value));
            }

            object pkValue = ReflectionUtils.GetProperty(entity, keyField);

            String CommandText = sb.ToString().TrimEnd(',') + " where " + keyField + "=" + ParameterPrefix + "__PK";

            Command.Parameters.Add(CreateParameter(ParameterPrefix + "__PK", pkValue));
            Command.CommandText = CommandText;

            bool Result = ExecuteNonQuery(Command) > -1;
            CloseConnection(Command);

            return Result;
        }
        
        /// <summary>
        /// This version of UpdateEntity allows you to specify which fields to update and
        /// so is a bit more efficient as it only checks for specific fields in the database
        /// and the underlying table.
        /// </summary>
        /// <seealso cref="SaveEntity">
        /// <seealso cref="InsertEntity"/>
        /// <param name="Entity"></param>
        /// <param name="Table"></param>
        /// <param name="KeyField"></param>
        /// <param name="FieldsToSkip"></param>
        /// <param name="FieldsToUpdate"></param>
        /// <returns></returns>
        public virtual bool UpdateEntity(object Entity, string Table, string KeyField, string FieldsToSkip, string FieldsToUpdate)
        {
            this.SetError();

            if (FieldsToSkip == null)
                FieldsToSkip = string.Empty;
            else
                FieldsToSkip = "," + FieldsToSkip.ToLower() + ",";

            DbCommand Command = CreateCommand(string.Empty);

            Type ObjType = Entity.GetType();

            StringBuilder sb = new StringBuilder();
            sb.Append("update " + Table + " set ");

            string[] Fields = FieldsToUpdate.Split(',');
            foreach (string Name in Fields)
            {
                if (FieldsToSkip.IndexOf("," + Name.ToLower() + ",") > -1)
                    continue;

                PropertyInfo Property = ObjType.GetProperty(Name);
                if (Property == null)
                    continue;

                object Value = Property.GetValue(Entity, null);

                sb.Append(" [" + Name + "]=" + ParameterPrefix + Name + ",");

                Command.Parameters.Add(CreateParameter(ParameterPrefix + Name, Value));
            }
            object pkValue = ReflectionUtils.GetProperty(Entity, KeyField);

            String CommandText = sb.ToString().TrimEnd(',') + " where " + KeyField + "=" + ParameterPrefix + "__PK";

            Command.Parameters.Add(CreateParameter(ParameterPrefix + "__PK", pkValue));
            Command.CommandText = CommandText;

            bool Result = ExecuteNonQuery(Command) > -1;
            CloseConnection(Command);

            return Result;
        }

        /// <summary>
        /// Inserts an object into the database based on its type information.
        /// The properties must match the database structure and you can skip
        /// over fields in the FieldsToSkip list.        
        /// </summary>        
        /// <seealso cref="SaveEntity">
        /// <seealso cref="UpdateEntity"/>
        /// <param name="entity"></param>
        /// <param name="table"></param>
        /// <param name="KeyField"></param>
        /// <param name="fieldsToSkip"></param>
        /// <returns>Scope Identity or Null</returns>
        public object InsertEntity(object entity, string table, string fieldsToSkip = null)
        {
            this.SetError();

            if (string.IsNullOrEmpty(fieldsToSkip))
                fieldsToSkip = string.Empty;
            else
                fieldsToSkip = "," + fieldsToSkip.ToLower() + ",";

            DbCommand Command = CreateCommand(string.Empty);

            Type ObjType = entity.GetType();

            StringBuilder FieldList = new StringBuilder();
            StringBuilder DataList = new StringBuilder();
            FieldList.Append("insert " + table + " (");
            DataList.Append(" values (");

            PropertyInfo[] Properties = ObjType.GetProperties(BindingFlags.Instance | BindingFlags.Public);
            foreach (PropertyInfo Property in Properties)
            {
                if (!Property.CanRead)
                    continue;

                string Name = Property.Name;

                if (fieldsToSkip.IndexOf("," + Name.ToLower() + ",") > -1)
                    continue;

                object Value = Property.GetValue(entity, null);

                FieldList.Append("[" + Name + "],");
                DataList.Append(ParameterPrefix + Name + ",");

                Command.Parameters.Add(CreateParameter(ParameterPrefix + Name, Value == null ? DBNull.Value : Value));
            }

            Command.CommandText = FieldList.ToString().TrimEnd(',') + ") " +
                                 DataList.ToString().TrimEnd(',') + ");select SCOPE_IDENTITY()";

            object Result = ExecuteScalar(Command);
                     

            CloseConnection();

            return Result;
        }


        /// <summary>
        /// Saves an enity into the database using insert or update as required.
        /// Requires a keyfield that exists on both the entity and the database.
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="table"></param>
        /// <param name="keyField"></param>
        /// <param name="fieldsToSkip"></param>
        /// <returns></returns>
        public virtual bool SaveEntity(object entity, string table, string keyField, string fieldsToSkip = null)
        {
            object pkValue = ReflectionUtils.GetProperty(entity, keyField);
            object res = null;
            if (pkValue != null)
                res = this.ExecuteScalar("select [" + keyField + "] from [" + table + "] where [" + keyField + "]=" + ParameterPrefix + "id",
                                         this.CreateParameter(ParameterPrefix + "id", pkValue));


            if (res == null)
            {
                this.InsertEntity(entity, table, fieldsToSkip);
                if (!string.IsNullOrEmpty(ErrorMessage))
                    return false;
            }
            else
                return this.UpdateEntity(entity, table, keyField, fieldsToSkip);

            return true;
        }


        #endregion



        /// <summary>
        /// Starts a new transaction on this connection/instance
        /// </summary>
        /// <returns></returns>
        public virtual bool BeginTransaction()
        {
            if (_Connection == null)
            {
                if (!this.OpenConnection())
                    return false;
            }            

            Transaction = _Connection.BeginTransaction();
            if (Transaction == null)
                return false;

            return true;
        }

        /// <summary>
        /// Commits all changes to the database and ends the transaction
        /// </summary>
        /// <returns></returns>
        public virtual bool CommitTransaction()
        {
            if (Transaction == null)
            {
                SetError("No active Transaction to commit.");
                return false;
            }

            Transaction.Commit();
            Transaction = null;

            CloseConnection();

            return true;
        }

        /// <summary>
        /// Rolls back a transaction
        /// </summary>
        /// <returns></returns>
        public virtual bool RollbackTransaction()
        {
            if (Transaction == null)
                return true;

            Transaction.Rollback();
            Transaction = null;

            CloseConnection();

            return true;
        }



        /// <summary>
        /// Sets the error message for the failure operations
        /// </summary>
        /// <param name="Message"></param>
        protected virtual void SetError(string Message, int errorNumber)
        {
            if (string.IsNullOrEmpty(Message))
            {
                ErrorMessage = string.Empty;
                ErrorNumber = 0;
                return;
            }

            ErrorMessage = Message;
            ErrorNumber = errorNumber;
        }        

        /// <summary>
        /// Sets the error message and error number.
        /// </summary>
        /// <param name="message"></param>
        protected virtual void SetError(string message)
        {
            SetError(message,0);
        }

        protected virtual void SetError(DbException ex)
        {
            SetError(ex.Message, ex.ErrorCode);
        }

        protected virtual void SetError(Exception ex)
        {
            SetError(ex.Message,0);
        }

        /// <summary>
        /// Sets the error message for failure operations.
        /// </summary>
        protected virtual void SetError()
        {
            SetError(null,0);
        }


        #region IDisposable Members

        public void Dispose()
        {
            if (_Connection != null)
                CloseConnection();
        }


        #endregion
    }
}