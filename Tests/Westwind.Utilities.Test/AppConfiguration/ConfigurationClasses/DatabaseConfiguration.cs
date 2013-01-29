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
        // Configuration store values 
        public string ApplicationName { get; set; }
        public DebugModes DebugMode { get; set; }
        public int MaxDisplayListItems { get; set; }
        public bool SendAdminEmailConfirmations { get; set; }
        public string Password { get; set; }
        public string AppConnectionString { get; set; }

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

    
        ///// <summary>
        ///// Override this method to create the custom default provider - in this case a database
        ///// provider with a few options.
        ///// </summary>
        protected override IConfigurationProvider OnCreateDefaultProvider(string sectionName, object configData)
        {
            string connectionString = "WestwindToolkitSamples";
            string tableName = "ConfigurationData";

            // ConfigData: new { ConnectionString = "...", Tablename = "..." }
            if (configData != null)
            {
                dynamic data = configData;
                connectionString = data.ConnectionString;
                tableName = data.Tablename;
            }

            var provider = new SqlServerConfigurationProvider<DatabaseConfiguration>()
                {
                    Key = 0,
                    ConnectionString = connectionString,
                    Tablename = tableName,
                    //ProviderName = "System.Data.SqlServerCe.4.0",
                    EncryptionKey = "ultra-seekrit", // use a generated value here
                    PropertiesToEncrypt = "Password,AppConnectionString"
                };

            return provider;
        }

        /// <summary>
        /// Optional - Create a custom overload with required parameters
        /// </summary>
        public void Initialize(string connectionString, string tableName = null)
        {
            // pass in anonymous object with parameters we're interested in
            // the OnCreateDefaultProvider reads the anonymous object values
            // and uses them for the SQL access object
            base.Initialize(configData: new { ConnectionString = connectionString, Tablename = tableName });
        }

    }

}