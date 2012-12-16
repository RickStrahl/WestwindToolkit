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

using System.Runtime.InteropServices;
using System.Security.Principal;
using System;

namespace Westwind.Utilities
{

    /// <summary>
    /// A set of utilities functions related to security.
    /// </summary>
    public static class SecurityUtils
    {
        const int LOGON32_LOGON_INTERACTIVE = 2;
        const int LOGON32_LOGON_NETWORK = 3;
        const int LOGON32_LOGON_BATCH = 4;
        const int LOGON32_LOGON_SERVICE = 5;
        const int LOGON32_LOGON_UNLOCK = 7;
        const int LOGON32_LOGON_NETWORK_CLEARTEXT = 8;
        const int LOGON32_LOGON_NEW_CREDENTIALS = 9;
        const int LOGON32_PROVIDER_DEFAULT = 0;

        [DllImport("advapi32.dll", SetLastError = true)]
        static extern int LogonUser(
            string lpszUsername,
            string lpszDomain,
            string lpszPassword,
            int dwLogonType,
            int dwLogonProvider,
            out IntPtr phToken
            );

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern int CloseHandle(IntPtr hObject);

        /// <summary>
        /// Logs on a user and changes the current process impersonation to that user.
        /// 
        /// IMPORTANT: Returns a WindowsImpersonationContext and you have to either
        /// dispose this instance or call RevertImpersonation with it.
        /// </summary>
        /// <remarks>
        /// Requires Full Trust permissions in ASP.NET Web Applications.
        /// </remarks>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <param name="domain"></param>
        /// <returns>WindowsImpersonation Context. Call RevertImpersonation() to clear the impersonation or Dispose() instance.</returns>
        public static WindowsImpersonationContext ImpersonateUser(string username, string password, string domain)
        {
            IntPtr token = IntPtr.Zero;
            try
            {
                int TResult = LogonUser(username, domain, password,
                                        LOGON32_LOGON_NETWORK, LOGON32_PROVIDER_DEFAULT,
                                        out token);

                WindowsImpersonationContext context = null;
                context = WindowsIdentity.Impersonate(token);
                CloseHandle(token);

                return context;
            }
            catch
            {
                return null;
            }
            finally
            {
                if (token != IntPtr.Zero)
                    CloseHandle(token);
            }
        }

        /// <summary>
        /// Releases an impersonation context and releases associated resources
        /// </summary>
        /// <param name="context">WindowsImpersonation context created with ImpersonateUser</param>
        public static void RevertImpersonation(WindowsImpersonationContext context)
        {
            context.Undo();
            context.Dispose();
        }
    }
}