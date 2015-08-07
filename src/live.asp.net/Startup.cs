// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Startup.cs" company=".NET Foundation">
//   Copyright (c) .NET Foundation. All rights reserved.
//   Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace live.asp.net
{
    using System.Diagnostics.CodeAnalysis;

    using live.asp.net.Formatters;
    using live.asp.net.Services;

    using Microsoft.AspNet.Authentication.Cookies;
    using Microsoft.AspNet.Authentication.OpenIdConnect;
    using Microsoft.AspNet.Builder;
    using Microsoft.AspNet.Hosting;
    using Microsoft.Framework.Configuration;
    using Microsoft.Framework.DependencyInjection;
    using Microsoft.Framework.Logging;
    using Microsoft.Framework.Runtime;

    /// <summary>
    /// The startup class.
    /// </summary>
    public class Startup
    {
        #region Fields

        /// <summary>
        /// The environment.
        /// </summary>
        private readonly IHostingEnvironment env;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="Startup"/> class.
        /// </summary>
        /// <param name="env">
        /// The environment.
        /// </param>
        /// <param name="appEnv">
        /// The application environment.
        /// </param>
        public Startup(IHostingEnvironment env, IApplicationEnvironment appEnv)
        {
            this.env = env;

            var builder = new ConfigurationBuilder(appEnv.ApplicationBasePath).AddJsonFile("config.json").AddJsonFile($"config.{env.EnvironmentName}.json", true);

            if (this.env.IsDevelopment())
            {
                builder.AddUserSecrets();
                builder.AddApplicationInsightsSettings(true);
            }

            builder.AddEnvironmentVariables();

            this.Configuration = builder.Build();
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the configuration.
        /// </summary>
        /// <value>The configuration.</value>
        public IConfiguration Configuration { get; set; }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// Configures the application.
        /// </summary>
        /// <param name="app">
        /// The application.
        /// </param>
        /// <param name="hostingEnvironment">
        /// The environment.
        /// </param>
        /// <param name="loggerFactory">
        /// The logger factory.
        /// </param>
        [SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "ASP.NET application")]
        public void Configure(IApplicationBuilder app, IHostingEnvironment hostingEnvironment, ILoggerFactory loggerFactory)
        {
            loggerFactory.MinimumLevel = LogLevel.Warning;
            loggerFactory.AddConsole();

            if (hostingEnvironment.IsProduction())
            {
                app.UseApplicationInsightsRequestTelemetry();
            }

            if (hostingEnvironment.IsDevelopment())
            {
                app.UseErrorPage();
            }
            else
            {
                app.UseErrorHandler("/error");
            }

            if (hostingEnvironment.IsProduction())
            {
                app.UseApplicationInsightsExceptionTelemetry();
            }

            app.UseStaticFiles();
            app.UseCookieAuthentication();
            app.UseOpenIdConnectAuthentication();

            app.UseMvc();
        }

        /// <summary>
        /// Configures the <paramref name="services"/>.
        /// </summary>
        /// <param name="services">
        /// The services.
        /// </param>
        [SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "ASP.NET application")]
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<AppSettings>(this.Configuration.GetConfigurationSection(nameof(AppSettings)), null);

            services.Configure<CookieAuthenticationOptions>(options => { options.AutomaticAuthentication = true; });

            services.Configure<OpenIdConnectAuthenticationOptions>(
                options =>
                    {
                        options.DefaultToCurrentUriOnRedirect = true;
                        options.AutomaticAuthentication = true;
                        options.ClientId = this.Configuration["Authentication:AzureAd:ClientId"];
                        options.Authority = this.Configuration["Authentication:AzureAd:AADInstance"] + this.Configuration["Authentication:AzureAd:TenantId"];
                        options.PostLogoutRedirectUri = this.Configuration["Authentication:AzureAd:PostLogoutRedirectUri"];
                        options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    });

            services.ConfigureAuthorization(
                options =>
                    {
                        options.AddPolicy(
                            "Admin",
                            policyBuilder =>
                                {
                                    policyBuilder.RequireClaim(
                                        "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name",
                                        this.Configuration["Authorization:AdminUsers"].Split(','));
                                });
                    });

            services.AddApplicationInsightsTelemetry(this.Configuration);

            services.ConfigureMvc(mvc => { mvc.OutputFormatters.Add(new iCalendarOutputFormatter()); });
            services.AddMvc();

            services.AddScoped<IShowsService, YouTubeShowsService>();

            if (string.IsNullOrEmpty(this.Configuration["AppSettings:AzureStorageConnectionString"]))
            {
                services.AddSingleton<ILiveShowDetailsService, FileSystemLiveShowDetailsService>();
            }
            else
            {
                services.AddSingleton<ILiveShowDetailsService, AzureStorageLiveShowDetailsService>();
            }
        }

        #endregion
    }
}
