#region License
/*
 **************************************************************
 *  Author: Rick Strahl 
 *          © West Wind Technologies, 2011
 *          http://www.west-wind.com/
 * 
 * Created: 6/19/2011
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
using Microsoft.Win32;

namespace Westwind.Utilities
{
    public static class  SystemUtils
    {
        /// <summary>
        /// Returns the logon password stored in the registry if Auto-Logon is used.
        /// This function is used privately for demos when I need to specify a login username and password.
        /// </summary>
        /// <param name="getUserName"></param>
        /// <returns></returns>
        public static string GetSystemPassword(bool getUserName)
        {
            var RegKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Winlogon");
            if (RegKey == null)
                return string.Empty;

            string Password;
            if (!getUserName)
                Password = (string)RegKey.GetValue("DefaultPassword");
            else
                Password = (string)RegKey.GetValue("DefaultUsername");

            if (Password == null)
                return string.Empty;

            return (string)Password;
        }  
    }
}
