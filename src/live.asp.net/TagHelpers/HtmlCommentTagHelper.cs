using Microsoft.AspNet.Razor.Runtime.TagHelpers;

namespace live.asp.net.TagHelpers
{
    public class HtmlCommentTagHelper : TagHelper
    {
        private const string StartHtmlComment = "<!-- ";
        private const string EndHtmlComment = " -->";

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.PreContent.AppendEncoded(StartHtmlComment);
            output.PostContent.AppendEncoded(EndHtmlComment);
            output.TagName = null;
        }
    }
}
