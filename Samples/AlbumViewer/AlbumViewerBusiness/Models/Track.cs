using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace AlbumViewerBusiness
{
    public class Track
    {
        public int Id { get; set; }
        
        public int AlbumId { get; set; }

        public int ArtistId { get; set; }
        [StringLength(128)]
        public string SongName { get; set; }
        [StringLength(10)]
        public string Length { get; set; }
        public int Bytes { get; set; }
        public decimal UnitPrice { get; set; }

        public override string ToString()
        {
            return SongName;
        }
    }
}