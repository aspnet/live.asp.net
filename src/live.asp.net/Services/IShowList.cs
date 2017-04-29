// Copyright (c) .NET Foundation. All rights reserved. 
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using live.asp.net.Models;

namespace live.asp.net.Services
{
    public interface IShowList
    {
        IList<Show> PreviousShows { get; set; }

        string MoreShowsUrl { get; set; }
    }
}
