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

            
            foreach (object art in artists)
            {
                var artist = (dynamic) art;
                string name = artist.ArtistName;
                int count = artist.AlbumCount;
                Console.WriteLine(name + " (" + count + ")");
            }
        }
    }
}
