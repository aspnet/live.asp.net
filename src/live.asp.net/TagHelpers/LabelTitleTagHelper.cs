// Copyright (c) .NET Foundation. All rights reserved. 
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace live.asp.net.TagHelpers
{
    [HtmlTargetElement("label", Attributes = "asp-for")]
    public class LabelTitleTagHelper : TagHelper
    {
        [HtmlAttributeName("asp-for")]
        public ModelExpression For { get; set; } = default!;

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            var description = For.Metadata.Description;
            if (!string.IsNullOrEmpty(description) && !output.Attributes.ContainsName("title"))
            {
                output.Attributes.Add(new TagHelperAttribute("title", description));
            }
        }
    }
}
