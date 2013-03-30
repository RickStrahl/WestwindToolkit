using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Westwind.Utilities.Test
{
    [TestClass]
    public class StringSerializerTests
    {

        [TestMethod]
        public void StringSerializerTest()
        {
            var state = new UserState();
            state.Email = "rstrahl@west-wind.com";
            state.UserId = "1";
            state.IsAdmin = true;
            state.Name = "Rick";
            state.Date = DateTime.Now;

            state.Role =  new Role()
            {
                Level = 10,
                Name = "Administrator"
            };

            string ser = StringSerializer.SerializeObject(state);

            Console.WriteLine(ser.Length);
            Console.WriteLine(ser);

            var state2 = StringSerializer.Deserialize<UserState>(ser);

            Assert.AreEqual(state.Email, state2.Email);
            Assert.AreEqual(state.UserId, state2.UserId);

            // exact date is not lined up to ticks so compare minutes
            Assert.AreEqual(state.Date.Minute, state2.Date.Minute);

            // Computed property
            Assert.AreEqual(state.UserIdInt, state2.UserIdInt);
            
            // Role is an unsupported type so it should come back as null
            Assert.IsNull(state2.Role);
        }

        [TestMethod]
        public void StringSerializerNullTest()
        {
            UserState state = null;

            string ser = StringSerializer.SerializeObject(state);

            Console.WriteLine(ser.Length);
            Console.WriteLine(ser);

            var state2 = StringSerializer.Deserialize<UserState>(ser);

            Assert.IsNull(state2);
        }


        internal class UserState
        {

            /// <summary>
            /// The display name for the userId
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// The user's email address or login acount
            /// </summary>
            public string Email { get; set; }

            /// <summary>
            /// The user's user Id as a string
            /// </summary>
            public string UserId { get; set; }

            /// <summary>
            /// The users admin status
            /// </summary>
            public bool IsAdmin { get; set; }

            /// <summary>
            /// 
            /// </summary>
            public DateTime Date { get; set; }

            /// <summary>
            /// Returns the User Id as an int if convertiable
            /// </summary>
            public int UserIdInt
            {
                get
                {
                    if (string.IsNullOrEmpty(UserId))
                        return 0;
                    return int.Parse(UserId);
                }
                set
                {
                    UserId = value.ToString();
                }
            }

            public Role Role { get; set; }




        }

        internal class Role
        {
            public string Name { get; set; }
            public int Level { get; set; }
        }

    }
}
