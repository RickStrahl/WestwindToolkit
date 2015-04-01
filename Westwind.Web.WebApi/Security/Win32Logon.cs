using System;
using System.Runtime.InteropServices;

namespace Westwind.Web.WebApi.Security
{
    internal sealed class Win32Logon
    {

        /// <summary>
        /// Logins the specified user name.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <param name="password">The password.</param>
        /// <returns></returns>
        //[PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        public static LoginStatus Login(string userName, string password)
        {
            var ret = LoginStatus.Success;
            var token = IntPtr.Zero;

            try
            {
                /* Batch is best for situations like this. But permission must be given in AD first*/
                var validLogin = LogonUser(userName, null, password, LogonTypes.Network, LogonProviders.Default, out token);

                if (!validLogin)
                {
                    ret = LoginStatus.Failed;
                    var lastError = Marshal.GetLastWin32Error();

                    switch (lastError)
                    {
                        case (int)LogonUserErrors.ERROR_PASSWORD_EXPIRED:
                        case (int)LogonUserErrors.ERROR_PASSWORD_MUST_CHANGE:
                            ret = LoginStatus.PasswordExpired;
                            break;
                        case (int)LogonUserErrors.ERROR_ACCOUNT_DISABLED:
                        case (int)LogonUserErrors.ERROR_ACCOUNT_EXPIRED:
                            ret = LoginStatus.Disabled;
                            break;
                        case (int)LogonUserErrors.ERROR_ACCOUNT_LOCKED_OUT:
                            ret = LoginStatus.LockedOut;
                            break;
                    }
                }
            }
            catch
            {
            }
            finally
            {
                if (token != IntPtr.Zero)
                {
                    CloseHandle(token);
                }
            }

            return ret;
        }

        /// <summary>
        /// Logons the user.
        /// </summary>
        /// <param name="principal">The principal.</param>
        /// <param name="authority">The authority.</param>
        /// <param name="password">The password.</param>
        /// <param name="logonType">Type of the logon.</param>
        /// <param name="logonProvider">The logon provider.</param>
        /// <param name="token">The token.</param>
        /// <returns></returns>
        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool LogonUser(
            string principal,
            string authority,
            string password,
            LogonTypes logonType,
            LogonProviders logonProvider,
            out IntPtr token);

        /// <summary>
        /// Closes the handle.
        /// </summary>
        /// <param name="handle">The handle.</param>
        /// <returns></returns>
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool CloseHandle(IntPtr handle);

        /// <summary>
        /// Possible Error Codes for Logon User
        /// </summary>
        enum LogonUserErrors : int
        {
            ERROR_PASSWORD_MUST_CHANGE = 1907,
            ERROR_LOGON_FAILURE = 1326,
            ERROR_ACCOUNT_RESTRICTION = 1327,
            ERROR_ACCOUNT_DISABLED = 1331,
            ERROR_INVALID_LOGON_HOURS = 1328,
            ERROR_NO_LOGON_SERVERS = 1311,
            ERROR_INVALID_WORKSTATION = 1329,
            ERROR_ACCOUNT_LOCKED_OUT = 1909,      //It gives this error if the account is locked, REGARDLESS OF WHETHER VALID CREDENTIALS WERE PROVIDED!!!
            ERROR_ACCOUNT_EXPIRED = 1793,
            ERROR_PASSWORD_EXPIRED = 1330
        }

        /// <summary>
        /// Logon Types for Logon User
        /// </summary>
        enum LogonTypes : uint
        {
            Interactive = 2,
            Network = 3,
            Batch = 4,
            Service = 5,
            Unlock = 7,
            NetworkCleartext = 8,
            NewCredentials = 9
        }

        /// <summary>
        /// Logon Providers for Logon User
        /// </summary>
        enum LogonProviders : uint
        {
            Default = 0, // default for platform (use this!)
            WinNT35,     // sends smoke signals to authority
            WinNT40,     // uses NTLM
            WinNT50      // negotiates Kerb or NTLM
        }
    }
}
