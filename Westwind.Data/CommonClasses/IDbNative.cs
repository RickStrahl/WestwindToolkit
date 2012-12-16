using System;
using System.Linq;
using System.Data.Common;
using System.Data;

namespace Westwind.Data
{
    /// <summary>
    /// Interface that describes a
    /// </summary>
    public interface IDbNative
    {
        /// <summary>
        /// Error Message set on error of operations
        /// </summary>
        string ErrorMessage { get; set; }

        /// <summary>
        /// Opens a connection to the database
        /// </summary>
        /// <returns></returns>
        bool OpenConnection();

        /// <summary>
        /// Opens the connection on this data context
        /// </summary>
        /// <returns></returns>
        void CloseConnection();

        /// <summary>
        /// Begins a new transaction on this instance
        /// </summary>
        /// <returns></returns>
        bool BeginTransaction();

        /// <summary>
        /// Commits a transaction on this instance
        /// </summary>
        /// <returns></returns>
        bool CommitTransaction();

        /// <summary>
        /// Rollsback a transaction on this instance
        /// </summary>
        /// <returns></returns>
        bool RollbackTransaction();

        /// <summary>
        /// Creates a new SQL Command
        /// </summary>
        /// <param name="sqlCommand"></param>
        /// <param name="dbParameters"></param>
        /// <returns></returns>
        DbCommand CreateCommand(string sql, params DbParameter[] dbParameters);

        /// <summary>
        /// Creates a new SQL Parameter
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        DbParameter CreateParameter(string name, object value);

        /// <summary>
        /// Creates a new SQL Parameter
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <param name="direction"></param>
        /// <returns></returns>
        DbParameter CreateParameter(string name, object value, ParameterDirection direction);
        /// <summary>
        /// Retrieves a DbCommand from an IQueryable. 
        /// 
        /// Note: This routine may have parameter mapping imperfections 
        /// due to the limited parameter data available in the query's
        /// parameter collection.
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        DbCommand GetCommand(IQueryable query);
        /// <summary>
        /// Returns a DbDataReader from a SQL statement.
        /// 
        /// Note:
        /// Reader is created with CommandBehavior.CloseConnection
        /// </summary>
        /// <param name="sqlCommand"></param>
        /// <param name="dbParameters"></param>
        /// <returns>DataReader or Null.</returns>
        DbDataReader ExecuteReader(DbCommand sqlCommand, params DbParameter[] dbParameters);
        /// <summary>
        /// Returns a DbDataReader from a SQL statement.
        /// 
        /// Note:
        /// Reader is created with CommandBehavior.CloseConnection
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="dbParameters"></param>
        /// <returns></returns>
        DbDataReader ExecuteReader(string sql, params DbParameter[] dbParameters);
        /// <summary>
        /// Executes a LINQ to SQL query and returns the results as a DataReader.
        /// </summary>
        /// <param name="query">LINQ query object</param>
        /// <returns></returns>
        DbDataReader ExecuteReader(IQueryable query);
        /// <summary>
        /// Executes a SQL command and retrieves a DataTable of the result
        /// </summary>
        /// <param name="sqlCommand"></param>
        /// <param name="tableName"></param>
        /// <param name="dbParameters"></param>
        /// <returns></returns>
        DataTable ExecuteDataTable(DbCommand sqlCommand, string tableName, params DbParameter[] dbParameters);
        /// <summary>
        /// Executes a SQL command from a string and retrieves a DataTable of the result
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="tableName"></param>
        /// <param name="dbParameters"></param>
        DataTable ExecuteDataTable(string sql, string tableName, params DbParameter[] dbParameters);
        /// <summary>
        /// Creates a DataTable from a Linq Query expression
        /// </summary>
        /// <param name="query">A LINQ to SQL query object</param>
        /// <param name="tableName">The resulting table name.</param>
        /// <returns></returns>
        DataTable ExecuteDataTable(IQueryable query, string tableName);
        /// <summary>
        /// Runs a query and returns a table in a DataSet either passed in or
        /// by creating a new one.
        /// </summary>
        /// <param name="sqlCommand">SQL Command object</param>
        /// <param name="dataSet">Dataset to add table to</param>
        /// <param name="tableName">Name of the result table</param>
        /// <param name="dbParameters">Optional SQL statement parameters</param>
        /// <returns></returns>
        DataSet ExecuteDataSet(DbCommand sqlCommand, DataSet dataSet, string tableName, params DbParameter[] dbParameters);
        /// <summary>
        /// Runs a query and returns a table in a DataSet either passed in or
        /// by creating a new one.
        /// </summary>
        /// <param name="sqlCommand">SQL string to execute</param>
        /// <param name="dataSet">Dataset to add table to</param>
        /// <param name="tableName">Name of the result table</param>
        /// <param name="dbParameters">Optional SQL statement parameters</param>
        /// <returns></returns>
        DataSet ExecuteDataSet(string sql, DataSet dataSet, string tableName, params DbParameter[] dbParameters);
        /// <summary>
        /// Runs a query and returns a table in a DataSet either passed in or
        /// by creating a new one.
        /// </summary>
        /// <param name="sqlCommand">SQL string to execute</param>
        /// <param name="dataSet">Dataset to add table to</param>
        /// <param name="tableName">Name of the result table</param>
        /// <param name="dbParameters">Optional SQL statement parameters</param>
        /// <returns></returns>
        DataSet ExecuteDataSet(IQueryable query, DataSet dataSet, string tableName);
        /// <summary>
        /// Executes a raw Sql Command against the server that doesn't return
        /// a result set.
        /// </summary>
        /// <param name="sqlCommand"></param>
        /// <param name="dbParameters"></param>
        /// <returns>-1 on error, records affected otherwise</returns>
        int ExecuteNonQuery(DbCommand sqlCommand, params DbParameter[] dbParameters);
        /// <summary>
        /// Executes a raw Sql Command against the server that doesn't return
        /// a result set.
        /// </summary>
        /// <param name="sqlCommand"></param>
        /// <param name="dbParameters"></param>
        /// <returns>-1 on error. Records affected otherwise</returns>
        int ExecuteNonQuery(string sql, params DbParameter[] dbParameters);
        /// <summary>
        /// Executes a SQL Query and returns a single result value that isn't
        /// part of a result cursor
        /// </summary>
        /// <param name="command"></param>
        /// <param name="dbParameters"></param>
        /// <returns></returns>
        object ExecuteScalar(DbCommand command, params DbParameter[] dbParameters);
        /// <summary>
        /// Executes a SQL QUery from a string and returns a single value
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="dbParameters"></param>
        /// <returns></returns>
        object ExecuteScalar(string sql, params DbParameter[] dbParameters);
    }
}
