// Copyright (c) .NET Foundation. All rights reserved. 
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;

namespace live.asp.net.Services
{
    public class AppStart : IStartupFilter
    {
        private readonly IApplicationLifetime _appLifetime;
        private readonly CachedWebRootFileProvider _cachedWebRoot;
        private readonly ILogger _logger;

        public AppStart(IApplicationLifetime appLifetime, CachedWebRootFileProvider cachedWebRoot, ILogger<AppStart> logger)
        {
            _appLifetime = appLifetime;
            _logger = logger;
            _cachedWebRoot = cachedWebRoot;
        }

        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next) => app =>
        {
            // Call next now to ensure that logger providers added by other StartupFilters like Application Insights are configured
            next(app);

            // Log application start/stop events
            if (_logger.IsEnabled(LogLevel.Information))
            {
                _appLifetime.ApplicationStarted.Register(() =>
                {
                    _logger.LogInformation("Application started");
                });
                _appLifetime.ApplicationStopping.Register(() =>
                {
                    _logger.LogInformation("Application stopping");
                });
                _appLifetime.ApplicationStopped.Register(() =>
                {
                    _logger.LogInformation("Application stopped");
                });
            }

            // Prime the cached web root file provider for static file serving
            _cachedWebRoot.PrimeCache();
        };
    }
}
