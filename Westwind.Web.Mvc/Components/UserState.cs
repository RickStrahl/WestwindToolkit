using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using System.IO;
using System.Web.Security;
using System.Web;
using Westwind.Utilities;

namespace Westwind.Web.Mvc
{
    /// <summary>
    /// User information container that can easily 'serialize'
    /// to a string and back. Meant to hold basic logon information.
    /// 
    /// I use this class a lot to attach as Forms Authentication
    /// Ticket data to keep basic user data without having to
    /// hit the database
    /// </summary>
    [Serializable]
    public class UserState
    {        

        public UserState()
        {
            Name = string.Empty;
            Email = string.Empty;
            UserId = string.Empty;
            IsAdmin = false;
        }

        /// <summary>
        /// The display name for the userId
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The user's email address or login acount
        /// </summary>
        public string Email {get; set; }

        /// <summary>
        /// The user's user Id as a string
        /// </summary>
        public string UserId {get; set; }

        /// <summary>
        /// The users admin status
        /// </summary>
        public bool IsAdmin {get; set; }

        /// <summary>
        /// Returns the User Id as an int if convertiable
        /// </summary>
        public int UserIdInt
        {
            get {
                if (string.IsNullOrEmpty(UserId))
                    return 0;
                return int.Parse(UserId);  
            }
            set 
            { 
                UserId = value.ToString(); 
            }
        }

        

/// <summary>
/// Exports a short string list of Id, Email, Name separated by |
/// </summary>
/// <returns></returns>
public override string ToString()
{
    return StringSerializer.SerializeObject(this);
}

/// <summary>
/// Imports Id, Email and Name from a | separated string
/// </summary>
/// <param name="itemString"></param>
public bool FromString(string itemString)
{
    if (string.IsNullOrEmpty(itemString))
        return false;

    var state = CreateFromString(itemString);
    if (state == null)
        return false;

    // copy the properties
    DataUtils.CopyObjectData(state, this);

    return true;
}


/// <summary>
/// Creates an instance of a userstate object from serialized
/// data.
/// 
/// IsEmpty() will return true if data was not loaded. A 
/// UserData object is always returned.
/// </summary>
/// <param name="userData"></param>
/// <returns></returns>
public static UserState CreateFromString(string userData)
{
    if (string.IsNullOrEmpty(userData))
        return null;

    return StringSerializer.Deserialize<UserState>(userData);
}


        
/// <summary>
/// Creates a UserState object from authentication information in the 
/// Forms Authentication ticket.
/// 
/// IsEmpty() will return false if no data was loaded but
/// a Userdata object is always returned
/// </summary>
/// <returns></returns>
public static UserState CreateFromFormsAuthTicket()
{
    return CreateFromString(((FormsIdentity)HttpContext.Current.User.Identity).Ticket.UserData);
}


        /// <summary>
        /// Determines whether UserState instance
        /// holds user information.
        /// </summary>
        /// <returns></returns>
        public bool IsEmpty()
        {
            return string.IsNullOrEmpty(this.UserId);
        }
    }
}
