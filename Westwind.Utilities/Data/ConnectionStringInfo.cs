using System;
using System.Configuration;
using System.Data.Common;
using Westwind.Utilities.Properties;

namespace Westwind.Utilities.Data
{
    /// <summary>
    /// Used to parse a connection string or connection string name 
    /// into a the base connection  string and dbProvider.
    /// 
    /// If a connection string is passed that's just used.
    /// If a ConnectionString entry name is passed the connection 
    /// string is extracted and the provider parsed.
    /// </summary>
    public class ConnectionStringInfo
    {
        /// <summary>
        /// The default connection string provider
        /// </summary>
        public static string DefaultProviderName = "System.Data.SqlClient";

        /// <summary>
        /// The connection string parsed
        /// </summary>
        public string ConnectionString { get; set; }

        /// <summary>
        /// The DbProviderFactory parsed from the connection string
        /// or default provider
        /// </summary>
        public DbProviderFactory Provider { get; set; }


        /// <summary>
        /// Figures out the Provider and ConnectionString from either a connection string
        /// name in a config file or full  ConnectionString and provider.         
        /// </summary>
        /// <param name="connectionString">Config file connection name or full connection string</param>
        /// <param name="providerName">optional provider name. If not passed with a connection string is considered Sql Server</param>
        public static ConnectionStringInfo GetConnectionStringInfo(string connectionString, string providerName = null)
        {            
            var info = new ConnectionStringInfo();

            if (string.IsNullOrEmpty(connectionString))
                throw new InvalidOperationException(Resources.AConnectionStringMustBePassedToTheConstructor);

            if (!connectionString.Contains("="))
            {
                // it's a connection string entry
                var connInfo = ConfigurationManager.ConnectionStrings[connectionString];
                if (connInfo != null)
                {
                    if (!string.IsNullOrEmpty(connInfo.ProviderName))
                        info.Provider = DbProviderFactories.GetFactory(connInfo.ProviderName);
                    else
                        info.Provider = DbProviderFactories.GetFactory(DefaultProviderName);

                    connectionString = connInfo.ConnectionString;
                }
                else
                    throw new InvalidOperationException(Resources.InvalidConnectionStringName + ": " + connectionString);
            }
            else
            {
                if (providerName == null)
                    providerName = DefaultProviderName;
                info.Provider = DbProviderFactories.GetFactory(providerName);
            }

            info.ConnectionString = connectionString;

            return info;
        }
    }
}
