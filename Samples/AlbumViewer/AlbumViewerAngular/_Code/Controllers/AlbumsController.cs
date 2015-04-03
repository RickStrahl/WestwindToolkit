using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AlbumViewerBusiness;
using Westwind.Utilities;
using Westwind.Web;
using Westwind.Web.Mvc;
using HttpVerbs = System.Web.Mvc.HttpVerbs;

namespace AlbumViewerAngular.Controllers
{
    [CallbackExceptionHandler()]
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
                throw new CallbackException("Model binding failed.", 500);

            // TODO: Hook up save code
            
            DataUtils.CopyObjectData(newAlbum, album, "");

            return new JsonNetResult(album);
        }

        

        [Route("albums")]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult SaveAlbum(Album dtoAlbum)
        {
            if (dtoAlbum == null)
                throw new CallbackException("No album provided for saving.", 500);

            if (!ModelState.IsValid)
                throw new CallbackException("Model binding failed.", 500);

            var id = dtoAlbum.Id;

            var albumBus = new AlbumBusiness();
            if (!albumBus.SaveAlbum(dtoAlbum))
                throw new CallbackException("Album save failed: " + albumBus.ErrorMessage);

            return new JsonNetResult(albumBus.Entity);
        }

        [Route("throw")]
        public ActionResult Throw()
        {
            string value = null;
            throw new  ArgumentException("Explicitly thrown error");
            return Content(value.ToString());
        }


    }

   //public class ApiException : Exception
   // {
   //     public int StatusCode { get; set; }
   //     public ApiException(string message, int statusCode = 500) :
   //         base(message)
   //     {
   //         StatusCode = StatusCode;
   //     }
   //     public ApiException(Exception ex, int statusCode = 500) : base(ex.Message)
   //     {
   //         StatusCode = statusCode;
   //     }
   // }
}