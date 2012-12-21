using System;
using System.Linq;
using System.Data.Common;
using System.Data;
using System.Data.Objects;
using System.Data.SqlClient;
using System.Collections.Generic;

namespace Westwind.Data
{
    /// <summary>
    /// Sql Server Native Data Connector implementation layered
    /// ontop of plain old ADO.NET.
    /// </summary>
    public class SqlNative : IDbNative
    {
        public SqlNative(DbConnection connection)
        {
            Connection = connection;
            ParameterPrefix = "@";
        }

        /// <summary>
        /// Internal Provider factory for native commands
        /// </summary>
        public static DbProviderFactory DbProvider = DbProviderFactories.GetFactory("System.Data.SqlClient");

        /// <summary>
        /// Internal locking object
        /// </summary>
        private object _syncLock = new object();

        /// <summary>
        /// Active Connection if any
        /// </summary>
        public DbConnection Connection { get; set; }

        /// <summary>
        /// Active Transaction if any
        /// </summary>
        public DbTransaction Transaction { get; set;  }

        /// <summary>
        /// The parameter prefix character used for SQL parameters
        /// </summary>
        public string ParameterPrefix { get; set; }

        /// <summary>
        /// Error Message if an error occurs
        /// </summary>
        public string ErrorMessage { get; set; }

        #region Base Operations (open/close connections, Parameters, Commands)

