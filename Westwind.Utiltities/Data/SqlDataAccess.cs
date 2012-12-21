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
            : base(connectionString)
        { }

        public SqlDataAccess(string connectionString, string providerName)
            :base(connectionString,providerName)
        { }
       

        ///// <summary>
        ///// Executes a SQL Command object and returns a SqlDataReader object
        ///// </summary>
        ///// <param name="Command">Command should be created with GetSqlCommand and open connection</param>
        ///// <param name="Parameters"></param>
        ///// <returns></returns>
        ///// <returns>A SqlDataReader. Make sure to call Close() to close the underlying connection.</returns>
        //public override DbDataReader ExecuteReader(DbCommand Command, params DbParameter[] Parameters)
        //{
        //    SetError();

        //    if (Command.Connection == null || Command.Connection.State != ConnectionState.Open)
        //    {
        //        if (!OpenConnection())
        //            return null;

        //        Command.Connection = _Connection;
        //    }

        //    foreach (DbParameter Parameter in Parameters)
        //    {
        //        Command.Parameters.Add(Parameter);
        //    }

        //    DbDataReader Reader;
        //    try
        //    {
        //        Reader = Command.ExecuteReader();
        //    }
        //    catch (SqlException ex)
        //    {
        //        SetError(ex.GetBaseException().Message, ex.Number);
        //        CloseConnection(Command);
        //        return null;
        //    }

        //    return Reader;
        //}



        ///// <summary>
        ///// Executes a command and returns a scalar value from it
        ///// </summary>
        ///// <param name="SqlCommand">A SQL Command object</param>
        ///// <returns>value or null on failure</returns>
        //public override object ExecuteScalar(DbCommand Command, params DbParameter[] Parameters)
        //{
        //    SetError();
            
        //    foreach (DbParameter Parameter in Parameters)
        //    {
        //        Command.Parameters.Add(Parameter);
        //    }

        //    object Result = null;
        //    try
        //    {
        //        Result = Command.ExecuteScalar();
        //    }
        //    catch (SqlException ex)
        //    {
        //        SetError(ex.Message,ex.Number);
        //    }
        //    finally
        //    {
        //        CloseConnection();
        //    }

        //    return Result;
        //}

        /// <summary>
        /// Sql 2005 and later specific semi-generic paging routine
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
