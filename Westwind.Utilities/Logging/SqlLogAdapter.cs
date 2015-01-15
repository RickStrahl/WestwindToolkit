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
using Westwind.Utilities.Data;
using System.Drawing;
using System.Data.Common;
using System.Collections.Generic;
using System.Reflection;
using System.Data.SqlClient;

namespace Westwind.Utilities.Logging
{
    /// <summary>
    /// Log adapter that writes to a SQL Server Database
    /// </summary>
    public class SqlLogAdapter : ILogAdapter        
    {
        public string ConnectionString
        {
            get { return _ConnectionString; }
            set { _ConnectionString = value; }
        }
        private string _ConnectionString = "";

        /// <summary>
        /// The name of the table that data in SQL Server is written to
        /// </summary>
        public string LogFilename
        {
            get { return _LogFilename; }
            set { _LogFilename = value; }
        }
        private string _LogFilename = "ApplicationLog";


        /// <summary>
        /// Must pass in a SQL Server connection string or 
        /// config ConnectionString Id.
        /// </summary>
        /// <param name="connectionString"></param>        
        public SqlLogAdapter(string connectionString)
        {
            ConnectionString = connectionString;
            LogFilename = LogManagerConfiguration.Current.LogFilename;
        }
        public SqlLogAdapter(string connectionString, string tableName)
        {
            ConnectionString = connectionString;
            LogFilename = tableName;
        }
         /// <summary>
        /// this version configures itself from the LogManager 
        /// configuration section
        /// </summary>
        public SqlLogAdapter()
        {
            ConnectionString = LogManagerConfiguration.Current.ConnectionString;
            LogFilename = LogManagerConfiguration.Current.LogFilename;
        }


        /// <summary>
        /// Internally creates and configures an instance of the DAL used for data access
        /// </summary>
        /// <returns></returns>
        private SqlDataAccess CreateDal()
        {
            SqlDataAccess dal = new SqlDataAccess(ConnectionString);
            return dal;
        }

        #region ILogAdapter Members



