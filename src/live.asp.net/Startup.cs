// Copyright (c) .NET Foundation. All rights reserved. 
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Security.Claims;
using live.asp.net.Formatters;
using live.asp.net.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace live.asp.net
{
    public class Startup
    {
        private readonly IHostingEnvironment _env;

        public Startup(IHostingEnvironment env)
        {
            _env = env;

            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
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

        public IConfigurationRoot Configuration { get; set; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<AppSettings>(options => Configuration.GetSection("AppSettings").Bind(options));

            // Configure header fowarding to only include the protocol due to behavior of ARR in Azure Web Sites
            // See: https://github.com/aspnet/IISIntegration/issues/140#issuecomment-215135928
            services.Configure<ForwardedHeadersOptions>(options => options.ForwardedHeaders = ForwardedHeaders.XForwardedProto);

            services.AddAuthentication(SharedOptions => SharedOptions.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme);

            services.AddAuthorization(options =>
                options.AddPolicy("Admin", policyBuilder =>
                    policyBuilder.RequireClaim(
                        ClaimTypes.Name,
                        Configuration["Authorization:AdminUsers"].Split(',')
                    )
                )
            );

            services.AddApplicationInsightsTelemetry(Configuration);

            services.AddMvc(options => options.OutputFormatters.Add(new iCalendarOutputFormatter()));

            services.AddScoped<IShowsService, YouTubeShowsService>();

            services.AddSingleton<IDeploymentEnvironment, DeploymentEnvironment>();

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

            app.UseStaticFiles();

            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AutomaticAuthenticate = true
            });

            app.UseOpenIdConnectAuthentication(new OpenIdConnectOptions
            {
                AutomaticAuthenticate = true,
                AutomaticChallenge = true,
                ClientId = Configuration["Authentication:AzureAd:ClientId"],
                Authority = Configuration["Authentication:AzureAd:AADInstance"] + Configuration["Authentication:AzureAd:TenantId"],
                ResponseType = OpenIdConnectResponseTypes.IdToken,
                SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme
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
