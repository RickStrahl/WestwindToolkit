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

        /// <summary>
        /// Initialize from Xml string
        /// </summary>
        /// <param name="xml"></param>
        public void Initialize(string xml)
        {
            base.Initialize(configData: xml);
        }


        protected override void OnInitialize(IConfigurationProvider provider = null, 
                                             string sectionName = null, 
                                             object configData = null)
        {
            if (provider == null)
            {
                provider = new StringConfigurationProvider<StringConfiguration>()
                {
                    EncryptionKey = "ultra-seekrit",  // use a generated value here
                    PropertiesToEncrypt = "Password,AppConnectionString",
                };
            }
                      
            // assign the provider
            Provider = provider;

            // read config from string
            if (configData != null && configData is string)
                Read(configData as string);        
        }

        public string ApplicationName { get; set; }
        public DebugModes DebugMode { get; set; }
        public int MaxDisplayListItems { get; set; }
        public bool SendAdminEmailConfirmations { get; set; }
        public string Password { get; set; }
        public string AppConnectionString { get; set; }

       
    }

}
