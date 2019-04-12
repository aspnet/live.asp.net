// Copyright (c) .NET Foundation. All rights reserved. 
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using live.asp.net.Models;

namespace live.asp.net.Services
{
    public class ShowList
    {
        public IList<Show> PreviousShows { get; set; } = new List<Show>();

        public IList<Show> UpcomingShows { get; set; } = new List<Show>();

        public string? MoreShowsUrl { get; set; }
    }
}
