using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using live.asp.net.Models;

namespace live.asp.net.ViewModels
{
    public class HomeViewModel
    {
        public bool IsOnAir => !string.IsNullOrEmpty(LiveShowEmbedUrl);

        public string LiveShowEmbedUrl { get; set; }

        public IList<Show> PreviousShows { get; set; }

        public bool ShowPreviousShows => PreviousShows.Count > 0;

        public string MoreShowsUrl { get; set; }

        public bool ShowMoreShowsUrl => !string.IsNullOrEmpty(MoreShowsUrl);
    }
}
