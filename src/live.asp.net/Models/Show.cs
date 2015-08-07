// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Show.cs" company=".NET Foundation">
//   Copyright (c) .NET Foundation. All rights reserved.
//   Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace live.asp.net.Models
{
    using System;

    /// <summary>
    /// The show.
    /// </summary>
    public class Show
    {
        #region Public Properties

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>The description.</value>
        public string Description { get; set; }

        /// <summary>
        /// The has title.
        /// </summary>
        /// <value>The has title.</value>
        public bool HasTitle => !string.IsNullOrEmpty(this.Title);

        /// <summary>
        /// Gets or sets the id.
        /// </summary>
        /// <value>The identifier.</value>
        public int Id { get; set; }

        /// <summary>
        /// The is in future.
        /// </summary>
        /// <value>The is in future.</value>
        public bool IsInFuture => this.ShowDate > DateTimeOffset.Now;

        /// <summary>
        /// The is new.
        /// </summary>
        /// <value>The is new.</value>
        public bool IsNew => !this.IsInFuture && (DateTimeOffset.Now - this.ShowDate).TotalDays <= 7;

        /// <summary>
        /// Gets or sets the provider.
        /// </summary>
        /// <value>The provider.</value>
        public string Provider { get; set; }

        /// <summary>
        /// Gets or sets the provider id.
        /// </summary>
        /// <value>The provider identifier.</value>
        public string ProviderId { get; set; }

        /// <summary>
        /// Gets or sets the show date.
        /// </summary>
        /// <value>The show date.</value>
        public DateTimeOffset ShowDate { get; set; }

        /// <summary>
        /// Gets or sets the thumbnail URL.
        /// </summary>
        /// <value>The thumbnail URL.</value>
        public string ThumbnailUrl { get; set; }

        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        /// <value>The title.</value>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the URL.
        /// </summary>
        /// <value>The URL.</value>
        public string Url { get; set; }

        #endregion
    }
}
