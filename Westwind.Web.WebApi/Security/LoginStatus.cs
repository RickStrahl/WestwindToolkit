using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Westwind.Web.WebApi.Security
{
    /// <summary>
    /// Login Status
    /// </summary>
    public enum LoginStatus
    {
        Success,
        Failed,
        Disabled,
        PasswordExpired,
        LockedOut,
        DoesNotBelongToSecurityGroup
    }
}
