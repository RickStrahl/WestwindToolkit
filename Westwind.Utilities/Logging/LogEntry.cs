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
using System.Text;

namespace Westwind.Utilities.Logging
{
    /// <summary>
    /// Message object that contains information about the current error information
    /// 
    /// Note: a WebLogEntry specific to Web applications lives in the
    /// Westwind.Web assembly.
    /// </summary>
    public class LogEntry
    {

        public LogEntry()
        {
        }

        public LogEntry(Exception ex)
        {
            UpdateFromException(ex);
        }

        /// <summary>
        /// The unique ID for this LogEntry
        /// </summary>
        public int Id
        {
          get { return _Id; }
          set { _Id = value; }
        }
        private int _Id = 0;

        /// <summary>
        /// When this error occurred.
        /// </summary>
        public DateTime Entered
        {
            get { return _Entered; }
            set { _Entered = value; }
        }
        private DateTime _Entered = DateTime.UtcNow;

        /// <summary>
        /// The Actual Error Message
        /// </summary>
        public string Message
        {
            get { return _Message; }
            set { _Message = value; }
        }
        private string _Message = "";


        /// <summary>
        /// Determines the error level of the messages
        /// </summary>
        public ErrorLevels ErrorLevel
        {
            get { return _ErrorLevel; }
            set { _ErrorLevel = value; }
        }
        private ErrorLevels _ErrorLevel = ErrorLevels.Error;


        /// <summary>
        /// Free form text field that contains extra data to display
        /// </summary>
        public string Details
        {
            get { return _Details; }
            set { _Details = value; }
        }
        private string _Details = "";

        /// <summary>
        /// The type of exception that was thrown if an error occurred
        /// </summary>
        public string ErrorType
        {
            get { return _ErrorType; }
            set { _ErrorType = value; }
        }
        private string _ErrorType = "";

        /// <summary>
        /// StackTrace in event of an exception
        /// </summary>
        public string StackTrace
        {
            get { return _StackTrace; }
            set { _StackTrace = value; }
        }
        private string _StackTrace = "";


        /// <summary>
        /// Updates the current request as an error log entry and 
        /// sets the ErrorType, Message and StackTrace properties
        /// from the content of the passed exception
        /// </summary>
        /// <param name="ex"></param>
        public void UpdateFromException(Exception ex)
        {
            ErrorLevel = ErrorLevels.Error;
            ErrorType = ex.GetType().Name.Replace("Exception", "");
            Message = ex.Message;            
            StackTrace = ex.StackTrace != null && ex.StackTrace.Length > 1490 ?  
                              ex.StackTrace.Substring(0,1500) : 
                              ex.StackTrace;
        }
    }


    [Flags]
    public enum ErrorLevels
    {
        /// <summary>
        /// A critical error occurred
        /// </summary>          
        Error = 2,
        /// <summary>
        /// A warning type message that drives attention to potential problems
        /// </summary>
        Warning = 1,
        /// <summary>
        /// An informational message such as start and stop notices
        /// </summary>
        Message = 4,
        /// <summary>
        /// Log Entry
        /// </summary>
        Log = 8,
        /// <summary>
        /// Unclassified message type
        /// </summary>
        Miscellaneous = 16,
        /// <summary>
        /// Empty/none 
        /// </summary>
        None = 0,
        /// <summary>
        /// All ErrorLevels - used only for querying
        /// </summary>            
        All = 256
    }
}
