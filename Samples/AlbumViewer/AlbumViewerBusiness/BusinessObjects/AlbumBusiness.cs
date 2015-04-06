using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Westwind.Data.EfCodeFirst;
using Westwind.Utilities;

namespace AlbumViewerBusiness
{
    public class AlbumBusiness : EfCodeFirstBusinessBase<Album,AlbumViewerContext>
    {
        public AlbumBusiness() 
        {
            
        }

        public AlbumBusiness(IBusinessObject<AlbumViewerContext> parentBusiness): base(parentBusiness)
        { }


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
                .OrderBy(alb => alb.Year)
                .ToList();
        }

        /// <summary>
        /// Creates a ready to be saved instance of EF CodeFirst Context
        /// </summary>
        /// <param name="?"></param>
        /// <returns></returns>
        public bool SaveAlbum(Album dtoAlbum)
        {
            if (dtoAlbum == null)
            {
                SetError("No album passed to update from.");
                return false;
            }

            var album = Load(dtoAlbum.Id);
            if (album == null)
                album = NewEntity();
            
            var artist = Context.Artists.FirstOrDefault(a => a.ArtistName == dtoAlbum.Artist.ArtistName);
            if (artist == null)
            {
                album.Artist = new ArtistBusiness().NewEntity();
                DataUtils.CopyObjectData(dtoAlbum.Artist, album.Artist, "Id");
            }
            else
            {
                album.Artist = artist;
                album.ArtistId = artist.Id;
            }

            DataUtils.CopyObjectData(dtoAlbum, album, "Id,Albums,Artist,Tracks,ArtistId");

            // delete any tracks that aren't there anymore
            album.Tracks
                .Where(a => dtoAlbum.Tracks.All(trck => trck.Id != a.Id))
                .ToList()
                .ForEach(dtrack => album.Tracks.Remove(dtrack));

            foreach (var dtoTrack in dtoAlbum.Tracks)
            {
                var track = Context.Tracks.FirstOrDefault(t => t.Id == dtoTrack.Id);
                if (track == null)
                    track = Context.Tracks.Add(new Track());

                // manual updates
                track.AlbumId = dtoTrack.AlbumId;
                track.SongName = dtoTrack.SongName;
                track.Length = dtoTrack.Length;                               
            }
            
            return Save();
        }

        protected override bool OnBeforeDelete(Album entity)
        {
            // explicitly have to remove tracks first
            foreach (var track in entity.Tracks.ToList())
            {
                Context.Tracks.Remove(track);                
            }
            return base.OnBeforeDelete(entity);
        }


    }
}
