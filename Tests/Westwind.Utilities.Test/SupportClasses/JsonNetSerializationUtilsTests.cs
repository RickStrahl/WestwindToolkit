using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Web.Script.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Westwind.Utilities.Test;

namespace Westwind.Utilities.Configuration.Tests
{
    [TestClass]
    public class JsonNetSerializationUtilsTests
    {
        [TestMethod]
        public void JsonStringSerializeTest()
        {
            var config = new AutoConfigFileConfiguration();

            string json = JsonSerializationUtils.Serialize(config, true, true);

            Console.WriteLine(json);
            Assert.IsNotNull(json);
        }

        [TestMethod]
        public void JsonSerializeToFile()
        {
            var config = new AutoConfigFileConfiguration();

            bool result = JsonSerializationUtils.SerializeToFile(config, "serialized.config", true, true);
            string filetext = File.ReadAllText("serialized.config");
            Console.WriteLine(filetext);
        }


        [TestMethod]
        public void JsonDeserializeStringTest()
        {
            var config = new AutoConfigFileConfiguration();
            config.ApplicationName = "New App";
            config.DebugMode = DebugModes.DeveloperErrorMessage;
            string json = JsonSerializationUtils.Serialize(config, true, true);

            config = null;

            config = JsonSerializationUtils.Deserialize(json, typeof(AutoConfigFileConfiguration), true) as AutoConfigFileConfiguration;

            Assert.IsNotNull(config);
            Assert.IsTrue(config.ApplicationName == "New App");
            Assert.IsTrue(config.DebugMode == DebugModes.DeveloperErrorMessage);

        }

        [TestMethod]
        public void DeserializeFromFileTest()
        {
            string fname = "serialized.config";

            var config = new AutoConfigFileConfiguration();
            config.ApplicationName = "New App";
            config.DebugMode = DebugModes.DeveloperErrorMessage;
            bool result = JsonSerializationUtils.SerializeToFile(config, fname, true, true);

            Assert.IsTrue(result);

            config = null;

            config = JsonSerializationUtils.DeserializeFromFile(fname, typeof(AutoConfigFileConfiguration)) as
                AutoConfigFileConfiguration;

            Assert.IsNotNull(config);
            Assert.IsTrue(config.ApplicationName == "New App");
            Assert.IsTrue(config.DebugMode == DebugModes.DeveloperErrorMessage);
        }

        [TestMethod]
        public void JsonNativeInstantiation()
        {
            // Try to create instance
            var ser = new JsonSerializer();

            //ser.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;
            //ser.ObjectCreationHandling = ObjectCreationHandling.Auto;
            //ser.MissingMemberHandling = MissingMemberHandling.Ignore;
            ser.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            ser.Converters.Add(new StringEnumConverter());

            var config = new AutoConfigFileConfiguration();
            config.ApplicationName = "New App";
            config.DebugMode = DebugModes.DeveloperErrorMessage;

            var writer = new StringWriter();
            var jtw = new JsonTextWriter(writer);
            jtw.Formatting = Formatting.Indented;

            ser.Serialize(jtw, config);

            string result = writer.ToString();
            jtw.Close();

            Console.WriteLine(result);

            dynamic json = ReflectionUtils.CreateInstanceFromString("Newtonsoft.Json.JsonSerializer");
            dynamic enumConverter = ReflectionUtils.CreateInstanceFromString("Newtonsoft.Json.Converters.StringEnumConverter");
            json.Converters.Add(enumConverter);

            writer = new StringWriter();
            jtw = new JsonTextWriter(writer);
            jtw.Formatting = Formatting.Indented;

            json.Serialize(jtw, config);

            result = writer.ToString();
            jtw.Close();

            Console.WriteLine(result);
        }

        [TestMethod]
        public void NativePerfTest()
        {
            // Try to create instance
            var ser = new JsonSerializer();
            ser.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            ser.Converters.Add(new StringEnumConverter());

            var config = new AutoConfigFileConfiguration();
            config.ApplicationName = "New App";
            config.DebugMode = DebugModes.DeveloperErrorMessage;

            string result = null;

            var sw = new Stopwatch();
            sw.Start();

            for (int i = 0; i < 10000; i++)
            {
                var writer = new StringWriter();
                var jtw = new JsonTextWriter(writer);
                jtw.Formatting = Formatting.Indented;
                ser.Serialize(jtw, config);
                result = writer.ToString();
                jtw.Close();
            }

            sw.Stop();
            Console.WriteLine("Native Serialize: " + sw.ElapsedMilliseconds + "ms");
            Console.WriteLine(result);
        }
        [TestMethod]
        public void Native2PerfTest()
        {
            var config = new AutoConfigFileConfiguration();
            config.ApplicationName = "New App";
            config.DebugMode = DebugModes.DeveloperErrorMessage;

            string result = JsonSerializationUtils.Serialize(config, true, true);

            var sw = new Stopwatch();
            sw.Start();

            for (int i = 0; i < 10000; i++)
            {
                result = JsonSerializationUtils.Serialize(config, true, true);
            }

            sw.Stop();
            Console.WriteLine("Utils Serialize: " + sw.ElapsedMilliseconds + "ms");
            Console.WriteLine(result);
        }

        [TestMethod]
        public void UtilsPerfTest()
        {
            var config = new AutoConfigFileConfiguration();
            config.ApplicationName = "New App";
            config.DebugMode = DebugModes.DeveloperErrorMessage;

            string result = JsonSerializationUtils.Serialize(config, true, true);
            result = JsonSerializationUtils.Serialize(config, true, true);

            var sw = new Stopwatch();
            sw.Start();

            for (int i = 0; i < 10000; i++)
            {
                result = JsonSerializationUtils.Serialize(config, true, true);
            }

            sw.Stop();
            Console.WriteLine("Utils Serialize: " + sw.ElapsedMilliseconds + "ms");
            Console.WriteLine(result);
        }

        [TestMethod]
        public void PrettifyJsonStringTest()
        {
            var test = new
            {
                name = "rick",
                company = "Westwind",
                entered = DateTime.UtcNow
            };

            string json = JsonConvert.SerializeObject(test);
            Console.WriteLine(json);

            
            string jsonFormatted = JsonSerializationUtils.FormatJsonString(json);
            //string jsonFormatted = JValue.Parse(json).ToString(Formatting.Indented);

            //JValue.Parse()
            Console.WriteLine(jsonFormatted);
        }
    }
}
