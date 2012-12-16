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
using System.Collections.Generic;

namespace Westwind.Utilities.Logging
{
    public interface ILogAdapter
    {
        /// <summary>
        /// ConnectionString or other string that identifies the output medium
        /// </summary>
        string ConnectionString { get; set; }

        /// <summary>
        /// The name of the log file if a file is involved
        /// </summary>
        string LogFilename { get; set; }        
        
        /// <summary>
        /// Writes an entry into the log
        /// </summary>
        /// <param name="webEntry"></param>
        /// <returns></returns>
        bool WriteEntry(WebLogEntry webEntry);

        /// <summary>
        /// Retireves an individual entry from the log
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        WebLogEntry GetEntry(int id);


        /// <summary>
        /// Retrieves a filtered list of entries
        /// </summary>
        /// <param name="errorLevel"></param>
        /// <param name="count"></param>
        /// <param name="dateFrom"></param>
        /// <param name="dateTo"></param>
        /// <param name="FieldList"></param>
        /// <returns></returns>
        IDataReader GetEntries(ErrorLevels errorLevel, int count, DateTime? dateFrom, DateTime? dateTo, string FieldList);

        /// <summary>
        /// Returns a filtered list of entries as a strongly typed list
        /// </summary>
        /// <param name="errorLevel"></param>
        /// <param name="count"></param>
        /// <param name="dateFrom"></param>
        /// <param name="dateTo"></param>
        /// <param name="FieldList"></param>
        /// <returns></returns>
        IEnumerable<WebLogEntry> GetEntryList(ErrorLevels errorLevel, int count, DateTime? dateFrom, DateTime? dateTo, string FieldList);

        /// <summary>
        /// Creates the a standard Log store if it doesn't exist
        /// </summary>
        /// <returns></returns>
        bool CreateLog();

        /// <summary>
        /// Deletes the log file completely
        /// </summary>
        /// <returns></returns>
        bool DeleteLog();


        /// <summary>
        /// Clears the Log store completely
        /// </summary>
        /// <returns></returns>
        bool Clear();

        /// <summary>
        /// Clears the log but leaves the last entries intact
        /// </summary>
        /// <param name="countToLeave"></param>
        /// <returns></returns>
        bool Clear(int countToLeave);
    }

    public enum LogTypes
    {
        ApplicationLog,
        ApplicationWebLog,       
        None
    }
}
