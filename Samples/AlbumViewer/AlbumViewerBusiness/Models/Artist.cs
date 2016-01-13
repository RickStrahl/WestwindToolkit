using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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