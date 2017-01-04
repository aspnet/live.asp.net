// Copyright (c) .NET Foundation. All rights reserved. 
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Text;
using Microsoft.Extensions.FileProviders;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.AspNetCore.Html;

namespace live.asp.net.TagHelpers
{
    [HtmlTargetElement("script", Attributes = "inline")]
    public class ScriptInliningTagHelper : TagHelper
    {
        private readonly IFileProvider _wwwroot;

        public ScriptInliningTagHelper(IHostingEnvironment env)
        {
            _wwwroot = env.WebRootFileProvider;
        }

        [ViewContext]
        public ViewContext ViewContext { get; set; }

        public bool Inline { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            if (!Inline)
            {
                return;
            }

            var src = output.Attributes["src"];

            if (src == null)
            {
                return;
            }

            string path = null;
            if (src.Value is string)
            {
                path = (string)src.Value;
            }
            else
            {
                var pathHtmlString = src.Value as HtmlString;
                if (pathHtmlString != null)
                {
                    path = pathHtmlString.Value;
                }
                else
                {
                    var pathHtmlContent = src.Value as IHtmlContent;
                    if (pathHtmlContent != null)
                    {
                        using (var tw = new StringWriter())
                        {
                            pathHtmlContent.WriteTo(tw, NullHtmlEncoder.Default);
                            path = tw.ToString();
                        }
                    }
                    else
                    {
                        path = src.Value.ToString();
                    }
                }
            }

            var resolvedPath = path;
            var queryStringStartIndex = path.IndexOf('?');
            if (queryStringStartIndex != -1)
            {
                resolvedPath = path.Substring(0, queryStringStartIndex);
            }

            Uri uri;
            if (Uri.TryCreate(resolvedPath, UriKind.Absolute, out uri))
            {
                // Don't inline if the path is absolute
                return;
            }

            var fileInfo = _wwwroot.GetFileInfo(resolvedPath);
            var requestPathBase = ViewContext.HttpContext.Request.PathBase;
            if (!fileInfo.Exists)
            {
                if (requestPathBase.HasValue &&
                    resolvedPath.StartsWith(requestPathBase.Value, StringComparison.OrdinalIgnoreCase))
                {
                    resolvedPath = resolvedPath.Substring(requestPathBase.Value.Length);
                    fileInfo = _wwwroot.GetFileInfo(resolvedPath);
                }

                if (!fileInfo.Exists)
                {
                    // Don't inline if the file is not on the current server
                    return;
                }
            }

            using (var readStream = fileInfo.CreateReadStream())
            using (var reader = new StreamReader(readStream, Encoding.UTF8))
            {
                output.Content.AppendHtml(reader.ReadToEnd());
            }

            output.Attributes.Remove(src);
        }
    }
}
