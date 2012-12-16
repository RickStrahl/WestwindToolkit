using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Westwind.Utilities.Configuration.Tests
{
    /// <summary>
    /// Custom Configuration Provider implementation that allows
    /// uses a different section and encrypts a couple of properties
    /// </summary>
    public class XmlFileConfiguration : Westwind.Utilities.Configuration.AppConfiguration
    {

        // Must implement public default constructor
        public XmlFileConfiguration()
        {
            // Default values assigned
            Initialize();
        }

        // Always call this constructor new CustomConfigFileConfiguration(null)
        public XmlFileConfiguration(string xmlFile)
        {
            // Default values assigned
            Initialize();

            if (string.IsNullOrEmpty(xmlFile))
                xmlFile = "XmlConfiguration.xml"; // in web apps use full path

            var provider = new XmlFileConfigurationProvider<XmlFileConfiguration>()
            {
                XmlConfigurationFile=xmlFile,
                EncryptionKey = "ultra-seekrit",  // use a generated value here
                PropertiesToEncrypt = "Password,AppConnectionString"
                // UseBinarySerialization = true                     
            };                
                       
            // assign the provider
            Provider = provider;
            Read();        
        }

        public string ApplicationName { get; set; }
        public DebugModes DebugMode { get; set; }
        public int MaxDisplayListItems { get; set; }
        public bool SendAdminEmailConfirmations { get; set; }
        public string Password { get; set; }
        public string AppConnectionString { get; set; }

        protected override void Initialize()
        {
            ApplicationName = "Configuration Tests";
            DebugMode = DebugModes.Default;
            MaxDisplayListItems = 15;
            SendAdminEmailConfirmations = false;
            Password = "seekrit";
            AppConnectionString = "server=.;database=hosers;uid=bozo;pwd=seekrit;";
        }
    }

}
