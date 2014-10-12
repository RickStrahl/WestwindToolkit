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
    public class JsonFileConfiguration : Westwind.Utilities.Configuration.AppConfiguration
    {
        public string ApplicationName { get; set; }
        public DebugModes DebugMode { get; set; }
        public int MaxDisplayListItems { get; set; }
        public bool SendAdminEmailConfirmations { get; set; }
        public string Password { get; set; }
        public string AppConnectionString { get; set; }
        public LicenseInformation License { get; set; }

        // Must implement public default constructor
        public JsonFileConfiguration()
        {
            ApplicationName = "Configuration Tests";
            DebugMode = DebugModes.Default;
            MaxDisplayListItems = 15;
            SendAdminEmailConfirmations = false;
            Password = "seekrit";
            AppConnectionString = "server=.;database=hosers;uid=bozo;pwd=seekrit;";
            License = new LicenseInformation
            {
                Name = "Rick",
                Company = "West Wind",
                LicenseKey = "RickWestWind-533112"
            };
        }


        // Automatically initialize with default config and config file
        public void Initialize(string configFile)
        {
            base.Initialize(configData: configFile);
        }

        protected override IConfigurationProvider OnCreateDefaultProvider(string sectionName, object configData)
        {
            string jsonFile = "JsonConfiguration.txt";
            if (configData != null)
                jsonFile = configData as string;

            var provider = new JsonFileConfigurationProvider<JsonFileConfiguration>()
            {
                JsonConfigurationFile = jsonFile,
                EncryptionKey = "ultra-seekrit",  // use a generated value here
                PropertiesToEncrypt = "Password,AppConnectionString,License.LicenseKey"
            };

            return provider;
        }
    }
}
