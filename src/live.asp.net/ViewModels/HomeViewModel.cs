using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using live.asp.net.Models;

namespace live.asp.net.ViewModels
{
    public class HomeViewModel
    {
        public IList<Show> PreviousShows { get; set; }
        
        public string MoreShowsUrl { get; set; }
    }
}
