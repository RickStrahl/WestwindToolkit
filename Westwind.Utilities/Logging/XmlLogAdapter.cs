using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Westwind.Utilities.Logging;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using System.Data;
using System.Web;
using System.Reflection;

namespace Westwind.Utilities.Logging
{
    public class XmlLogAdapter : ILogAdapter
    {

        public XmlLogAdapter()
        {
            ConnectionString = LogManagerConfiguration.Current.LogFilename;
            if (string.IsNullOrEmpty(ConnectionString))
                ConnectionString = LogManagerConfiguration.Current.ConnectionString;
        }        

        /// <summary>
        /// The Xml Connection string is the filename
        /// </summary>
        public string ConnectionString
        {
            get
            {
                return _ConnectionString;
            }
            set
            {
                _ConnectionString = value;

                // fix up filename in web path. If no path assume server relative
                if (HttpContext.Current != null && !_ConnectionString.Contains(":\\") )
                {
                    _ConnectionString = _ConnectionString.Replace("\\", "/");
                
                    if (!_ConnectionString.StartsWith("~"))
                        _ConnectionString = "~/" + _ConnectionString;

                    _ConnectionString = HttpContext.Current.Server.MapPath(_ConnectionString);

                    if (Path.GetExtension(_ConnectionString) == string.Empty)
                        _ConnectionString += ".xml";
                }
            }
        }
        private string _ConnectionString = "";

        /// <summary>
        /// The name of the file where we're logging to
        /// </summary>
        public string LogFilename
        {
            get { return ConnectionString; }
            set { ConnectionString = value;}
        }

        private static object _writeLock = new object();

        /// <summary>
        /// Writes an entry to the log
        /// </summary>
        /// <param name="entry"></param>
        /// <returns></returns>
        public bool WriteEntry(WebLogEntry entry)
        {
            lock (_writeLock)
            {
                if (entry.Id == 0)
                    entry.Id = entry.GetHashCode();


                string logFilename = ConnectionString;
                bool writeEndDoc = true;
                FileStream fileStream = null;
                try
                {
                    fileStream = new FileStream(logFilename, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Write);
                    fileStream.Seek(0, SeekOrigin.End);

                    // *** If the file's not empty start writing over the end doc tag
                    // *** We'll rewrite it at the end
                    if (fileStream.Position > 0)
                        fileStream.Seek(-1 * "</ApplicationLog>\r\n".Length, SeekOrigin.End);
                }
                catch
                {
                    return false;
                }

                XmlTextWriter writer = new XmlTextWriter((Stream)fileStream, Encoding.UTF8);
                writer.Formatting = Formatting.Indented;
                writer.IndentChar = ' ';
                writer.Indentation = 4;

                // *** If the file is empty write the root element
                if (fileStream.Position == 0)
                {
                    writer.WriteStartElement("ApplicationLog");
                    writeEndDoc = false; // it'll automatically unwind the StartElement
                }

                writer.WriteStartElement("LogEntry");
                writer.WriteElementString("Id", entry.Id.ToString());

                writer.WriteStartElement("Entered");
                writer.WriteValue(entry.Entered);
                writer.WriteEndElement();

                writer.WriteElementString("Message", entry.Message);
                writer.WriteElementString("ErrorLevel", entry.ErrorLevel.ToString());
                writer.WriteElementString("Details", entry.Details);

                writer.WriteElementString("Url", entry.Url);
                writer.WriteElementString("QueryString", entry.QueryString);
                writer.WriteElementString("UserAgent", entry.UserAgent);
                writer.WriteElementString("Referrer", entry.Referrer);
                writer.WriteElementString("PostData", entry.PostData);
                writer.WriteElementString("IpAddress", entry.IpAddress);
                writer.WriteElementString("RequestDuration", entry.RequestDuration.ToString());
                
                writer.WriteEndElement(); // error


                if (writeEndDoc)
                    writer.WriteRaw("\r\n</ApplicationLog>\r\n");
                else
                {
                    writer.WriteEndElement();
                    writer.WriteRaw("\r\n");
                }

                writer.Close();
                fileStream.Close();

                return true;
            }            
        }

 
        /// <summary>
        /// Returns an individual entry entity
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public WebLogEntry GetEntry(int id)
        {
            XElement doc = XElement.Load(LogFilename);
            XElement match = doc.Descendants("LogEntry")
                                .Where( el => (int) el.Element("Id") == id )
                                .FirstOrDefault();

            DataTable dt = CreateEntryDataTable();
            DataRow dr = dt.NewRow();
            UpdateDataRowFromElement(match, dr);
            dt.Rows.Add(dr);
            DataTableReader reader = new DataTableReader(dt);
            reader.Read();
            
            WebLogEntry entry = new WebLogEntry();
            DataUtils.DataReaderToObject(reader, entry, null);

            return entry;
        }

        public System.Data.IDataReader GetEntries()
        {
            return GetEntries(ErrorLevels.All, 99999999, DateTime.MinValue, DateTime.MaxValue, "*");
        }      

