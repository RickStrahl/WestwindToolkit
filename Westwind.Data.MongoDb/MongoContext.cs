using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;

namespace Westwind.Data.MongoDb
{
    public abstract class MongoContext
    {
        public string ConnectionString { get; set; }
        public string Database { get; set; }


        /// <summary>
        /// Sets MongoDb global Conventions
        /// </summary>
        /// <param name="conventions"></param>
        public static void SetConventions(IEnumerable<IConvention> conventions = null)
        {
            // Map global convention to all serialization
            var pack = new ConventionPack();
            pack.Add(new IgnoreExtraElementsConvention(true));

            if (conventions != null)
                pack.AddRange(conventions);

            ConventionRegistry.Register("ApplicationConventions", pack, t => true);
        }

        /// <summary>
        /// Creates a connection to a database based on the Databasename and 
        /// optional server connection string.
        /// 
        /// Returned Mongo Database 'connection' can be cached and reused.
        /// </summary>
        /// <param name="database">Name of the database to work with</param>
        /// <param name="connectionString">Server connection string</param>        
        /// <returns></returns>
        /// <remarks>Important: 
        /// This only works if you implement a DbContext contstructor on your custom context
        /// that accepts a connectionString parameter.
        /// </remarks>
        public virtual MongoDatabase GetDatabase(string database = null, string connectionString = null)
        {
            // apply global values from this context if not passed
            if (string.IsNullOrEmpty(database))
                database = Database;
            if (string.IsNullOrEmpty(connectionString))
                connectionString = ConnectionString;
            
            if (string.IsNullOrEmpty(connectionString))
                connectionString = "mongodb://localhost";

            var client = new MongoClient(connectionString);
            var server = client.GetServer();

            if (string.IsNullOrEmpty(database))
            {
                var uri = new Uri(connectionString);
                var path = uri.LocalPath;
            }

            var db = server.GetDatabase(database);

            return db;
        }
    }
}
