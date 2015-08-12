﻿// Copyright (c) .NET Foundation. All rights reserved. 
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using live.asp.net.Formatters;
using live.asp.net.Services;
using Microsoft.AspNet.Authentication.Cookies;
using Microsoft.AspNet.Authentication.OpenIdConnect;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Hosting;
using Microsoft.AspNet.Http;
using Microsoft.Framework.Configuration;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.Logging;
using Microsoft.Framework.Runtime;

namespace live.asp.net
{
    public class Startup
    {
        private readonly IHostingEnvironment _env;

        public Startup(IHostingEnvironment env, IApplicationEnvironment appEnv)
        {
            _env = env;

            var builder = new ConfigurationBuilder(appEnv.ApplicationBasePath)
                .AddJsonFile("config.json")
                .AddJsonFile($"config.{env.EnvironmentName}.json", optional: true);

            if (_env.IsDevelopment())
            {
                builder.AddUserSecrets();
                builder.AddApplicationInsightsSettings(developerMode: true);
            }

            builder.AddEnvironmentVariables();

            Configuration = builder.Build();
        }

        public IConfiguration Configuration { get; set; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<AppSettings>(Configuration.GetConfigurationSection("AppSettings"), optionsName: null);

            services.Configure<CookieAuthenticationOptions>(options =>
            {
                options.AutomaticAuthentication = true;
            });

            services.Configure<OpenIdConnectAuthenticationOptions>(options =>
            {
                options.DefaultToCurrentUriOnRedirect = true;
                options.AutomaticAuthentication = true;
                options.ClientId = Configuration["Authentication:AzureAd:ClientId"];
                options.Authority = Configuration["Authentication:AzureAd:AADInstance"] + Configuration["Authentication:AzureAd:TenantId"];
                options.PostLogoutRedirectUri = Configuration["Authentication:AzureAd:PostLogoutRedirectUri"];
                options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            });

            services.ConfigureAuthorization(options =>
            {
                options.AddPolicy("Admin", policyBuilder =>
                {
                    policyBuilder.RequireClaim(
                        "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name",
                        Configuration["Authorization:AdminUsers"].Split(',')
                    );
                });
            });

            services.AddApplicationInsightsTelemetry(Configuration);

            services.ConfigureMvc(mvc =>
            {
                mvc.OutputFormatters.Add(new iCalendarOutputFormatter());
            });
            services.AddMvc();

            services.AddScoped<IShowsService, YouTubeShowsService>();

            if (string.IsNullOrEmpty(Configuration["AppSettings:AzureStorageConnectionString"]))
            {
                services.AddSingleton<ILiveShowDetailsService, FileSystemLiveShowDetailsService>();
            }
            else
            {
                services.AddSingleton<ILiveShowDetailsService, AzureStorageLiveShowDetailsService>();
            }
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.MinimumLevel = LogLevel.Warning;
            loggerFactory.AddConsole();

            if (env.IsProduction())
            {
                app.UseApplicationInsightsRequestTelemetry();
            }

            if (env.IsDevelopment())
            {
                app.UseErrorPage();
            }
            else
            {   
                app.UseErrorHandler("/error");
            }

            if (env.IsProduction())
            {
                app.UseApplicationInsightsExceptionTelemetry();
            }

            app.UseStaticFiles();
            app.UseCookieAuthentication();
            app.UseOpenIdConnectAuthentication();

            app.Use((context, next) =>
            {
                if (context.Request.Path.StartsWithSegments("/ping"))
                {
                    return context.Response.WriteAsync("pong");
                }
                return next();
            });

            app.UseMvc();
        }
    }
}
