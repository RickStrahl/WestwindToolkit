using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using System.IO;
using System.Web.Security;
using System.Web;

namespace Westwind.Web.Mvc
{
    /// <summary>
    /// User information container that can easily 'serialize'
    /// to a string and back. Meant to hold basic logon information
    /// </summary>
    [Serializable]
    public class UserState
    {
        private const string STR_Seperator = "|@";

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
            set { UserId = value.ToString(); }
        }

        

        /// <summary>
        /// Exports a short string list of Id, Email, Name separated by |
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {            
            //var serializer = new LosFormatter();
            //StringWriter writer = new StringWriter();
            //serializer.Serialize(writer, this);
            //return writer.ToString();
           
            return string.Join(STR_Seperator, new string[] { this.UserId, this.Name, this.IsAdmin.ToString(), this.Email });
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

            UserId = state.UserId;
            Email = state.Email;
            Name = state.Name;
            IsAdmin = state.IsAdmin;

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

            //var serializer = new LosFormatter();
            //return serializer.Deserialize(userData) as UserState;

            string[] strings = userData.Split(new string[1] {STR_Seperator}, StringSplitOptions.None );
            if (strings.Length < 4)
                return null;

            var userState = new UserState();

            userState.UserId = strings[0];
            userState.Name = strings[1];            
            userState.IsAdmin = strings[2] == "True";
            userState.Email = strings[3];
            
            return userState;
        }

        /// <summary>
        /// Creates a UserData object from authentication information in the 
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
