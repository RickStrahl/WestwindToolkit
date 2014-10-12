using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Westwind.Utilities.Configuration.Tests
{
    /// <summary>
    /// Custom Configuration Provider implementation that allows
    /// uses a different section and encrypts a couple of properties
    /// </summary>
    public class XmlFileConfiguration : Westwind.Utilities.Configuration.AppConfiguration
    {
        public string ApplicationName { get; set; }
        public DebugModes DebugMode { get; set; }
        public int MaxDisplayListItems { get; set; }
        public bool SendAdminEmailConfirmations { get; set; }
        public string Password { get; set; }
        public string AppConnectionString { get; set; }
        public LicenseInformation License {get; set; }


        // Must implement public default constructor
        public XmlFileConfiguration()
        {
            ApplicationName = "Configuration Tests";
            DebugMode = DebugModes.Default;
            MaxDisplayListItems = 15;
            SendAdminEmailConfirmations = false;
            Password = "seekrit";
            License = new LicenseInformation()
            {
                Company = "West Wind",
                Name = "Rick",
                LicenseKey = "westwindrick-4123122"
            };
            AppConnectionString = "server=.;database=hosers;uid=bozo;pwd=seekrit;";
        }


        // Automatically initialize with default config and config file
        public void Initialize(string configFile)
        {
            base.Initialize(configData: configFile);
        }

        protected override IConfigurationProvider OnCreateDefaultProvider(string sectionName, object configData)
        {
            string xmlFile = "XmlConfiguration.xml";
            if (configData != null)
                xmlFile = xmlFile;

            var provider = new XmlFileConfigurationProvider<XmlFileConfiguration>()
            {
                XmlConfigurationFile = xmlFile,
                EncryptionKey = "ultra-seekrit",  // use a generated value here
                PropertiesToEncrypt = "Password,AppConnectionString,License.LicenseKey"
                // UseBinarySerialization = true                     
            };

            return provider;
        }
    }
}
