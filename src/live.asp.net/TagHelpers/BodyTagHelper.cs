using Microsoft.AspNet.Razor.Runtime.TagHelpers;

namespace live.asp.net.TagHelpers
{
    public class BodyTagHelper : TagHelper
    {
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            base.Process(context, output);

            if (!output.Content.GetContent().Contains("$(\"p[data-hidden-value]\"))")
                && output.Content.GetContent().Contains("click-to-show"))
            {
                const string script = @"<script>
        $(function () {
            $(""p[data - hidden - value]"")
                    .click(function() {
                    var $self = $(this),
                        state = $self.data(""state"") || ""hidden"";

                    if (state === ""hidden"")
                    {
                        $self.text($self.data(""hidden-value""));
                        $self.data(""state"", ""showing"");
                        $self.addClass(""click-to-show-revealed"");
                    }
                    else
                    {
                        $self.text(""click to show"");
                        $self.data(""state"", ""hidden"");
                        $self.removeClass(""click-to-show-revealed"");
                    }
                });
            });
    </script>";
                output.PostContent.Append(script);
            }
        }
    }
}