// Copyright (c) .NET Foundation. All rights reserved. 
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Security.Claims;
using live.asp.net.Formatters;
using live.asp.net.Services;
using Microsoft.AspNet.Authentication.Cookies;
using Microsoft.AspNet.Authentication.OpenIdConnect;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Hosting;
using Microsoft.AspNet.Http;
using Microsoft.Dnx.Runtime;
using Microsoft.Framework.Configuration;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.Logging;

namespace live.asp.net
{
    public class Startup
    {
        private readonly IHostingEnvironment _env;

        public Startup(IHostingEnvironment env, IApplicationEnvironment appEnv)
        {
            _env = env;

            var builder = new ConfigurationBuilder()
                .SetBasePath(appEnv.ApplicationBasePath)
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
            services.Configure<AppSettings>(Configuration.GetSection("AppSettings"));

            services.AddAuthorization(options =>
            {
                options.AddPolicy("Admin", policyBuilder =>
                {
                    policyBuilder.RequireClaim(
                        ClaimTypes.Name,
                        Configuration["Authorization:AdminUsers"].Split(',')
                    );
                });
            });

            services.AddApplicationInsightsTelemetry(Configuration);

            services.AddMvc(options =>
            {
                options.OutputFormatters.Add(new iCalendarOutputFormatter());
            });

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
            loggerFactory.AddDebug();

            if (env.IsProduction())
            {
                app.UseApplicationInsightsRequestTelemetry();
            }

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {   
                app.UseExceptionHandler("/error");
            }

            if (env.IsProduction())
            {
                app.UseApplicationInsightsExceptionTelemetry();
            }

            app.UseIISPlatformHandler();

            app.UseStaticFiles();

            app.UseCookieAuthentication(options =>
            {
                options.AutomaticAuthentication = true;
            });

            app.UseOpenIdConnectAuthentication(options =>
            {
                options.DefaultToCurrentUriOnRedirect = true;
                options.AutomaticAuthentication = true;
                options.ClientId = Configuration["Authentication:AzureAd:ClientId"];
                options.Authority = Configuration["Authentication:AzureAd:AADInstance"] + Configuration["Authentication:AzureAd:TenantId"];
                options.PostLogoutRedirectUri = Configuration["Authentication:AzureAd:PostLogoutRedirectUri"];
                options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            });

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
