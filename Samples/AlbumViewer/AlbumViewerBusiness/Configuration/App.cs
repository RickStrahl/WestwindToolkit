using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Westwind.Utilities.Configuration;

namespace AlbumViewerBusiness
{
    public class App
    {
        public static AlbumViewerConfiguration Configuration { get; set; }

        static App()
        {
            // set up global config object and load initial config data
            Configuration = new AlbumViewerConfiguration();            
            Configuration.Initialize();
        }
    }

}
