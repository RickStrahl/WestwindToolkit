using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AlbumViewerBusiness;
using Westwind.Web;
using Westwind.Web.Mvc;
using HttpVerbs = System.Web.Mvc.HttpVerbs;

namespace AlbumViewerAngular.Controllers
{
    [CallbackExceptionHandler()]
    public class ArtistsController : Controller
    {

        [Route("artists")]
        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult Index()
        {
            var artistBus = new ArtistBusiness();
            var artists = artistBus.GetArtistsWithAlbumCount();
            
            return new JsonNetResult(artists);
        }


        [Route("artistlookup")]
        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult LookupArtist(string search)
        {
            var artistBus = new ArtistBusiness();
            var artists = artistBus.GetArtistLookup(search)
                .Select(art => new
                {
                    name = art.ArtistName,
                    id = art.ArtistName,
                    artistId = art.Id
                });

            return new JsonNetResult(artists.ToList());
        }

        [Route("artists/{id}")]
        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult Get(int id = -1)
        {
            if (id < 1)
                throw new CallbackException("Invalid id passed.");

            var artistBus = new ArtistBusiness();
            var artist = artistBus.Load(id);
            if (artist == null)
                throw new CallbackException("Invalid id passed.");

            var albumBus = new AlbumBusiness();
            var albums = albumBus.GetAlbumsForArtist(artist.Id);

            return new JsonNetResult(new ArtistResponse
            {
                Artist = artist,
                Albums = albums
            });
        }


        [Route("artists")]
        [AcceptVerbs(HttpVerbs.Post | HttpVerbs.Put)]
        public ActionResult SaveArtist(Artist postedArtist)
        {            
            if (!ModelState.IsValid)
                throw new CallbackException("Model binding failed.", 500);

            var artistBus = new ArtistBusiness();

            // attach to context
            var artist = artistBus.Attach(postedArtist);

            if (!artistBus.Validate())
            {
                throw new CallbackException("Please correct the following: " + artistBus.ValidationErrors.ToString());
            }

            if (!artistBus.Save())
                throw new CallbackException("Unable to save artist: " + artistBus.ErrorMessage);

            var albumBus = new AlbumBusiness();
            var albums = albumBus.GetAlbumsForArtist(artist.Id);

            return new JsonNetResult(new ArtistResponse
            {
                Artist = artist,
                Albums = albums
            });
        }


        [Route("artists/delete/{id}")]
        [AcceptVerbs(HttpVerbs.Get)] // DELETE would not work here???
        public ActionResult DeleteArtist(int id)
        {
            if (id < 1)
                throw new CallbackException("Invalid Id passed.");

            var artistBus = new ArtistBusiness();
            if (!artistBus.Delete(id,true))
                throw new CallbackException("Couldn't delete artist: " + artistBus.ErrorMessage);

            return new JsonNetResult(true);
        }       
    }

    public class ArtistResponse
    {
        public Artist Artist { get; set; }
        public ICollection<Album> Albums { get; set; }
    }

}