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

using System.Configuration;
using System.ComponentModel;
using System.Xml.Serialization;
using Westwind.Utilities.Configuration;

namespace Westwind.Utilities.Logging
{
    /// <summary>
    /// Holds persisted Configuration Management settings that are 
    /// maintained in the .config file
    /// </summary>
    public class LogManagerConfiguration : AppConfiguration
    {

        /// <summary>
        /// Initialize the LogManagerConfiguration by reading
        /// from configuration section "LogManager"
        /// </summary>
        public LogManagerConfiguration()
        {
            
        }

        static LogManagerConfiguration()
        {
            Current = new LogManagerConfiguration();
            Current.Initialize();
        }


        protected override void OnInitialize(IConfigurationProvider provider, string sectionName, object configData)
        {
            if (provider == null)
            {
                provider = new ConfigurationFileConfigurationProvider<LogManagerConfiguration>()
                {
                    ConfigurationSection = "LogManager"
                };
            }
            
            Provider = provider;            
            Read();
        }

        /// <summary>
        /// Static singleton instance of the configuration object that
        /// is always accessible
        /// </summary>
        [XmlIgnore]        
        public static LogManagerConfiguration Current { get; set;  }



        /// <summary>
        /// The 'connection string' for the LogManager. Can be a ConnectionStrings entry or a full connection string
        /// </summary>
        [ConfigurationProperty("connectionString", DefaultValue = ""),
        Description("The 'connection string' for the LogManager. Can be a ConnectionStrings entry or a full connection string")]
        public string ConnectionString
        {
            get { return _ConnectionString; }
            set { _ConnectionString = value; }
        }
        private string _ConnectionString = "";


        /// <summary>
        /// The name of the file or table that holds log data.
        /// 
        /// For SQL this will be a tablename. For Xml this is a 
        /// virtual path for the xml log file (~/admin/ApplicationWebLog.xml)
        /// In non-web apps a full OS path should be used.
        /// </summary>
        [ConfigurationProperty("logFilename"),
        Description("The name of the file or table that holds log data.")]
        public string LogFilename
        {
            get { return _LogFilename; }
            set { _LogFilename = value; }
        }
        private string _LogFilename = "~/Admin/ApplicationWebLog.xml";


        /// <summary>
        /// Determines what type of log is logged to
        /// </summary>
        public LogAdapterTypes LogAdapter
        {
            get { return _LogAdapter; }
            set { _LogAdapter = value; }
        }
        private LogAdapterTypes _LogAdapter = LogAdapterTypes.Xml;


        /// <summary>
        /// Determines whether Web Requests are logged.
        /// 
        /// This property doesn't actually do anything in the log provider
        /// itself, but acts as a configuration setting placeholder that 
        /// can be used in an application to determine whether you should 
        /// log each request in a Web application
        /// </summary>
        public bool LogWebRequests
        {
            get { return _LogWebRequests; }
            set { _LogWebRequests = value; }
        }
        private bool _LogWebRequests = false;

        /// <summary>
        /// Determines whether Errors are logged.
        /// 
        /// This property doesn't actually do anything in the log provider
        /// itself, but acts as a configuration setting placeholder that 
        /// can be used in an application to determine whether you should 
        /// log Errors
        /// </summary>
        public bool LogErrors
        {
            get { return _LogErrors; }
            set { _LogErrors = value; }
        }
        private bool _LogErrors = true;
    }

    /// <summary>
    /// Determines what type of log output is created
    /// </summary>
    public enum LogAdapterTypes
    {
        Sql,
        Xml
    }
}
