// Copyright (c) .NET Foundation. All rights reserved. 
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace live.asp.net.Services
{
    public static class ServiceExtensions
    {
        public static IServiceCollection AddCachedWebRoot(this IServiceCollection services)
        {
            // Turn off compaction on memory pressure as it results in things being evicted during the priming of the
            // cache on application start.
            services.AddMemoryCache(options => options.CompactOnMemoryPressure = false);
            services.AddSingleton<CachedWebRootFileProvider>();
            services.AddSingleton<IConfigureOptions<StaticFileOptions>, StaticFileOptionsSetup>();
            return services;
        }
    }
}