        /// <summary>
        /// Opens a connection to the database
        /// </summary>
        /// <returns></returns>
        public bool OpenConnection()
        {
            if (Transaction != null && Transaction.Connection != null)            
                this.Connection = Transaction.Connection;

            try
            {
                if (this.Connection.State != ConnectionState.Open)
                    this.Connection.Open();
            }
            catch (Exception ex)
            {
                SetError(ex);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Opens the connection on this data context
        /// </summary>
        /// <returns></returns>
        public void CloseConnection()
        {
            if (Transaction == null)
                this.Connection.Close();
        }


        /// <summary>
        /// Starts a new transaction on this connection/instance.
        /// 
        /// NOTE: provided only for ADO.NET style transactions
        /// LINQ to SQL will create its own connection instances
        /// and will close open transactions on its own.
        /// </summary>
        /// <returns></returns>
        public bool BeginTransaction()
        {
            if (!this.OpenConnection())
                return false;

            this.Transaction = Connection.BeginTransaction();
            if (Transaction == null)
                return false;

            return true;
        }

        /// <summary>
        /// Commits all changes to the database and ends the transaction
        /// </summary>
        /// <returns></returns>
        public bool CommitTransaction()
        {
            if (this.Transaction == null)
                return false;

            this.Transaction.Commit();
            this.Transaction = null;
            this.CloseConnection();

            return true;
        }


        /// <summary>
        /// Rolls back a transaction
        /// </summary>
        /// <returns></returns>
        public bool RollbackTransaction()
        {
            if (this.Transaction == null)
                return true;

            this.Transaction.Rollback();
            this.Transaction = null;
            this.CloseConnection();

            return true;
        }

        ///// <summary>
        ///// Pass positional parameters
        ///// </summary>
        ///// <param name="sql"></param>
        ///// <param name="dbParameters"></param>
        ///// <returns></returns>
        //public DbCommand CreateCommandWithValues(string sql, params object[] parameters)
        //{
        //    if (parameters == null || parameters.Length < 1)
        //        return CreateCommand(sql);

        //    var parmList = new List<DbParameter>();
        //    int count = 0;
        //    foreach (object parm in parameters)
        //    {
        //        parmList.Add(this.CreateParameter(ParameterPrefix + count.ToString(), parm));
        //    }

        //    return CreateCommand(sql, parmList.ToArray());
        //}

        /// <summary>
        /// Creates a new SQL Command
        /// </summary>
        /// <param name="sqlCommand"></param>
        /// <param name="parameters">Parameters for the command. Either dbParameter instances or objects linked with @0,@1,@2 etc.</param>
        /// <returns></returns>
        public DbCommand CreateCommand(string sql, params object[] parameters)
        {
            return CreateCommand(sql, CommandType.Text, parameters);
        }

        /// <summary>
        /// Creates a new SQL Command
        /// </summary>
        /// <param name="sqlCommand"></param>
        /// <param name="parameters">Parameters for the command. Either dbParameter instances or objects linked with @0,@1,@2 etc.</param>
        /// <returns></returns>
        public DbCommand CreateCommand(string sql, CommandType commandType, params object[] parameters)
        {
            DbCommand sqlCommand = Connection.CreateCommand();
            sqlCommand.CommandText = sql;
            sqlCommand.Connection = Connection;

            if (parameters != null)
                AddParameters(sqlCommand, parameters);

            if (Transaction != null)
                sqlCommand.Transaction = Transaction;

            return sqlCommand;
        }
    

        /// <summary>
        /// Creates a new SQL Parameter
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <param name="direction"></param>
        /// <returns></returns>
        public DbParameter CreateParameter(string name, object value, ParameterDirection direction = ParameterDirection.Input)
        {
            DbParameter parm = DbProvider.CreateParameter();
            parm.ParameterName = name;
            parm.Value = value;
            parm.Direction = direction;
            return parm;
        }


        /// <summary>
        /// Retrieves a DbCommand from an IQueryable. 
        /// 
        /// Note: This routine may have parameter mapping imperfections 
        /// due to the limited parameter data available in the query's
        /// parameter collection.
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public DbCommand GetCommand(IQueryable query)
        {
            ObjectQuery q = query as ObjectQuery;            

            if (q == null)
                throw new InvalidCastException("Query could not be converted to an ObjectQuery");

            DbCommand command = Connection.CreateCommand();
            command.CommandText = q.ToTraceString();

            foreach (var parm in q.Parameters)
            {
                DbParameter queryParm = this.CreateParameter(parm.Name, parm.Value);

                command.Parameters.Add(parm);
            }

            return command;
        }
        #endregion

        #region Query Operations

        /// <summary>
        /// Returns a DbDataReader from a SQL statement.
        /// 
        /// Note:
        /// Reader is created with CommandBehavior.CloseConnection
        /// </summary>
        /// <param name="sqlCommand"></param>
        /// <param name="parameters"></param>
        /// <returns>DataReader or Null.</returns>
        public DbDataReader ExecuteReader(DbCommand sqlCommand, params object[] parameters)
        {
            if (parameters != null)       
                AddParameters(sqlCommand,parameters);

            if (!this.OpenConnection())
                return null;

            try
            {
                return sqlCommand.ExecuteReader(CommandBehavior.CloseConnection);
            }
            catch (Exception ex)
            {
                SetError(ex, true);
                return null;
            }
        }

        /// <summary>
        /// Returns a DbDataReader from a SQL statement.
        /// 
        /// Note:
        /// Reader is created with CommandBehavior.CloseConnection
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="dbParameters"></param>
        /// <returns></returns>
        public DbDataReader ExecuteReader(string sql, params object[] parameters)
        {
            return this.ExecuteReader(CreateCommand(sql, parameters));
        }

        /// <summary>
        /// Executes a LINQ to SQL query and returns the results as a DataReader.
        /// </summary>
        /// <param name="query">LINQ query object</param>        
        /// <returns></returns>
        public DbDataReader ExecuteReader(IQueryable query)
        {
            return this.ExecuteReader(this.GetCommand(query));
        }

        /// <summary>
        /// Executes a SQL command and retrieves a DataTable of the result
        /// </summary>
        /// <param name="sqlCommand"></param>
        /// <param name="tableName"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public DataTable ExecuteDataTable(DbCommand sqlCommand, string tableName, params object[] parameters)
        {
            if (parameters != null)
                AddParameters(sqlCommand,parameters);
            
            DbDataAdapter Adapter = DbProvider.CreateDataAdapter();
            Adapter.SelectCommand = sqlCommand;

            DataTable dt = new DataTable(tableName);

            try
            {
                if (!this.OpenConnection())
                    return null;

                Adapter.Fill(dt);
            }
            catch (Exception ex)
            {
                SetError(ex, true);
                return null;
            }
            finally
            {
                this.CloseConnection();
            }

            return dt;
        }

        /// <summary>
        /// Executes a SQL command from a string and retrieves a DataTable of the result
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="tableName"></param>
        /// <param name="dbParameters"></param>
        public DataTable ExecuteDataTable(string sql, string tableName, params object[] parameters)
        {
            return this.ExecuteDataTable(this.CreateCommand(sql,parameters), tableName);
        }

        /// <summary>
        /// Creates a DataTable from a Linq Query expression
        /// </summary>
        /// <param name="query">A LINQ to SQL query object</param>
        /// <param name="tableName">The resulting table name.</param>
        /// <returns></returns>
        public DataTable ExecuteDataTable(IQueryable query, string tableName)
        {
            return this.ExecuteDataTable(this.GetCommand(query), tableName);
        }

        /// <summary>
        /// Runs a query and returns a table in a DataSet either passed in or
        /// by creating a new one.
        /// </summary>
        /// <param name="sqlCommand">SQL Command object</param>
        /// <param name="dataSet">Dataset to add table to</param>
        /// <param name="tableName">Name of the result table</param>
        /// <param name="dbParameters">Optional SQL statement parameters</param>
        /// <returns></returns>
        public DataSet ExecuteDataSet(DbCommand sqlCommand, DataSet dataSet, string tableName, params object[] parameters)
        {
            if (dataSet == null)
                dataSet = new DataSet();

            DbDataAdapter Adapter = DbProvider.CreateDataAdapter();
            Adapter.SelectCommand = sqlCommand;

            if(parameters != null)
                AddParameters(sqlCommand,parameters);
            
            DataTable dt = new DataTable(tableName);

            if (dataSet.Tables.Contains(tableName))
                dataSet.Tables.Remove(tableName);

            try
            {
                this.OpenConnection();
                Adapter.Fill(dataSet, tableName);
            }
            catch (Exception ex)
            {
                SetError(ex, true);
                return null;
            }
            finally
            {
                this.CloseConnection();
            }

            return dataSet;
        }


        /// <summary>
        /// Runs a query and returns a table in a DataSet either passed in or
        /// by creating a new one.
        /// </summary>
        /// <param name="sqlCommand">SQL string to execute</param>
        /// <param name="dataSet">Dataset to add table to</param>
        /// <param name="tableName">Name of the result table</param>
        /// <param name="dbParameters">Optional SQL statement parameters</param>
        /// <returns></returns>
        public DataSet ExecuteDataSet(string sql, DataSet dataSet, string tableName, params object[] parameters)
        {
            return this.ExecuteDataSet(this.CreateCommand(sql,parameters), dataSet, tableName);
        }

        /// <summary>
        /// Runs a query and returns a table in a DataSet either passed in or
        /// by creating a new one.
        /// </summary>
        /// <param name="sqlCommand">SQL string to execute</param>
        /// <param name="dataSet">Dataset to add table to</param>
        /// <param name="tableName">Name of the result table</param>
        /// <param name="dbParameters">Optional SQL statement parameters</param>
        /// <returns></returns>
        public DataSet ExecuteDataSet(IQueryable query, DataSet dataSet, string tableName)
        {
            return this.ExecuteDataSet(this.GetCommand(query), dataSet, tableName);
        }


        /// <summary>
        /// Executes a raw Sql Command against the server that doesn't return
        /// a result set.
        /// </summary>
        /// <param name="sqlCommand"></param>
        /// <param name="dbParameters"></param>
        /// <returns>-1 on error, records affected otherwise</returns>
        public int ExecuteNonQuery(DbCommand sqlCommand, params object[] parameters)
        {
            int RecordCount = 0;

            if (parameters != null)
                AddParameters(sqlCommand, parameters);

            try
            {
                if (!this.OpenConnection())
                    return -1;

                RecordCount = sqlCommand.ExecuteNonQuery();                
            }
            catch (Exception ex)
            {
                SetError(ex, true);
                return -1;
            }

            finally
            {
                this.CloseConnection();
            }

            return RecordCount;
        }

        /// <summary>
        /// Executes a raw Sql Command against the server that doesn't return
        /// a result set.
        /// </summary>
        /// <param name="sqlCommand"></param>
        /// <param name="parameters"></param>
        /// <returns>-1 on error. Records affected otherwise</returns>
        public int ExecuteNonQuery(string sql, params object[] parameters)
        {
            return this.ExecuteNonQuery(this.CreateCommand(sql, parameters));
        }

        /// <summary>
        /// Executes a SQL Query and returns a single result value that isn't
        /// part of a result cursor
        /// </summary>
        /// <param name="command"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public virtual object ExecuteScalar(DbCommand command, params object[] parameters)
        {
            if (command.Connection.State != ConnectionState.Open)
                this.OpenConnection();

            if (parameters != null)
                AddParameters(command, parameters);

            object result;
            try
            {
                result = command.ExecuteScalar();
            }
            catch (Exception ex)
            {
                SetError(ex, true);
                return null;
            }
            finally
            {
                this.CloseConnection();
            }

            return result;
        }

        /// <summary>
        /// Executes a SQL QUery from a string and returns a single value
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="dbParameters"></param>
        /// <returns></returns>
        public object ExecuteScalar(string sql, params object[] parameters)
        {
            return this.ExecuteScalar(this.CreateCommand(sql, parameters));
        }

        #endregion

        #region EntityRoutines
        #endregion

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


        protected void SetError()
        {
            this.SetError(string.Empty);
        }

        protected void SetError(string message)
        {
            if (string.IsNullOrEmpty(message))
            {
                this.ErrorMessage = string.Empty;
                return;
            }
            this.ErrorMessage = message;
        }

        protected void SetError(Exception ex, bool checkInner = false)
        {
            if (ex == null)
                this.ErrorMessage = string.Empty;

            Exception e = ex;
            if (checkInner)
                e = e.GetBaseException();

            ErrorMessage = e.Message;
        }

        protected void SetError(Exception ex)
        {
            SetError(ex, false);
        }
    }
}
