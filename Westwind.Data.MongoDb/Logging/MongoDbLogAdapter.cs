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
using System.Linq;
using System.Data;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using MongoDB.Driver.Linq;
using Westwind.Data.MongoDb;
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
    public class MongoDbLogAdapter : ILogAdapter
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
        /// Instance of the MongoDb core database instance.
        /// </summary>
        public MongoDbBusinessBase<WebLogEntry, MongoDbContext> MongoBusiness { get; set; }


        /// <summary>
        /// Must pass in a SQL Server connection string or 
        /// config ConnectionString Id.
        /// </summary>
        /// <param name="connectionString">Connection string to a MongoDb database</param>        
        /// <param name="tableName">Name of the table to create in MongoDb database</param>        
        public MongoDbLogAdapter(string connectionString, string tableName = null)
        {
            ConnectionString = connectionString;
            if (tableName != null)
                LogFilename = tableName;

            MongoBusiness = new MongoDbBusinessBase<WebLogEntry, MongoDbContext>(connectionString: ConnectionString, collection: LogFilename);
        }
        /// <summary>
        /// this version configures itself from the LogManager 
        /// configuration section
        /// </summary>
        public MongoDbLogAdapter()
        {
            ConnectionString = LogManagerConfiguration.Current.ConnectionString;
            LogFilename = LogManagerConfiguration.Current.LogFilename;

            MongoBusiness = new MongoDbBusinessBase<WebLogEntry, MongoDbContext>(connectionString: ConnectionString, collection: LogFilename);
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
            if (entry.Id == 0)
                entry.Id = Guid.NewGuid().GetHashCode();

            return MongoBusiness.Save(entry);
        }



        /// <summary>
        /// Returns an individual Web log entry from the log table
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public WebLogEntry GetEntry(int id)
        {
            return MongoBusiness.Load(id);
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
            throw new NotImplementedException();
        }

        public IEnumerable<WebLogEntry> GetEntryList(ErrorLevels errorLevel = ErrorLevels.All,
                                      int count = 200,
                                      DateTime? dateFrom = null,
                                      DateTime? dateTo = null,
                                      string fieldList = null)
        {

            if (dateFrom == null)
                dateFrom = DateTime.Now.Date.AddDays(-2);
            if (dateTo == null)
                dateTo = DateTime.Now.Date.AddDays(1);

            var entries = MongoBusiness.Collection.AsQueryable()
                .Where(wl => wl.Entered >= dateFrom && wl.Entered <= dateTo);
            if (errorLevel != ErrorLevels.All)
                entries.Where(wl => wl.ErrorLevel == errorLevel);

            return entries.Take(count).OrderByDescending(wl => wl.Entered);
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

            MongoBusiness.Database.DropCollection(LogFilename);

            var entry = new WebLogEntry()
            {
                Message = "Log file created.",
                ErrorLevel = ErrorLevels.Info
            };

            bool result = MongoBusiness.Save(entry);

            MongoBusiness.Collection.CreateIndex(new string[] { "Entered" });

            return result;
        }

        /// <summary>
        /// Deletes the Sql Log Table
        /// </summary>
        /// <param name="logType"></param>
        /// <returns></returns>
        public bool DeleteLog()
        {
            MongoBusiness.Database.DropCollection(LogFilename);
            return true;
        }


        /// <summary>
        /// Clears all the records of the log table
        /// </summary>
        /// <returns></returns>
        public bool Clear()
        {
            MongoBusiness.Collection.RemoveAll();
            return true;
        }

        /// <summary>
        /// Clears the table and leaves the last number of records specified intact
        /// Note: the count to Leave is ignored
        /// </summary>
        /// <param name="countToLeave"></param>
        /// <returns></returns>
        public bool Clear(int countToLeave)
        {
            if (countToLeave == 0)
                MongoBusiness.Collection.RemoveAll();
            else
            {
                long count = MongoBusiness.Collection.Count();
                var items = MongoBusiness.Collection.AsQueryable()
                    .OrderBy(wl => wl.Entered)
                    .Select(wl => wl.Id)
                    .Take(((int)count - countToLeave))
                    .ToList();
                for (int i = items.Count - 1; i > -1; i--)
                {
                    MongoBusiness.Delete(items[i]);
                }
            }

            return true;
        }

        public bool Clear(decimal daysToDelete)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Returns the number of total log entries
        /// </summary>
        /// <returns></returns>
        public int GetEntryCount(ErrorLevels errorLevel = ErrorLevels.All)
        {
            if (errorLevel == ErrorLevels.All)
                return (int) MongoBusiness.Collection.Count();

            return (int)MongoBusiness.Collection.Count(Query<WebLogEntry>.EQ(wl=> wl.ErrorLevel, errorLevel ));
                
        }

        #endregion
    }
    public class xMongoLogContext : MongoDbContext
    {
        
    }
}
