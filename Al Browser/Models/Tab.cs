using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Al_Browser.Models
{
    public class Tab
    {
        private string _favicon = "";

        public int Id { get; set; }
        public string Title { get; set; }
        public string Url { get; set; }
        public string Favicon
        {
            get { return _favicon; }
            set
            {
                _favicon = "http://google.com/s2/favicons?domain=" + value.Replace("http://", "").Replace("https://", "");
            }
        }

        public Tab(int Id, string Title, string Url)
        {
            this.Id = Id;
            this.Title = Title;
            this.Url = Url;
            Favicon = Url;
        }
    }
}