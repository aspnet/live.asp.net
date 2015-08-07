// --------------------------------------------------------------------------------------------------------------------
// <copyright company=".NET Foundation" file="ScriptInliningTagHelper.cs">
//   Copyright (c) .NET Foundation. All rights reserved.
//   Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace live.asp.net.TagHelpers
{
    using System;
    using System.Diagnostics.Contracts;
    using System.IO;
    using System.Text;

    using Microsoft.AspNet.FileProviders;
    using Microsoft.AspNet.Hosting;
    using Microsoft.AspNet.Mvc;
    using Microsoft.AspNet.Razor.Runtime.TagHelpers;

    /// <summary>
    /// A tag helper that in-lines script
    /// </summary>
    [TargetElement("script", Attributes = "inline")]
    public class ScriptInliningTagHelper : TagHelper
    {
        /// <summary>
        /// The wwwroot
        /// </summary>
        private readonly IFileProvider wwwroot;

        /// <summary>
        /// Initializes a new instance of the <see cref="ScriptInliningTagHelper"/> class.
        /// </summary>
        /// <param name="env">
        /// The env.
        /// </param>
        public ScriptInliningTagHelper(IHostingEnvironment env)
        {
            this.wwwroot = env.WebRootFileProvider;
        }

        /// <summary>
        /// Gets or sets the view context.
        /// </summary>
        /// <value>The view context.</value>
        [ViewContext]
        public ViewContext ViewContext { get; set; }

        /// <summary>
        /// Gets or sets the in-line.
        /// </summary>
        /// <value>The in-line.</value>
        public bool Inline { get; set; }

        /// <summary>
        /// Processes the specified <paramref name="context"/>.
        /// </summary>
        /// <param name="context">
        /// The context.
        /// </param>
        /// <param name="output">
        /// The output.
        /// </param>
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            Contract.Requires(context != null);
            Contract.Requires(output != null);

            if (!this.Inline)
            {
                return;
            }

            var src = output.Attributes["src"];

            var path = src?.Value.ToString();
            if (path == null)
            {
                return;
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

            var fileInfo = this.wwwroot.GetFileInfo(resolvedPath);
            var requestPathBase = this.ViewContext.HttpContext.Request.PathBase;
            if (!fileInfo.Exists)
            {
                if (requestPathBase.HasValue &&
                    resolvedPath.StartsWith(requestPathBase.Value, StringComparison.OrdinalIgnoreCase))
                {
                    resolvedPath = resolvedPath.Substring(requestPathBase.Value.Length);
                    fileInfo = this.wwwroot.GetFileInfo(resolvedPath);
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
                output.Content.Append(reader.ReadToEnd());
            }

            output.Attributes.Remove(src);
        }
    }
}
