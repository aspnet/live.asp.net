// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AppSettings.cs" company=".NET Foundation">
//   Copyright (c) .NET Foundation. All rights reserved.
//   Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace live.asp.net
{
    /// <summary>
    ///     The application settings class.
    /// </summary>
    public class AppSettings
    {
        #region Public Properties

        /// <summary>
        ///     Gets or sets the name of the Azure storage BLOB.
        /// </summary>
        /// <value>
        ///     The name of the Azure storage BLOB.
        /// </value>
        public string AzureStorageBlobName { get; set; }

        /// <summary>
        ///     Gets or sets the Azure storage connection string.
        /// </summary>
        /// <value>
        ///     The Azure storage connection string.
        /// </value>
        public string AzureStorageConnectionString { get; set; }

        /// <summary>
        ///     Gets or sets the name of the Azure storage container.
        /// </summary>
        /// <value>
        ///     The name of the Azure storage container.
        /// </value>
        public string AzureStorageContainerName { get; set; }

        /// <summary>
        ///     Gets or sets YouTube API key.
        /// </summary>
        /// <value>
        ///     YouTube API key.
        /// </value>
        public string YouTubeApiKey { get; set; }

        /// <summary>
        ///     Gets or sets the name of YouTube application.
        /// </summary>
        /// <value>
        ///     The name of YouTube application.
        /// </value>
        public string YouTubeApplicationName { get; set; }

        /// <summary>
        ///     Gets or sets YouTube play list identifier.
        /// </summary>
        /// <value>
        ///     YouTube play list identifier.
        /// </value>
        public string YouTubePlaylistId { get; set; }

        #endregion
    }
}