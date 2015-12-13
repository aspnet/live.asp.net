// Copyright (c) .NET Foundation. All rights reserved. 
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.ObjectModel;
using System.Security.Claims;
using live.asp.net.Formatters;
using live.asp.net.Services;
using live.asp.net.ViewModels;
using Microsoft.AspNet.Authentication.Cookies;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Hosting;
using Microsoft.AspNet.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.PlatformAbstractions;

namespace live.asp.net
{
    public class Startup
    {
        private readonly IHostingEnvironment _env;

        public static void Main(string[] args) => WebApplication.Run<Startup>(args);

        public Startup(IHostingEnvironment env, IApplicationEnvironment appEnv)
        {
            _env = env;

            var builder = new ConfigurationBuilder()
                .SetBasePath(appEnv.ApplicationBasePath)
                .AddJsonFile("appsettings.json")
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true);

            if (_env.IsDevelopment())
            {
                builder.AddUserSecrets();
                builder.AddApplicationInsightsSettings(developerMode: true);
            }

            builder.AddEnvironmentVariables();

            Configuration = builder.Build();
        }

        public IConfiguration Configuration { get; set; }
        //Move Later
       

        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<AppSettings>(Configuration.GetSection("AppSettings"));
            services.AddSingleton<IOnAir, OnAir>();

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
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
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
                options.AutomaticAuthenticate = true;
            });

            app.UseOpenIdConnectAuthentication(options =>
            {
                options.AutomaticAuthenticate = true;
                options.AutomaticChallenge = true;
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
