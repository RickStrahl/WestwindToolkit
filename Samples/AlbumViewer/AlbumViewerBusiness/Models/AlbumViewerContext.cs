using System;
using System.Collections.Generic;
using System.Data.Entity;
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

        public DbSet<Album> Albums { get; set; }
        public DbSet<Artist> Artists { get; set; }
        public DbSet<Track> Tracks { get; set; }


        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            Database.SetInitializer<AlbumViewerContext>(new CreateDatabaseIfNotExists<AlbumViewerContext>());
        }
    }
}
