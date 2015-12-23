using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Al_Browser.Models
{
    public class Favorite
    {
        public string Title { get; set; }
        public string Url { get; set; }
        public string Favicon { get; set; }

        public Favorite(string Title, string Url)
        {
            this.Title = Title;
            this.Url = Url;
            Favicon = "http://google.com/s2/favicons?domain=" + Url;
        }
    }
}
