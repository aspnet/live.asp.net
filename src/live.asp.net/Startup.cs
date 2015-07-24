using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Diagnostics;
using Microsoft.AspNet.Hosting;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Routing;
using Microsoft.AspNet.Authentication;
using Microsoft.AspNet.Authentication.Cookies;
using Microsoft.AspNet.Authentication.OpenIdConnect;
using Microsoft.Framework.Configuration;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.Logging;
using Microsoft.Framework.Logging.Console;
using Microsoft.Framework.Runtime;
using live.asp.net.Services;

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
            }
            builder.AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfiguration Configuration { get; set; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<CookieAuthenticationOptions>(options =>
            {
                options.AutomaticAuthentication = true;
            });

            services.Configure<OpenIdConnectAuthenticationOptions>(options =>
            {
                options.AutomaticAuthentication = true;
                options.ClientId = Configuration["Authentication:AzureAd:ClientId"];
                options.Authority = Configuration["Authentication:AzureAd:AADInstance"] + Configuration["Authentication:AzureAd:TenantId"];
                options.PostLogoutRedirectUri = Configuration["Authentication:AzureAd:PostLogoutRedirectUri"];
                options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            });

            services.AddMvc();

            if (_env.IsDevelopment())
            {
                services.AddSingleton<IShowsService, DevShowsService>();
            }
            else
            {
                //services.AddSingleton<IShowsService, YouTubeShowsService>();
            }
        }
        
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.MinimumLevel = LogLevel.Warning;
            loggerFactory.AddConsole();

            if (env.IsDevelopment())
            {
                app.UseErrorPage(ErrorPageOptions.ShowAll);
            }
            else
            {
                app.UseErrorHandler("/Home/Error");
            }

            app.UseStaticFiles();
            app.UseCookieAuthentication();
            app.UseOpenIdConnectAuthentication();
            
            app.UseMvc();
        }
    }
}
