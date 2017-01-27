// Copyright (c) .NET Foundation. All rights reserved. 
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using live.asp.net.Middlesware;

namespace live.asp.net.Middlesware
{
    /// <summary>
    /// Middleware that sets HSTS response header for ensuring subsequent requests are made over HTTPS only.
    /// See https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/Strict-Transport-Security for more details.
    /// </summary>
    public class HstsMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly HstsOptions _options;
        private readonly string _headerValue;

        public HstsMiddleware(RequestDelegate next, HstsOptions options)
        {
            _next = next;
            _options = options;
            _headerValue = FormatHeader(options);
        }

        public async Task Invoke(HttpContext httpContext)
        {
            await _next(httpContext);

            // It's only valid to set the HSTS header over HTTPS itself
            if (httpContext.Request.Host.Host != "localhost"
                && httpContext.Request.IsHttps
                && !httpContext.Request.Headers.ContainsKey("Strict-Transport-Security"))
            {
                httpContext.Response.Headers.Add("Strict-Transport-Security", _headerValue);
            }
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
        public static IApplicationBuilder UseHstsMiddleware(this IApplicationBuilder builder)
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
        public static IApplicationBuilder UseHstsMiddleware(this IApplicationBuilder builder, HstsOptions options)
        {
            return builder.UseMiddleware<HstsMiddleware>(options);
        }
    }
}
