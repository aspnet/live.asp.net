// Copyright (c) .NET Foundation. All rights reserved. 
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

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

        public string AddToGoogleUrl
        {
            get
            {
                // reference: http://stackoverflow.com/a/21653600/22941

                const string eventTitle = "ASP.NET Community Standup";
                const string dateTimeFormat = "yyyyMMddTHHmmssZ";
                const string url = "https://live.asp.net/";

                var dates = $"{NextShowDateUtc?.ToString(dateTimeFormat)}/{NextShowDateUtc?.AddMinutes(30).ToString(dateTimeFormat)}";

                return $"https://www.google.com/calendar/render?action=TEMPLATE&text={eventTitle}&dates={dates}&details={url}&location=&sf=true&output=xml";
            }
        }
    }
}
