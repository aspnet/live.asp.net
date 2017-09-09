// Copyright (c) .NET Foundation. All rights reserved. 
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using live.asp.net.Middlesware;

namespace live.asp.net.Middlesware
{
    /// <summary>
    /// Middleware that sets HSTS response header for ensuring subsequent requests are made over HTTPS only.
    /// See https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/Strict-Transport-Security for more details.
    /// </summary>
    public class HstsMiddleware
    {
        private const string _hstsHeaderName = "Strict-Transport-Security";
        private readonly RequestDelegate _next;
        private readonly HstsOptions _options;
        private readonly string _headerValue;
        private readonly ILogger<HstsMiddleware> _logger;

        public HstsMiddleware(RequestDelegate next, HstsOptions options, ILogger<HstsMiddleware> logger)
        {
            _next = next;
            _options = options;
            _logger = logger;
            _headerValue = FormatHeader(options);
        }

        public Task Invoke(HttpContext httpContext)
        {
            if (httpContext.Response.HasStarted)
            {
                _logger.LogInformation("HSTS response header cannot be set as response writing has already started.");
                return _next(httpContext);
            }

            if (!_options.EnableLocalhost && string.Equals(httpContext.Request.Host.Host, "localhost", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogDebug("HSTS response header will not be set for localhost.");
                return _next(httpContext);
            }

            if (!httpContext.Request.IsHttps)
            {
                _logger.LogDebug("HSTS response header will not be set as the scheme is not HTTPS.");
                return _next(httpContext);
            }

            if (httpContext.Request.Headers.ContainsKey(_hstsHeaderName))
            {
                _logger.LogDebug("HSTS response header is already set: {headerValue}", httpContext.Request.Headers[_hstsHeaderName]);
                return _next(httpContext);
            }

            _logger.LogDebug("Adding HSTS response header: {headerValue}", _headerValue);
            httpContext.Response.Headers.Add(_hstsHeaderName, _headerValue);

            return _next(httpContext);
        }

        private string FormatHeader(HstsOptions options)
        {
            var headerValue = "max-age=" + _options.MaxAge.TotalSeconds;

            if (_options.IncludeSubdomains)
            {
                headerValue += "; includeSubdomains";
            }

            if (_options.Preload)
            {
                headerValue += "; preload";
            }

            return headerValue;
        }

    }

    public class HstsOptions
    {
        /// <summary>
        /// The time that the browser should remember that this site is only to be accessed using HTTPS.
        /// Defaults to 365 days.
        /// </summary>
        public TimeSpan MaxAge { get; set; } = TimeSpan.FromDays(365);

        /// <summary>
        /// Whether this rule applies to all of the site's subdomains as well.
        /// Defaults to <c>true</c>.
        /// </summary>
        public bool IncludeSubdomains { get; set; } = true;

        /// <summary>
        /// See https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/Strict-Transport-Security#Preloading_Strict_Transport_Security for details.
        /// Defaults to <c>false</c>.
        /// </summary>
        public bool Preload { get; set; } = false;

        /// <summary>
        /// Whether HSTS headers will be sent when serving to localhost.
        /// Defaults to <c>false</c>;
        /// </summary>
        public bool EnableLocalhost { get; set; } = false;
    }
}

namespace Microsoft.AspNetCore.Builder
{
    public static class HstsMiddlewareExtensions
    {
        /// <summary>
        /// Add middleware that sets HSTS response header for ensuring subsequent requests are made over HTTPS only.
        /// See https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/Strict-Transport-Security for more details.
        /// </summary>
        /// <param name="builder">The <see cref="IApplicationBuilder"/>.</param>
        /// <returns>The <see cref="IApplicationBuilder"/>.</returns>
        public static IApplicationBuilder UseHsts(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<HstsMiddleware>(new HstsOptions());
        }

        /// <summary>
        /// Add middleware that sets HSTS response header for ensuring subsequent requests are made over HTTPS only.
        /// See https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/Strict-Transport-Security for more details.
        /// </summary>
        /// <param name="builder">The <see cref="IApplicationBuilder"/>.</param>
        /// <param name="options">The <see cref="HstsOptions"/> to use.</param>
        /// <returns>The <see cref="IApplicationBuilder"/>.</returns>
        public static IApplicationBuilder UseHsts(this IApplicationBuilder builder, HstsOptions options)
        {
            return builder.UseMiddleware<HstsMiddleware>(options);
        }
    }
}
