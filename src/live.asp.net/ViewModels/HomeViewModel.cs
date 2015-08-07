// --------------------------------------------------------------------------------------------------------------------
// <copyright file="HomeViewModel.cs" company=".NET Foundation">
//   Copyright (c) .NET Foundation. All rights reserved.
//   Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace live.asp.net.ViewModels
{
    using System;
    using System.Collections.Generic;

    using live.asp.net.Models;

    /// <summary>
    ///     Class HomeViewModel.
    /// </summary>
    public class HomeViewModel
    {
        #region Public Properties

        /// <summary>
        ///     Gets or sets the administration message.
        /// </summary>
        /// <value>
        ///     The administration message.
        /// </value>
        public string AdminMessage { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether we have an administration message.
        /// </summary>
        /// <value>
        ///     A value indicating whether we have an administration message.
        /// </value>
        public bool HasAdminMessage => !string.IsNullOrEmpty(this.AdminMessage);

        /// <summary>
        ///     Gets or sets a value indicating whether we are on air.
        /// </summary>
        /// <value>
        ///     A value indicating whether we are on air.
        /// </value>
        public bool IsOnAir => !this.HasAdminMessage && !string.IsNullOrEmpty(this.LiveShowEmbedUrl);

        /// <summary>
        ///     Gets or sets the live show embed URL.
        /// </summary>
        /// <value>
        ///     The live show embed URL.
        /// </value>
        public string LiveShowEmbedUrl { get; set; }

        /// <summary>
        ///     Gets or sets the more shows URL.
        /// </summary>
        /// <value>
        ///     The more shows URL.
        /// </value>
        public string MoreShowsUrl { get; set; }

        /// <summary>
        ///     Gets or sets the next show date UTC.
        /// </summary>
        /// <value>
        ///     The next show date UTC.
        /// </value>
        public DateTime? NextShowDateUtc { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether the next show has been scheduled.
        /// </summary>
        /// <value>
        ///     A value indicating whether the next show has been scheduled.
        /// </value>
        public bool NextShowScheduled => this.NextShowDateUtc.HasValue;

        /// <summary>
        ///     Gets or sets the previous shows.
        /// </summary>
        /// <value>
        ///     The previous shows.
        /// </value>
        public IList<Show> PreviousShows { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether to show the more shows URL.
        /// </summary>
        /// <value>
        ///     A value indicating whether to show the more shows URL.
        /// </value>
        public bool ShowMoreShowsUrl => !string.IsNullOrEmpty(this.MoreShowsUrl);

        /// <summary>
        ///     Gets or sets the show previous shows.
        /// </summary>
        /// <value>
        ///     The show previous shows.
        /// </value>
        public bool ShowPreviousShows => this.PreviousShows.Count > 0;

        #endregion
    }
}
