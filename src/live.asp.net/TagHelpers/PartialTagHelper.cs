// --------------------------------------------------------------------------------------------------------------------
// <copyright company=".NET Foundation" file="PartialTagHelper.cs">
//   Copyright (c) .NET Foundation. All rights reserved.
//   Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace live.asp.net.TagHelpers
{
    using System.Threading.Tasks;

    using Microsoft.AspNet.Mvc;
    using Microsoft.AspNet.Mvc.Razor;
    using Microsoft.AspNet.Mvc.Rendering;
    using Microsoft.AspNet.Razor.Runtime.TagHelpers;

    /// <summary>
    /// The partial tag helper class.
    /// </summary>
    [TargetElement("partial", Attributes = "name")]
    public class PartialTagHelper : TagHelper
    {
        /// <summary>
        /// The view engine
        /// </summary>
        private readonly ICompositeViewEngine viewEngine;

        /// <summary>
        /// Initializes a new instance of the <see cref="PartialTagHelper"/> class.
        /// </summary>
        /// <param name="viewEngine">
        /// The view engine.
        /// </param>
        public PartialTagHelper(ICompositeViewEngine viewEngine)
        {
            this.viewEngine = viewEngine;
        }

        /// <summary>
        /// Gets or sets the view context.
        /// </summary>
        /// <value>The view context.</value>
        [ViewContext]
        public ViewContext ViewContext { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the model.
        /// </summary>
        /// <value>The model.</value>
        public object Model { get; set; }

        /// <summary>
        /// Processes asynchronously.
        /// </summary>
        /// <param name="context">
        /// The <paramref name="context"/>.
        /// </param>
        /// <param name="output">
        /// The <paramref name="output"/>.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/>.
        /// </returns>
        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = null;

            var partialResult = this.viewEngine.FindPartialView(this.ViewContext, this.Name);

            if (partialResult != null && partialResult.Success)
            {
                var partialViewData = new ViewDataDictionary(this.ViewContext.ViewData, this.Model);
                var partialWriter = new TagHelperContentWrapperTextWriter(this.ViewContext.Writer.Encoding, output.Content);
                var partialViewContext = new ViewContext(this.ViewContext, partialResult.View, partialViewData, partialWriter);

                await partialResult.View.RenderAsync(partialViewContext);
            }
        }
    }
}
