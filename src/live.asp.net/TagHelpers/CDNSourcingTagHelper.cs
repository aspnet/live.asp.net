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
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Mvc.TagHelpers.Internal;
using Microsoft.Extensions.Caching.Memory;

namespace live.asp.net.TagHelpers
{
	[HtmlTargetElement("link", Attributes = "[rel='stylesheet'],fromCDN")]
	[HtmlTargetElement("image", Attributes = "fromCDN")]
    public class CDNSourcingTagHelper : TagHelper
    {
		private readonly AppSettings _appSettings;
		private const string FROMCDNAttributeName = "fromCDN";

		public CDNSourcingTagHelper(IOptions<AppSettings> appSettings)
        {
			_appSettings = appSettings.Value;
		}

        [ViewContext]
        public ViewContext ViewContext { get; set; }

		[HtmlAttributeName(FROMCDNAttributeName)]
        public bool FromCDN { get; set; }

		public override void Process(TagHelperContext context, TagHelperOutput output)
        {
			base.Process(context, output);
			if (!FromCDN)
            {
                return;
            }

			var src = output.Attributes["src"];
			var href = output.Attributes["xlink:href"] ?? output.Attributes["href"] ;

			AddCDNPrefix(src, output);
			AddCDNPrefix(href, output);
		}

		private void AddCDNPrefix(TagHelperAttribute attribute, TagHelperOutput output)
		{
			if (attribute == null)
			{
				return;
			}
			
			var path = default(string);
			switch (attribute.Value)
			{
				case string p:
					path = p;
					break;
				case HtmlString s when s.Value != null:
					path = s.Value;
					break;
				case IHtmlContent pathHtmlContent:
					using (var tw = new StringWriter())
					{
						pathHtmlContent.WriteTo(tw, NullHtmlEncoder.Default);
						path = tw.ToString();
					}
					break;
				default:
					path = attribute?.Value?.ToString();
					break;
			}
			var resolvedPath = path ?? attribute.Value.ToString();

			var queryStringStartIndex = resolvedPath.IndexOf('?');
			if (queryStringStartIndex != -1)
			{
				if (Uri.TryCreate(resolvedPath.Substring(0, queryStringStartIndex), UriKind.Absolute, out Uri uri))
				{
					// Don't update if the path is absolute
					return;
				}
			}			
			
			output.Attributes.SetAttribute(attribute.Name, string.Format("{0}{1}", _appSettings.CDNUrl, resolvedPath));

		}

		
	}	
}
