using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Westwind.Data.EfCodeFirst;

namespace AlbumViewerBusiness
{
    public class AlbumViewerContext : EfCodeFirstContext
    {
        /// <summary>
        /// Default contructor that uses connection string value from a
        /// config.json file: Data:MusicStore:ConnectionString)
        /// </summary>
        public AlbumViewerContext()
            : base("AlbumViewerContext")
        {
        }

        public AlbumViewerContext(string connectionString) : base(connectionString)
        {

        }

        public DbSet<Album> Albums { get; set; }
        public DbSet<Artist> Artists { get; set; }
        public DbSet<Track> Tracks { get; set; }

        public DbSet<User> Users { get; set; }


        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            Database.SetInitializer<AlbumViewerContext>(new AlbumViewerInitializer());

        }        
    }


    public class AlbumViewerInitializer : CreateDatabaseIfNotExists<AlbumViewerContext>
    {
        public override void InitializeDatabase(AlbumViewerContext context)
        {
            base.InitializeDatabase(context);
        }

        /*
            To reset database: 
            ----------------
            drop table users
            drop table tracks
            drop table albums
            drop table artists
            drop table __MigrationHistory
        */

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        protected override void Seed(AlbumViewerContext context)
        {
            base.Seed(context);
            
            // serves as model warmup and db initialization            
            if (!context.Users.Any())
            {
                string jsonFile = Path.Combine(App.Configuration.ApplicationRootPath, "data\\albums.js");
                string json = File.ReadAllText(jsonFile);

                AlbumViewerDataImporter.ImportFromJson(context, json);
            }
        }
    }
}
