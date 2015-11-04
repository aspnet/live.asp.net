using Microsoft.AspNet.Razor.Runtime.TagHelpers;
using Microsoft.Dnx.Runtime;

namespace live.asp.net.TagHelpers
{
    [HtmlTargetElement("framework-info", TagStructure = TagStructure.WithoutEndTag)]
    public class FrameworkInformationTagHelper : TagHelper
    {
        private readonly IApplicationEnvironment ApplicationEnvironment;

        public string Label { get; set; } = "Framework";
        public FrameworkInformationTagHelper(IApplicationEnvironment app)
        {
            ApplicationEnvironment = app;
        }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            if (string.IsNullOrWhiteSpace(Label))
                output.Content.SetContent(ApplicationEnvironment.RuntimeFramework.ToString());
            else
                output.Content.SetContent($"{Label}: {ApplicationEnvironment.RuntimeFramework.ToString()}");
            output.TagName = null;
        }
    }
}
