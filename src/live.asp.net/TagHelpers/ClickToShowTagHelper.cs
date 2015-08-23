// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNet.Mvc.Rendering;
using Microsoft.AspNet.Mvc.TagHelpers;
using Microsoft.AspNet.Razor.Runtime.TagHelpers;

namespace live.asp.net.TagHelpers
{
    [TargetElement("click-to-show", Attributes = "value")]
    public class ClickToShowTagHelper : TagHelper
    {
        public string Value { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = "p";

            var p = new TagBuilder("p");
            p.AddCssClass("click-to-show");
            p.MergeAttribute("data-hidden-value", Value);

            output.MergeAttributes(p);
            output.Content.SetContent("click to show");
        }
    }
}