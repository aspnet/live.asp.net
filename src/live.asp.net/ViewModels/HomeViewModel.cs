using System;
using System.Collections.Generic;
using live.asp.net.Models;

namespace live.asp.net.ViewModels
{
    public class HomeViewModel
    {
        public bool IsOnAir => !HasAdminMessage && !string.IsNullOrEmpty(LiveShowEmbedUrl);

        public string LiveShowEmbedUrl { get; set; }

        public DateTime? NextShowDateUtc { get; set; }

        public bool NextShowScheduled => NextShowDateUtc.HasValue;

        public string AdminMessage { get; set; }

        public bool HasAdminMessage => !string.IsNullOrEmpty(AdminMessage);

        public IList<Show> PreviousShows { get; set; }

        public bool ShowPreviousShows => PreviousShows.Count > 0;

        public string MoreShowsUrl { get; set; }

        public bool ShowMoreShowsUrl => !string.IsNullOrEmpty(MoreShowsUrl);
    }
}
