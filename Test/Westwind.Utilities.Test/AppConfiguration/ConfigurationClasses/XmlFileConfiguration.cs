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

        // Must implement public default constructor
        public XmlFileConfiguration()
        {
            ApplicationName = "Configuration Tests";
            DebugMode = DebugModes.Default;
            MaxDisplayListItems = 15;
            SendAdminEmailConfirmations = false;
            Password = "seekrit";
            AppConnectionString = "server=.;database=hosers;uid=bozo;pwd=seekrit;";
        }

        // Automatically initialize with default config and config file
        public void Initialize(string configFile)
        {
            base.Initialize(configData: configFile);
        }

        protected override void OnInitialize(IConfigurationProvider provider = null, 
                                             string sectionName = null,
                                             object configData = null)
        {
            if (provider == null)
            {
                string xmlFile = "XmlConfiguration.xml";                 

                provider = new XmlFileConfigurationProvider<XmlFileConfiguration>()
                {
                    XmlConfigurationFile = xmlFile,
                    EncryptionKey = "ultra-seekrit",  // use a generated value here
                    PropertiesToEncrypt = "Password,AppConnectionString"
                    // UseBinarySerialization = true                     
                };
            }

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

      
    }

}
