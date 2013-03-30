using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Web.UI;
using System.IO;

namespace Westwind.Utilities.Test
{
    [TestClass]
    public class StringSerializerTests
    {
        private const int INT_ProfileLoop = 1000;

        [TestMethod]
        public void StringSerializerTest()
        {
            var state = new UserState();
            state.Email = "rstrahl@west-wind.com";
            state.UserId = "1";
            state.IsAdmin = true;
            state.Name = "Rick Strahl | Markus Egger";
            state.Date = DateTime.Now;
            state.Role = new Role() { Level = 10, Name = "Rick" };            
            
            string ser = null;
            ser = StringSerializer.SerializeObject(state);

            Stopwatch watch = new Stopwatch();
            watch.Start();
            
            for (int i = 0; i < INT_ProfileLoop; i++)
            {
                ser = StringSerializer.SerializeObject(state);
            }

            watch.Stop();

            Console.WriteLine("StringSerializer: " + ser.Length + "  elapsed: " + watch.ElapsedMilliseconds + "ms");
            Console.WriteLine(ser);

            var state2 = StringSerializer.Deserialize<UserState>(ser);

            Assert.AreEqual(state.Email, state2.Email);
            Assert.AreEqual(state.UserId, state2.UserId);
            Assert.AreEqual(state.Name, state2.Name);

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

        [TestMethod]
        public void XmlSerializerSizeTest()
        {
            var state = new UserState();
            state.Email = "rstrahl@west-wind.com";
            state.UserId = "1";
            state.IsAdmin = true;
            state.Name = "Rick Strahl | Markus Egger";
            state.Date = DateTime.Now;
            state.Role = null;

            
            string xml = null; 
            var bytes = SerializationUtils.SerializeObjectToByteArray(state, true);

            Stopwatch watch = new Stopwatch();
            watch.Start();


            for (int i = 0; i < INT_ProfileLoop; i++)
            {
                bytes = SerializationUtils.SerializeObjectToByteArray(state, true);
            }

            
            watch.Stop();

            xml = SerializationUtils.SerializeObjectToString(state, true);
            Console.WriteLine("Xml: " + xml.Length + "  elapsed: " + watch.ElapsedMilliseconds + "ms");
            Console.WriteLine(xml);
        }

        [TestMethod]
        public void JsonSerializerSizeTest()
        {
            var state = new UserState();
            state.Email = "rstrahl@west-wind.com";
            state.UserId = "1";
            state.IsAdmin = true;
            state.Name = "Rick Strahl | Markus Egger";
            state.Date = DateTime.Now;
            state.Role = null;


            string json = null;
            json = JsonConvert.SerializeObject(state);

            Stopwatch watch = new Stopwatch();
            watch.Start();

            for (int i = 0; i < INT_ProfileLoop; i++)
            {
                json = JsonConvert.SerializeObject(state);
            }

            watch.Stop();

            Console.WriteLine("Json: " + json.Length  + "  time: " + watch.ElapsedMilliseconds + "ms");
            Console.WriteLine(json);
        }
        [TestMethod]        
        public void LosSerializerSizeTest()
        {
            var state = new UserState();
            state.Email = "rstrahl@west-wind.com";
            state.UserId = "1";
            state.IsAdmin = true;
            state.Name = "Rick Strahl | Markus Egger";
            state.Date = DateTime.Now;
            state.Role = null;


            var los = new LosFormatter();
            var writer = new StringWriter();

            los.Serialize(writer, state);
            string json = writer.ToString();


            Stopwatch watch = new Stopwatch();
            watch.Start();

            for (int i = 0; i < INT_ProfileLoop; i++)
            {
                writer = new StringWriter();
                los.Serialize(writer, state);
                json = writer.ToString();
            }

            watch.Stop();

            Console.WriteLine("LosFormatter: " + json.Length + "  time: " + watch.ElapsedMilliseconds + "ms");
            Console.WriteLine(json);
        }


        [Serializable]
        public class UserState
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

        [Serializable]
        public class Role
        {
            public string Name { get; set; }
            public int Level { get; set; }
        }

    }
}
