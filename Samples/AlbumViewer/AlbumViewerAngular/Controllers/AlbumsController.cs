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
    public class AlbumsController : Controller
    {
        [Route("albums")]
        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult Index()
        {
            var albumBus = new AlbumBusiness();
            return new JsonNetResult(albumBus.GetAllAlbums());
        }

        [Route("albums/{id}")]
        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult GetAlbum(int id = -1)
        {
            if (id < 1)
                throw new CallbackException("Invalid album id passed.");

            var albumBus = new AlbumBusiness();
            var album = albumBus.Load(id);
            if (album == null)
                throw new CallbackException("Invalid album id passed.");

            return new JsonNetResult(album);
        }

        [Route("albums/{id}")]
        [AcceptVerbs(HttpVerbs.Put)]
        public ActionResult SaveAlbum(int id, Album newAlbum)
        {
            bool llNew = id == -2 ? true : false;

            var albumBus = new AlbumBusiness();
            var album = albumBus.Load(id);

            if (!ModelState.IsValid)
                throw new CallbackException("Model binding failed.",500);

            // TODO: Hook up save code


            return new JsonNetResult(album);
        }

        

        [Route("albums")]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult SaveAlbum(Album album)
        {
            return SaveAlbum(-2,album);
        }

    }

   public class ApiException : Exception
    {
        public int StatusCode { get; set; }
        public ApiException(string message, int statusCode = 500) :
            base(message)
        {
            StatusCode = StatusCode;
        }
        public ApiException(Exception ex, int statusCode = 500) : base(ex.Message)
        {
            StatusCode = statusCode;
        }
    }
}