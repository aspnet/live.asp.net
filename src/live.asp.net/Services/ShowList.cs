// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ShowList.cs" company=".NET Foundation">
//   Copyright (c) .NET Foundation. All rights reserved.
//   Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace live.asp.net.Services
{
    using System.Collections.Generic;

    using live.asp.net.Models;

    /// <summary>
    ///     The show list class.
    /// </summary>
    public class ShowList
    {
        #region Public Properties

        /// <summary>
        ///     Gets or sets the more shows URL.
        /// </summary>
        /// <value>
        ///     The more shows URL.
        /// </value>
        public string MoreShowsUrl { get; set; }

        /// <summary>
        ///     Gets or sets the shows.
        /// </summary>
        /// <value>
        ///     The shows.
        /// </value>
        public IList<Show> Shows { get; set; }

        #endregion
    }
}