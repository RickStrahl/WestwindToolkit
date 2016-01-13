using System;
using System.Collections.Generic;
using System.Linq;
using AlbumViewerBusiness;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AlbumViewerTests
{
    [TestClass]
    public class AritstTests
    {
        [TestMethod]
        public void GetAritstsTest()
        {
            var albumBus = new ArtistBusiness();
            var artists = albumBus.GetArtists();
            Assert.IsNotNull(artists, albumBus.ErrorMessage);

            var artistList = artists.ToList();

            Assert.IsTrue(artistList.Count > 0, "List should return some data.");
        }

        [TestMethod]
        public void GetArtistsWithCountsTest()
        {
            var albumBus = new ArtistBusiness();
            var artists = albumBus.GetArtistsWithAlbumCount() as IEnumerable<dynamic>;

            Assert.IsNotNull(artists, albumBus.ErrorMessage);

            
            foreach (var artist in artists)
            {                
                string name = artist.ArtistName as string;
                int count = artist.AlbumCount;
                Console.WriteLine(name + " (" + count + ")");
            }
        }

        [TestMethod]
        public void GetAlbumsTest()
        {
            var albumBus = new AlbumBusiness();
            var albums = albumBus.GetAllAlbums();
            Assert.IsNotNull(albums, albumBus.ErrorMessage);

            var albumList = albums.ToList();

            Console.WriteLine(albumList.Count);

            Assert.IsTrue(albumList.Count > 0, "List should return some data.");
        }

    }
}
