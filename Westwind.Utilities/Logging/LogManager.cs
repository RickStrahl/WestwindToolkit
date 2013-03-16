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
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Westwind.Utilities.Logging
{

    /// <summary>
    /// The LogManager provides the top level access to the log and is
    /// the public facing interface to the log. It uses an underlying 
    /// ILogProvider implementation to write to the appropriate log store.
    /// which includes Sql Server, Text File, Xml File or EventLog (and
    /// can be extended with your own providers)
    /// 
    /// To use you'll need to create an instance of the LogProvider once
    /// can call the static Create method. From then on in you can always
    /// use the LogManager.Current instance to access features
    /// of the logging engine.
    /// 
    /// To set up (Application_Start
    /// // Specify Sql Log with a Connection string or ConnectionString Config Entry Name
    /// LogManager.Create( new SqlLogAdapter("WestWindAdmin") );
    /// 
    /// To access log features:
    /// LogEntry entry = new LogEntry();
    /// entry.Message = "Application started...";
    /// entry.ErrorLevel = 
    /// </summary>
    public class LogManager
    {
        private static object SyncRoot = new object();

        /// <summary>
        /// Creates an instance of a log Manager and attaches it
        /// to the static Current property that is reusable throughout
        /// the application.
        /// 
        /// This method should be called once when the application starts
        /// </summary>
        /// <param name="adapter"></param>
        /// <returns></returns>
        public static LogManager Create(ILogAdapter adapter)
        {
            lock (SyncRoot)
            {
                if (Current == null)
                {
                    LogManager manager = new LogManager(adapter);
                    Current = manager;
                }

                return Current;
            }
        }

        /// <summary>
        /// Creates an instance of the LogManager based on the
        /// settings configured in the web.config file
        /// </summary>        
        /// <returns></returns>
        public static LogManager Create(LogAdapterTypes logType)
        {
            ILogAdapter adapter = null;
            
            if (logType == LogAdapterTypes.Sql)
                adapter = new SqlLogAdapter();
            else if (logType == LogAdapterTypes.Xml)
                adapter = new XmlLogAdapter();

            return Create(adapter);
        }

        /// <summary>
        /// Creates a new instance of the LogManager based on the
        /// settings in the .config file.
        /// </summary>
        /// <returns></returns>
        public static LogManager Create()
        {
            return Create(LogManagerConfiguration.Current.LogAdapter);
        }

        /// <summary>
        /// Static instance of the log manager. Used so you can configure
        /// the log manager once and easily reuse it in an application
        /// </summary>
        public static LogManager Current
        {
            get {
                if (_Current == null)                
                    _Current = LogManager.Create();
                
                return _Current; 
            }
            set { _Current = value; }
        }
        private static LogManager _Current;

   
        /// <summary>
        /// Global instance of the LogAdapter used
        /// </summary>
        public ILogAdapter LogAdapter
        {
            get { return _LogAdapter; }
            set { _LogAdapter = value; }
        }
        private ILogAdapter _LogAdapter = null;


        /// <summary>
        /// Don't allow no-parm 
        /// </summary>
        private LogManager() { }

        /// <summary>
        /// Main signature allows creating of manager with an adapter
        /// to specify logging target
        /// 
        /// Available Adapters are:
        /// SqlLogAdapter
        /// XmlLogAdapter (n/a)
        /// TextLogAdapter (n/a)
        /// EventLogAdapter (n/a)
        /// </summary>
        /// <param name="logAdapter"></param>
        public LogManager(ILogAdapter logAdapter)
        {
            LogAdapter = logAdapter;
        }


        /// <summary>
        /// Writes a Web specific log entry into the log
        /// </summary>
        /// <param name="entry"></param>
        /// <returns></returns>
        public bool WriteEntry(WebLogEntry entry)
        {
            return LogAdapter.WriteEntry(entry);
        }




        /// <summary>
        /// Retrieves an individual log entry if possible. Depending
        /// on the implementation of the log log entries may not be
        /// retrievable individually (for example from a text log) or
        /// the event log.
        /// </summary>
        /// <param name="id"></param>
        /// <returns>Web Log Entry or null</returns>
        /// <exception cref="System.InvalidOperationException">Fired if id is not found or data can't be accessed</exception>
        public WebLogEntry GetWebLogEntry(int id)
        {
            return LogAdapter.GetEntry(id);
        }

        /// <summary>
        /// Creates a new Log table/file/log depending on the provider.
        /// Check provider documentation on requirements for 'connections'
        /// or locations for logs.
        /// </summary>
        /// <param name="logType"></param>
        /// <returns></returns>
        public bool CreateLog()
        {
            return LogAdapter.CreateLog();
        }

        /// <summary>
        /// Deletes the Log completely by removing the table/file/log
        /// </summary>
        /// <param name="logType"></param>
        /// <returns></returns>
        public bool DeleteLog()
        {
            return LogAdapter.DeleteLog();
        }

        /// <summary>
        /// Clears out all the entries in the log
        /// </summary>
        /// <returns></returns>
        public bool Clear()
        {
            return LogAdapter.Clear();
        }
        /// <summary>
        /// Clears the log but leaves specified number of the last entries        
        /// </summary>
        /// <param name="countToLeave"></param>
        /// <returns></returns>
        public bool Clear(int countToLeave)
        {
            return LogAdapter.Clear(countToLeave);
        }
    }
}
