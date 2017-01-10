// Copyright (c) .NET Foundation. All rights reserved. 
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Options;

namespace live.asp.net.Services
{
    public class StaticFileOptionsSetup : IConfigureOptions<StaticFileOptions>
    {
        private readonly CachedWebRootFileProvider _cachedWebRoot;

        public StaticFileOptionsSetup(CachedWebRootFileProvider cachedWebRoot)
        {
            _cachedWebRoot = cachedWebRoot;
        }

        public void Configure(StaticFileOptions options)
        {
            options.FileProvider = _cachedWebRoot;
        }
    }
}
