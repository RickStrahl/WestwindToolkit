using System;
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
    }
}
