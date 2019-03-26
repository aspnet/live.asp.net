// Copyright (c) .NET Foundation. All rights reserved. 
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using live.asp.net.Formatters;
using live.asp.net.Services;
using Microsoft.ApplicationInsights.AspNetCore.Extensions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.AzureAD.UI;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace live.asp.net
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; set; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<AppSettings>(Configuration.GetSection("AppSettings"));

            var azureAdClientId = Configuration["AzureAd:ClientId"];
            var adminRole = Configuration["AzureAd:AdminRole"];

            if (string.IsNullOrEmpty(azureAdClientId) || string.IsNullOrEmpty(adminRole))
            {
                throw new InvalidOperationException("Missing configuration values for Azure AD AuthN/Z.");
            }

            services.AddAuthentication(AzureADDefaults.AuthenticationScheme)
                    .AddAzureAD(options => Configuration.Bind("AzureAd", options));

            services.AddAuthorization(options =>
                options.AddPolicy("Admin", policyBuilder =>
                    policyBuilder.RequireRole(adminRole)
                )
            );

            services.AddMvc(options => options.OutputFormatters.Add(new iCalendarOutputFormatter()))
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            services.AddCachedWebRoot();
            services.AddSingleton<IStartupFilter, AppStart>();
            services.AddScoped<IShowsService, YouTubeShowsService>();
            services.AddSingleton<IObjectMapper, SimpleMapper>();
            services.AddSingleton<IDeploymentEnvironment, DeploymentEnvironment>();
            services.AddSingleton<IConfigureOptions<ApplicationInsightsServiceOptions>, ApplicationInsightsServiceOptionsSetup>();
            services.AddSingleton<CookieConsentService>();

            if (string.IsNullOrEmpty(Configuration["AppSettings:AzureStorageConnectionString"]))
            {
                services.AddSingleton<ILiveShowDetailsService, FileSystemLiveShowDetailsService>();
            }
            else
            {
                services.AddSingleton<ILiveShowDetailsService, AzureStorageLiveShowDetailsService>();
            }
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.Use((context, next) => context.Request.Path.StartsWithSegments("/ping")
                ? context.Response.WriteAsync("pong")
                : next()
            );

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                //app.UseHsts();
                app.UseExceptionHandler("/error");
            }

            app.UseRewriter(new RewriteOptions()
                .AddIISUrlRewrite(env.ContentRootFileProvider, "urlRewrite.config"));

            app.UseStatusCodePages();

            app.UseStaticFiles();

            app.UseAuthentication();

            app.UseMvc();
        }
    }
}
