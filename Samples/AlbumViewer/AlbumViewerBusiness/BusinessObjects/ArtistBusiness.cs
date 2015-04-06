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

        /// <summary>
        /// Returns an anonymous, filtered artist object list that
        /// includes an album count.
        /// </summary>
        /// <param name="nameFilter"></param>
        /// <returns>Anonymous List of artist object plus AlbumCount property</returns>
        public object GetArtistsWithAlbumCount(string nameFilter = null)
        {
            var artists = Context.Artists
               .Where(art => !string.IsNullOrEmpty(art.ArtistName));

            if (!string.IsNullOrEmpty(nameFilter))
                artists = artists.Where(art => art.ArtistName.Contains(nameFilter));

            return artists
                .OrderBy(art => art.ArtistName)
                .Select(art => new
                {
                    art.ArtistName,
                    art.Description,
                    art.ImageUrl,
                    art.Id,
                    AlbumCount = Context.Albums.Count(alb => alb.ArtistId == art.Id)
                })
                .ToList();
        }

        /// <summary>
        /// Artist look up used for Auto-Complete list in the Album entry form
        /// </summary>
        /// <param name="searchFilter"></param>
        /// <returns></returns>
        public ICollection<Artist> GetArtistLookup(string searchFilter)
        {
             return Context.Artists
                           .Where(art => art.ArtistName.StartsWith(searchFilter))
                           .ToList();
        }


        protected override bool OnBeforeDelete(Artist entity)
        { 	   
                var albums = Context.Albums.Where(alb => alb.ArtistId == entity.Id);

                var albumBus = new AlbumBusiness(this);
                foreach (var album in albums)
                {
                    if (!albumBus.Delete(album.Id,true,true))
                    {
                        SetError(albumBus.ErrorMessage);
                        return false;
                    }
                }
                return base.OnBeforeDelete(entity);
        }



    }
    
}
