using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Westwind.Data.EfCodeFirst;

namespace AlbumViewerBusiness
{
    public class AlbumBusiness : EfCodeFirstBusinessBase<Album,AlbumViewerContext>
    {
        public ICollection<Album> GetAllAlbums()
        {
            return Context.Albums
                .Include("Artist")
                .Include("Tracks")
                .OrderBy(alb => alb.Title)
                .ToList();
        }

        public ICollection<Album> GetAlbumsForArtist(int artistId)
        {
            return Context.Albums
                .Include("Artist")
                .Include("Tracks")
                .Where(alb => alb.ArtistId == artistId)
                .OrderBy(alb => alb.Title)
                .ToList();
        }
    }
}
