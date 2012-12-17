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
    public class DatabaseConfiguration : Westwind.Utilities.Configuration.AppConfiguration
    {

        // Must implement public default constructor
        public DatabaseConfiguration()
        {
            ApplicationName = "Configuration Tests";
            DebugMode = DebugModes.Default;
            MaxDisplayListItems = 15;
            SendAdminEmailConfirmations = false;
            Password = "seekrit";
            AppConnectionString = "server=.;database=hosers;uid=bozo;pwd=seekrit;";
        }

        public void Initialize(string connectionString, string tableName = null)
        {
            base.Initialize(configData: new { ConnectionString = connectionString, Tablename = tableName });
        }

        protected override void OnInitialize(IConfigurationProvider provider = null,
                                      string sectionName = null,
                                      object configData = null)
        {

            if (provider == null)
            {
                // default connect values
                string connectionString =  "LocalDatabaseConnection";
                string tableName = "ConfigurationData";

                // ConfigData: new { ConnectionString = "...", Tablename = "..." }
                if (configData != null)
                {
                    dynamic data = configData;
                    connectionString = data.ConnectionString;
                    tableName = data.Tablename;                       
                }

                provider = new SqlServerConfigurationProvider<DatabaseConfiguration>()
                {
                    ConnectionString = connectionString,
                    Tablename = tableName,
                    ProviderName = "System.Data.SqlServerCe.4.0",
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
