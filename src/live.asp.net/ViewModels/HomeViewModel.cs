// Copyright (c) .NET Foundation. All rights reserved. 
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using live.asp.net.Models;

namespace live.asp.net.ViewModels
{
    public class HomeViewModel
    {
        private static readonly string _dateTimeFormat = "yyyyMMddTHHmmssZ";

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

        public string AddToGoogleUrl
        {
            get
            {
                // reference: http://stackoverflow.com/a/21653600/22941
                var from = NextShowDateUtc?.ToString(_dateTimeFormat);
                var to = NextShowDateUtc?.AddMinutes(30).ToString(_dateTimeFormat);

                return $"https://www.google.com/calendar/render?action=TEMPLATE&text=ASP.NET Community Standup&dates={from}/{to}&details=https://live.asp.net/&location=&sf=true&output=xml";
            }
        }
    }
}
