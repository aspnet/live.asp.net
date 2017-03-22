﻿// Copyright (c) .NET Foundation. All rights reserved. 
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using live.asp.net.Models;

namespace live.asp.net.Services
{
    public interface ILiveShowDetailsService
    {
        Task LoadAsync(ILiveShowDetails liveShowDetails);

        Task SaveAsync(ILiveShowDetails liveShowDetails);
    }
}
