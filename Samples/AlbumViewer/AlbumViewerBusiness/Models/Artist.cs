using System.ComponentModel.DataAnnotations;

namespace AlbumViewerBusiness
{
    public class Artist
    {
        public int Id { get; set; }

        [StringLength(128)]
        public string ArtistName { get; set; }        
        public string Description { get; set; }
        [StringLength(256)]
        public string ImageUrl { get; set; }
        [StringLength(256)]
        public string AmazonUrl { get; set; }        
    }
}