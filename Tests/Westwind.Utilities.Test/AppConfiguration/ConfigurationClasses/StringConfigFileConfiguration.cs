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
    public class StringConfiguration : Westwind.Utilities.Configuration.AppConfiguration
    {
        public string ApplicationName { get; set; }
        public DebugModes DebugMode { get; set; }
        public int MaxDisplayListItems { get; set; }
        public bool SendAdminEmailConfirmations { get; set; }
        public string Password { get; set; }
        public string AppConnectionString { get; set; }

        // Must implement public default constructor
        public StringConfiguration()
        {
            ApplicationName = "Configuration Tests";
            DebugMode = DebugModes.Default;
            MaxDisplayListItems = 15;
            SendAdminEmailConfirmations = false;
            Password = "seekrit";
            AppConnectionString = "server=.;database=hosers;uid=bozo;pwd=seekrit;";
        }

        protected override IConfigurationProvider OnCreateDefaultProvider(string sectionName, object configData)
        {
            var provider = new StringConfigurationProvider<StringConfiguration>()
            {
                InitialStringData = configData as String,
                EncryptionKey = "ultra-seekrit",  // use a generated value here
                PropertiesToEncrypt = "Password,AppConnectionString",
            };

            return provider;
        }

        /// <summary>
        /// Optional - easier overload for xml string loading
        /// </summary>
        /// <param name="xml"></param>
        public void Initialize(string xml)
        {
            base.Initialize(configData: xml);
        }
    }
}
