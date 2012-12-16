#region License
/*
 **************************************************************
 *  Author: Rick Strahl 
 *          © West Wind Technologies, 2011
 *          http://www.west-wind.com/
 * 
 * Created: 06/12/2011
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


namespace Westwind.Utilities
{
	/// <summary>
	/// DebugUtils class contains various utility methods
    /// for debugging and diagnostic tasks
	/// </summary>
	public  static class DebugUtils
	{		
        /// <summary>
        /// Returns the innermost Exception for an object
        /// </summary>
        /// <param name="ex"></param>
        /// <returns></returns>
        public static Exception GetInnerMostException(Exception ex)
        {
            Exception currentEx = ex;
            while (currentEx.InnerException != null)
            {
                currentEx = currentEx.InnerException;
            }

            return currentEx;
        }

        /// <summary>
        /// Returns an array of the entire exception list in reverse order
        /// (innermost to outermost exception)
        /// </summary>
        /// <param name="ex">The original exception to work off</param>
        /// <returns>Array of Exceptions from innermost to outermost</returns>
        public static Exception[] GetInnerExceptions(Exception ex)
        {
            List<Exception> exceptions = new List<Exception>();
            exceptions.Add(ex);
            
            Exception currentEx = ex;
            while (currentEx.InnerException != null)
            {
                currentEx = currentEx.InnerException;
                exceptions.Add(currentEx);
            }

            // Reverse the order to the innermost is first
            exceptions.Reverse();

            return exceptions.ToArray();
        }

	}

}
