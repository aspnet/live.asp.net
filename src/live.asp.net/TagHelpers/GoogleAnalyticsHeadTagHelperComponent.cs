// Copyright (c) .NET Foundation. All rights reserved. 
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Threading.Tasks;

namespace live.asp.net.TagHelpers
{
    public class GoogleAnalyticsHeadTagHelperComponent : TagHelperComponent
    {
        private readonly IHostingEnvironment _hostingEnvironment;

        public GoogleAnalyticsHeadTagHelperComponent(IHostingEnvironment hostingEnvironment)
        {
            _hostingEnvironment = hostingEnvironment;
        }

        public override int Order => 1;

        public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            if (context.TagName == "head")
            {
                if (_hostingEnvironment.EnvironmentName == EnvironmentName.Development)
                {
                    output.PostContent.AppendHtml(Resources.GoogleAnalyticsHead);
                }            
            }

            return Task.FromResult(0);
        }
    }
}
