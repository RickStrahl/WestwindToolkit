using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Westwind.Data.EfCodeFirst;

namespace AlbumViewerBusiness
{
    public class ArtistBusiness : EfCodeFirstBusinessBase<Artist,AlbumViewerContext>
    {
        public ICollection<Artist> GetArtists(string nameFilter = null)
        {
            var artists = Context.Artists
                .Where(art => !string.IsNullOrEmpty(art.ArtistName));

            if (!string.IsNullOrEmpty(nameFilter))
                artists = artists.Where(art => art.ArtistName.Contains(nameFilter));

            return artists
                .OrderBy(art => art.ArtistName)
                .ToList();
        }

        public ICollection<Artist> GetArtistLookup(string searchFilter)
        {
             return Context.Artists
                           .Where(art => art.ArtistName.StartsWith(searchFilter))
                           .ToList();
        }

        
    }
    
}