        /// <summary>
        /// Returns a filtered list of XML entries sorted in descending order.
        /// </summary>
        /// <param name="errorLevel">The specific error level to return</param>
        /// <param name="count">Max number of items to return</param>
        /// <param name="dateFrom">From Date</param>
        /// <param name="dateTo">To Date</param>
        /// <param name="fieldList">"*" or any of the fields you want returned - currently not supported</param>
        /// <returns></returns>
        public System.Data.IDataReader GetEntries(ErrorLevels errorLevel, int count, DateTime? dateFrom, DateTime? dateTo, string fieldList)
        {
            if (dateFrom == null)
                dateFrom = DateTime.Now.Date.AddDays(-2);
            if (dateTo == null)
                dateTo = DateTime.Now.Date.AddDays(1);

            XElement doc = XElement.Load(LogFilename);
            var res = doc.Descendants("LogEntry");

            string elevel = errorLevel.ToString();

            if (errorLevel != ErrorLevels.All )
               res = res.Where(el => (string)el.Element("ErrorLevel") == elevel );

            res = res.Take(count)
                     .OrderByDescending(el => (DateTime) el.Element("Entered") );

            DataTable dt = CreateEntryDataTable();
            
            foreach (XElement node in res)
            {                   
                DataRow row = dt.NewRow();
                UpdateDataRowFromElement(node, row);
                                
                row["RequestDuration"] = (decimal)node.Element("RequestDuration");
                dt.Rows.Add(row);
            }
            DataTableReader reader = new DataTableReader(dt);
            

            return reader;
        }

        /// <summary>
        /// Not implemented yet
        /// </summary>
        /// <param name="errorLevel"></param>
        /// <param name="count"></param>
        /// <param name="dateFrom"></param>
        /// <param name="dateTo"></param>
        /// <param name="fieldList"></param>
        /// <returns></returns>
        public IEnumerable<WebLogEntry> GetEntryList(ErrorLevels errorLevel = ErrorLevels.All,
                                      int count = 200,
                                      DateTime? dateFrom = null,
                                      DateTime? dateTo = null,
                                      string fieldList = null)
        {
            var reader = GetEntries(errorLevel, count, dateFrom, dateTo, fieldList);
            if (reader == null || reader.FieldCount < 1)
            {
                yield break;
            }

            var piList = new Dictionary<string, PropertyInfo>();
            while (reader.Read())
            {
                var entry = new WebLogEntry();
                DataUtils.DataReaderToObject(reader, entry, null, piList);
                yield return entry;
            }
        }


        /// <summary>
        /// Does nothing for the XmlLogAdapter - log is created with first new entry instead
        /// </summary>
        /// <param name="logType"></param>
        /// <returns></returns>
        public bool CreateLog()
        {
            return true;
        }

        /// <summary>
        /// Deletes the XML log file
        /// </summary>
        /// <returns></returns>
        public bool DeleteLog()
        {
            try
            {
                File.Delete(LogFilename);            
                return true;
            }
            catch { }
            return false;
        }

        /// <summary>
        /// Clears out all items from the XML log - in effect deletes the log file.
        /// </summary>
        /// <returns></returns>
        public bool Clear()
        {
            return DeleteLog();
        }

        /// <summary>
        /// Not Implemented
        /// </summary>
        /// <param name="countToLeave"></param>
        /// <returns></returns>
        public bool Clear(int countToLeave)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Not implemented
        /// </summary>
        /// <param name="daysToDelete"></param>
        /// <returns></returns>
        public bool Clear(decimal daysToDelete)
        {
            throw new NotImplementedException();
        }

        public int GetEntryCount(ErrorLevels errorLevel = ErrorLevels.All)
        {
            throw new NotFiniteNumberException();
        }

        /// <summary>
        /// Creates a DataTable on the fly
        /// </summary>
        /// <returns></returns>
        private DataTable CreateEntryDataTable()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("Id", typeof(int));
            dt.Columns.Add("Entered", typeof(DateTime));
            dt.Columns.Add("Message", typeof(string));
            dt.Columns.Add("ErrorLevel", typeof(int));
            dt.Columns.Add("Details", typeof(string));
            dt.Columns.Add("Url", typeof(string));
            dt.Columns.Add("QueryString", typeof(string));
            dt.Columns.Add("UserAgent", typeof(string));
            dt.Columns.Add("Referrer", typeof(string));
            dt.Columns.Add("PostData", typeof(string));
            dt.Columns.Add("IpAddress", typeof(string));
            dt.Columns.Add("RequestDuration", typeof(decimal));

            return dt;
        }

        /// <summary>
        /// Updates the DataRow with data from node passed in
        /// </summary>
        /// <param name="node"></param>
        /// <param name="row"></param>
        private void UpdateDataRowFromElement(XElement node, DataRow row)
        {
            row["Id"] = (int)node.Element("Id");
            row["Entered"] = (DateTime)node.Element("Entered");
            row["Message"] = (string)node.Element("Message");
            row["ErrorLevel"] = (int)Enum.Parse(typeof(ErrorLevels), (string)node.Element("ErrorLevel"));

            row["Details"] = (string)node.Element("Details");
            row["Url"] = (string)node.Element("Url");
            row["QueryString"] = (string)node.Element("QueryString");
            row["UserAgent"] = (string)node.Element("UserAgent");
            row["Referrer"] = (string)node.Element("Referrer");
            row["PostData"] = (string)node.Element("PostData");
            row["IpAddress"] = (string)node.Element("IpAddress");
        }
    }
}
