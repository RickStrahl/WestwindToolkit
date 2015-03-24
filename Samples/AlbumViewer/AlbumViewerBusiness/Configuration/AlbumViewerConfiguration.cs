using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Westwind.Utilities.Configuration;

namespace AlbumViewerBusiness
{
    class AlbumViewerConfiguration : AppConfiguration
    {
        public string ApplicationName { get; set; }
        public string BaseUrl { get; set; }
        public int MaxAlbumsToReturn { get; set; }
    }

}
