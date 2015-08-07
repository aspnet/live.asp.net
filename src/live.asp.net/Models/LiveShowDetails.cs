// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LiveShowDetails.cs" company=".NET Foundation">
//   Copyright (c) .NET Foundation. All rights reserved.
//   Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace live.asp.net.Models
{
    using System;

    /// <summary>
    /// The live show details.
    /// </summary>
    public class LiveShowDetails
    {
        #region Public Properties

        /// <summary>
        /// Gets or sets the admin message.
        /// </summary>
        public string AdminMessage { get; set; }

        /// <summary>
        /// Gets or sets the live show embed URL.
        /// </summary>
        public string LiveShowEmbedUrl { get; set; }

        /// <summary>
        /// Gets or sets the next show date UTC.
        /// </summary>
        public DateTime? NextShowDateUtc { get; set; }

        #endregion
    }
}