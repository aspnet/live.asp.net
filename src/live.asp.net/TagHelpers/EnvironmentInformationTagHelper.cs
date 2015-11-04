using Microsoft.AspNet.Hosting;
using Microsoft.AspNet.Razor.Runtime.TagHelpers;

namespace live.asp.net.TagHelpers
{
    [HtmlTargetElement("environment-info", TagStructure = TagStructure.WithoutEndTag)]
    public class EnvironmentInformationTagHelper : TagHelper
    {
        private readonly IHostingEnvironment HostingEnvironment;

        public string Label { get; set; } = "Environment";
        public EnvironmentInformationTagHelper(IHostingEnvironment env)
        {
            HostingEnvironment = env;
        }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            if (string.IsNullOrWhiteSpace(Label))
                output.Content.SetContent(HostingEnvironment.EnvironmentName);
            else
                output.Content.SetContent($"{Label}: {HostingEnvironment.EnvironmentName}");
            output.TagName = null;
        }
    }
}
