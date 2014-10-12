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
public class CustomConfigFileConfiguration : Westwind.Utilities.Configuration.AppConfiguration
{
    public string ApplicationName { get; set; }
    public DebugModes DebugMode { get; set; }
    public int MaxDisplayListItems { get; set; }
    public bool SendAdminEmailConfirmations { get; set; }
    public string Password { get; set; }
    public string AppConnectionString { get; set; }
    public LicenseInformation License { get; set; }
    public List<string> ServerList { get; set;  }

        public CustomConfigFileConfiguration()
        {
            ApplicationName = "Configuration Tests";
            DebugMode = DebugModes.Default;
            MaxDisplayListItems = 15;
            SendAdminEmailConfirmations = false;
            Password = "seekrit";
            AppConnectionString = "server=.;database=hosers;uid=bozo;pwd=seekrit;";
            License = new LicenseInformation()
            {
                Company = "West Wind",
                Name = "Rick", 
                LicenseKey = "westwindrick-51123"
            };
            ServerList = new List<string>()
            {
                "DevServer",
                "Maximus",
                "Tempest"
            };
        }

        /// <summary>
        /// Override to provide a custom default provider (created when Initialize() is
        /// called with no parameters).
        /// </summary>
        /// <param name="sectionName"></param>
        /// <param name="configData"></param>
        /// <returns></returns>
        protected override IConfigurationProvider OnCreateDefaultProvider(string sectionName, object configData)
        {
            var provider = new ConfigurationFileConfigurationProvider<CustomConfigFileConfiguration>()
            {
                //ConfigurationFile = "CustomConfiguration.config",
                ConfigurationSection = sectionName,
                EncryptionKey = "ultra-seekrit",  // use a generated value here
                PropertiesToEncrypt = "Password,AppConnectionString,License.LicenseKey"
            };

            return provider;
        }
    }
}
