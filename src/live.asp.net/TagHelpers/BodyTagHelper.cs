// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNet.Razor.Runtime.TagHelpers;

namespace live.asp.net.TagHelpers
{
    public class BodyTagHelper : TagHelper
    {
        private static readonly Assembly Assembly = typeof (BodyTagHelper).Assembly;

        private readonly string ClickToShowJavaScriptResourceName = "live.asp.net.Compiler.Resources.ClickToShowTagHelper.js";

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            var bodyContent = await context.GetChildContentAsync();

            if (bodyContent.Contains("data-hidden-value"))
            {
                var clickToShowJavaScript = string.Empty;
                using (var resourceStream = Assembly.GetManifestResourceStream(ClickToShowJavaScriptResourceName))
                {
                    if (resourceStream != null)
                    {
                        using (var streamReader = new StreamReader(resourceStream))
                        {
                            clickToShowJavaScript = streamReader.ReadToEnd();
                        }
                    } 
                }

                output.PostContent.Append("<script>")
                        .Append(clickToShowJavaScript)
                        .Append("</script>");
            }
        }
    }
}