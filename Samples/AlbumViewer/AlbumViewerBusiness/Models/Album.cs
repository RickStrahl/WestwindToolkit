using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace AlbumViewerBusiness
{
    public class Album
    {
        public int Id { get; set; }

     
        public string Title { get; set; }
        public string Description { get; set; }
        public int Year { get; set; }
        public string ImageUrl { get; set; }
        public string AmazonUrl { get; set; }
        public string SpotifyUrl { get; set; }

        [ForeignKey("Artist")]
        public int? ArtistId { get; set; }

        public virtual Artist Artist { get; set; }
        public virtual IList<Track> Tracks { get; set; }        

        public Album()
        {        
        }

    }
} 