        /// <summary>
        /// Writes a new Web specific entry into the log file
        /// 
        /// Assumes that your log file is set up to be a Web Log file
        /// </summary>
        /// <param name="webEntry"></param>
        /// <returns></returns>
        /// <exception cref="System.InvalidOperationException">Thrown if the insert operation fails</exception>
        public bool WriteEntry(WebLogEntry entry)
        {
            using (SqlDataAccess data = CreateDal())
            {
                string sql = string.Format(@"
insert into [{0}] (Entered,Message,ErrorLevel,Details,ErrorType,StackTrace,IpAddress,UserAgent,Url,QueryString,Referrer,PostData,RequestDuration) values
                (@Entered,@Message,@ErrorLevel,@Details,@ErrorType,@StackTrace,@IpAddress,@UserAgent,@Url,@QueryString,@Referrer,@PostData,@RequestDuration)
select CAST(scope_identity() as integer)
", LogFilename);


                object result = data.ExecuteScalar(sql,
                    data.CreateParameter("@Entered", entry.Entered, DbType.DateTime),
                    data.CreateParameter("@Message", StringUtils.TrimTo(entry.Message, 255), 255),
                    data.CreateParameter("@ErrorLevel", entry.ErrorLevel),
                    data.CreateParameter("@Details", StringUtils.TrimTo(entry.Details, 4000), 4000),
                    data.CreateParameter("@ErrorType", entry.ErrorType),
                    data.CreateParameter("@StackTrace", StringUtils.TrimTo(entry.StackTrace, 1500), 1500),
                    data.CreateParameter("@IpAddress", entry.IpAddress),
                    data.CreateParameter("@UserAgent", StringUtils.TrimTo(entry.UserAgent, 255)),
                    data.CreateParameter("@Url", entry.Url),
                    data.CreateParameter("@QueryString", StringUtils.TrimTo(entry.QueryString, 255)),
                    data.CreateParameter("@Referrer", entry.Referrer),
                    data.CreateParameter("@PostData", StringUtils.TrimTo(entry.PostData, 2048), 2048),
                    data.CreateParameter("@RequestDuration", entry.RequestDuration)
                );


                // check for table missing and retry
                if (data.ErrorNumber == 208)
                {
                    // if the table could be created try again
                    if (CreateLog())
                        return WriteEntry(entry);
                }

                if (result == null || result == DBNull.Value)
                    throw new InvalidOperationException("Unable add log entry into table " + LogFilename + ". " + data.ErrorMessage);

                // Update id
                entry.Id = (int)result;

                return true;
            }
        }

    

        /// <summary>
        /// Returns an individual Web log entry from the log table
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public WebLogEntry GetEntry(int id)
        {
            using (SqlDataAccess data = CreateDal())
            {
                WebLogEntry entry = new WebLogEntry();
                if (!data.GetEntity(entry, LogFilename, "Id", id, null))
                    return null;

                return entry;
            }
        }
               

        /// <summary>
        /// Returns entries for a given error level, and date range
        /// </summary>
        /// <param name="errorLevel"></param>
        /// <param name="count"></param>
        /// <param name="dateFrom"></param>
        /// <param name="dateTo"></param>
        /// <returns></returns>
        public IDataReader GetEntries(ErrorLevels errorLevel = ErrorLevels.All, 
                                      int count = 200, 
                                      DateTime? dateFrom = null, 
                                      DateTime? dateTo = null,
                                      string fieldList = null)
        {
            if (dateFrom == null)
                dateFrom = DateTime.Now.Date.AddDays(-2);
            if (dateTo ==null)
                dateTo = DateTime.Now.Date.AddDays(1);
            if (fieldList == null)
                fieldList = "*";

            SqlDataAccess data = CreateDal();
            
            string sql = string.Format("select TOP {1} {2} from [{0}] where " +
                                           (errorLevel != ErrorLevels.All ? "ErrorLevel = @ErrorLevel and " : "") +
                                           "Entered >= @dateFrom and Entered < @dateTo " +
                                           "order by Entered DESC", LogFilename, count, fieldList);
            
            var reader = data.ExecuteReader(sql,
                data.CreateParameter("@ErrorLevel", (int)errorLevel),
                data.CreateParameter("@dateFrom", dateFrom.Value.Date),
                data.CreateParameter("@dateTo", dateTo.Value.AddDays(1).Date));
   
            return reader;            
        }

        //IEnumerable<WebLogEntry> GetEntryList(ErrorLevels errorLevel, int count, DateTime dateFrom, DateTime dateTo, string FieldList);
        public IEnumerable<WebLogEntry> GetEntryList(ErrorLevels errorLevel = ErrorLevels.All,
                                      int count = 200,
                                      DateTime? dateFrom = null,
                                      DateTime? dateTo = null,
                                      string fieldList = null)
        {
            IDataReader reader = GetEntries(errorLevel, count, dateFrom, dateTo, fieldList);

            if (reader == null || reader.IsClosed)
            {
                yield break;
            }

            Dictionary<string, PropertyInfo> piList = new Dictionary<string, PropertyInfo>();
            while (reader.Read())
            {
                var entry = new WebLogEntry();
                DataUtils.DataReaderToObject(reader, entry, null, piList);
                yield return entry;
            }

            reader.Close();
        }

        /// <summary>
        /// Creates a new log table in the current database. If the table exists already it
        /// is dropped and recreated.
        /// 
        /// Requires database admin access.
        /// </summary>
        /// <param name="logType"></param>
        /// <returns></returns>
        public bool CreateLog()
        {
            using (SqlDataAccess data = CreateDal())
            {
                // try to drop the log table first
                try
                {
                    DeleteLog();
                }
                catch { } // ignore InvalidOperation

                string sql = string.Format(STR_ApplicationWebLogCreateStatement, LogFilename);
                int result = data.ExecuteNonQuery(sql);
                if (result < 0)
                    throw new InvalidOperationException("Failed to create Application Log Table: " + data.ErrorMessage);                                
            }

            return true;
        }

        /// <summary>
        /// Deletes the Sql Log Table
        /// </summary>
        /// <param name="logType"></param>
        /// <returns></returns>
        public bool DeleteLog()
        {
            using (SqlDataAccess data = CreateDal())
            {
                string sql = "DROP TABLE " + LogFilename;
                int result = data.ExecuteNonQuery(sql);
                if (result < 0)
                    throw new InvalidOperationException("Failed to create Application Log Table: " + data.ErrorMessage);

            }
            return true;
        }


        /// <summary>
        /// Clears all the records of the log table
        /// </summary>
        /// <returns></returns>
        public bool Clear()
        {
            using (SqlDataAccess data = CreateDal())
            {
                if (data.ExecuteNonQuery("delete [" + LogFilename + "]") < 0)
                    throw new InvalidOperationException("Failed to delete table" + LogFilename + ". " + data.ErrorMessage);

            }
            return true;
        }

        /// <summary>
        /// Clears the table and leaves the last number of records specified intact
        /// </summary>
        /// <param name="countToLeave"></param>
        /// <returns></returns>
        public bool Clear(int countToLeave)
        {
            string sql = "delete [{0}] where Id not in (select top {1} Id from [{0}] order by entered desc)";
            sql = string.Format(sql,LogFilename,countToLeave);

            using (SqlDataAccess data = CreateDal())
            {
                if (data.ExecuteNonQuery(sql) < 0)
                    throw new InvalidOperationException("Failed to remove entries. " + data.ErrorMessage);
            }

            return true;
        }

        public bool Clear(decimal daysToDelete)
        {
            
            var date = DateTime.UtcNow.Date.AddDays((int) daysToDelete * -1);
            string sql = "delete [{0}] where entered < @date";
            sql = string.Format(sql, LogFilename);

            using (SqlDataAccess data = CreateDal())
            {
                if (data.ExecuteNonQuery(sql,data.CreateParameter("@date",date)) < 0)
                    throw new InvalidOperationException("Failed to remove entries. " + data.ErrorMessage);
            }

            return true;
            
        }
        
        /// <summary>
        /// Returns the number of total log entries
        /// </summary>
        /// <returns></returns>
        public int GetEntryCount(ErrorLevels errorLevel = ErrorLevels.All)
        {
            using (SqlDataAccess data = CreateDal())
            {
                string sql = "select count(id) from " + LogFilename;
                DbParameter[] parms = null;

                if (!(errorLevel == ErrorLevels.All || errorLevel == ErrorLevels.None))
                {
                    sql = sql + " where errorlevel = @ErrorLevel";
                    parms = new DbParameter[1]
                        { data.CreateParameter("@ErrorLevel",(int) errorLevel) };
                }

                object result = data.ExecuteScalar(sql, parms);
                if (result == null)
                    throw new InvalidOperationException("Failed to count entries. " + data.ErrorMessage);

                return (int)result;
            }

        }

        #endregion



        public const string STR_ApplicationWebLogCreateStatement = @"
CREATE TABLE [dbo].[{0}](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Entered] [datetime] NOT NULL,
	[Message] [nvarchar](255) NULL,
	[ErrorLevel] [int] NOT NULL,
	[Details] [nvarchar](4000) NULL,
    [ErrorType] [nvarchar](50) NULL,
    [StackTrace] [nvarchar] (1500) NULL,
	[Url] [nvarchar](255) NULL,
	[QueryString] [nvarchar](255) NULL,
	[IpAddress] [nvarchar](20) NULL,
	[Referrer] [nvarchar](255) NULL,
	[UserAgent] [nvarchar](255) NULL,
	[PostData] [nvarchar](2048) NULL,
	[RequestDuration] [decimal](9, 3) NOT NULL
) ON [PRIMARY]

ALTER TABLE [dbo].[{0}] ADD  CONSTRAINT [DF_{0}_Entered]  DEFAULT (getdate()) FOR [Entered]
ALTER TABLE [dbo].[{0}] ADD  CONSTRAINT [DF_{0}_ErrorLevel]  DEFAULT ((0)) FOR [ErrorLevel]
ALTER TABLE [dbo].[{0}] ADD  CONSTRAINT [DF_{0}_RequestDuration]  DEFAULT ((-1)) FOR [RequestDuration]
";

    }
}
