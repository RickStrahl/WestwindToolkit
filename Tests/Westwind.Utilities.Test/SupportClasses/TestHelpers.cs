using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace Westwind.Utilities.Configuration.Tests
{
    public class TestHelpers
    {
        public static string GetTestConfigFilePath()
        {
            return (typeof(TestHelpers).Assembly.Location + ".config");
                   
        }

        public static void DeleteTestConfigFile()
        {
            string configFile = GetTestConfigFilePath();
            try
            {
                File.Delete(configFile);
            }
            catch { }
        }

    }
}
