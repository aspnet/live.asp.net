using Microsoft.ApplicationInsights.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Threading.Tasks;

namespace live.asp.net.TagHelpers
{
    public class AppInsightsTagHelperComponent : TagHelperComponent
    {
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly HttpContext _httpContext;
        private readonly string _script;

        public AppInsightsTagHelperComponent(
            IHostingEnvironment hostingEnvironment,
            IHttpContextAccessor httpContextAccessor,
            JavaScriptSnippet appInsightsJs)
        {
            _hostingEnvironment = hostingEnvironment;
            _httpContext = httpContextAccessor.HttpContext;
            _script = appInsightsJs.FullScript;
        }

        public override int Order => 2;


        public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            if (context.TagName == "head")
            {
                if (_hostingEnvironment.EnvironmentName == EnvironmentName.Development)
                {
                    output.PostContent.AppendHtml(_script);

                    if (_httpContext.User.Identity.IsAuthenticated)
                    {
                        output.PostContent.AppendHtml(string.Format(Resources.AppInsightsAuth,
                            _httpContext.User.Identity.Name.Replace("\\", "\\\\")));
                    }
                }
            }

            return Task.CompletedTask;
        }
    }
}
