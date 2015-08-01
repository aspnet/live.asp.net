// Copyright (c) .NET Foundation. All rights reserved. 
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Security.Claims;
using System.Threading.Tasks;

namespace live.asp.net.Services
{
    public interface IShowsService
    {
        Task<ShowList> GetRecordedShowsAsync(ClaimsPrincipal user, bool disableCache);
    }
}
