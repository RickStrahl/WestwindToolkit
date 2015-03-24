using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;

namespace Westwind.Data.MongoDb
{

    /// <summary>
    /// Describes a Mongo Applications overall connectivity and 
    /// status for a MongoDb connection.
    /// 
    /// This class handles some connection and Configuration
    /// tasks for Mongo globally.
    /// </summary>
    public class MongoDbContext
    {
        /// <summary>
        /// A full connection string to a MongoDb server
        /// Optionally include the database name as a path        
        /// </summary>
        public string ConnectionString { get; set; }

        /// <summary>
        /// The name of the database to access if not specified
        /// on the connectionstring path.
        /// </summary>
        public string DatabaseName { get; set; }

        static MongoDbContext()
        {
            var ctx = new MongoDbContext();
            ctx.SetConventions();
        }

        public MongoDbContext()
        {            
        }

        /// <summary>
        /// Sets MongoDb global Conventions
        /// 
        /// This method should be called only once on startup
        /// </summary>
        /// <param name="conventions"></param>
        public virtual void SetConventions(IEnumerable<IConvention> conventions = null)
        {
            // Map global convention to all serialization
            var pack = new ConventionPack();
            pack.Add(new IgnoreExtraElementsConvention(true));

            if (conventions != null)
                pack.AddRange(conventions);

            ConventionRegistry.Register("ApplicationConventions", pack, t => true);
        }

        /// <summary>
        /// Creates a connection to a databaseName based on the Databasename and 
        /// optional server connection string.
        /// 
        /// Returned Mongo DatabaseName 'connection' can be cached and reused.
        /// </summary>
        /// <param name="connectionString">Mongo server connection string.
        /// Can either be a connection string entry name from the ConnectionStrings
        /// section in the config file or a full server string.        
        /// If not specified looks for connectionstring entry in  same name as
        /// the context. Failing that mongodb://localhost is used.
        ///  
        /// Examples:
        /// MyDatabaseConnectionString   (ConnectionStrings Config Name)       
        /// mongodb://localhost
        /// mongodb://localhost:22011/MyDatabase
        /// mongodb://username:password@localhost:22011/MyDatabase        
        /// </param>        
        /// <param name="databaseName">Name of the databaseName to work with if not specified on the connection string</param>
        /// <returns>Database instance</returns>
        public virtual MongoDatabase GetDatabase(string connectionString = null, string databaseName = null)
        {
            // apply global values from this context if not passed
            if (string.IsNullOrEmpty(databaseName))
                databaseName = DatabaseName;
            if (string.IsNullOrEmpty(connectionString))
                connectionString = ConnectionString;

            // if not specified use connection string with name of type
            if (string.IsNullOrEmpty(connectionString))
                connectionString = GetType().Name;

            // is it a connection string name?
            if (!connectionString.Contains("://"))
            {
                var conn = ConfigurationManager.ConnectionStrings[connectionString];
                if (conn != null)
                    connectionString = conn.ConnectionString;
                else
                    connectionString = "mongodb://localhost";                
            }

            ConnectionString = connectionString;                

            var client = new MongoClient(connectionString);
            var server = client.GetServer();

            // is it provided on the connection string?
            if (string.IsNullOrEmpty(databaseName))
            {
                var uri = new Uri(connectionString);
                var path = uri.LocalPath;
                databaseName = uri.LocalPath.Replace("/", "");                
            }

            var db = server.GetDatabase(databaseName);

            if (db != null)
                DatabaseName = databaseName;

            return db;
        }
    }
}
