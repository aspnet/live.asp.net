// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AzureStorageLiveShowDetailsService.cs" company=".NET Foundation">
//   Copyright (c) .NET Foundation. All rights reserved.
//   Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace live.asp.net.Services
{
    using System;
    using System.Threading.Tasks;

    using live.asp.net.Models;

    using Microsoft.ApplicationInsights;
    using Microsoft.Framework.Caching.Memory;
    using Microsoft.Framework.OptionsModel;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Blob;

    using Newtonsoft.Json;

    /// <summary>
    /// The azure storage live show details service.
    /// </summary>
    public class AzureStorageLiveShowDetailsService : ILiveShowDetailsService
    {
        #region Static Fields

        /// <summary>
        /// The <see cref="cache"/> key.
        /// </summary>
        private static readonly string CacheKey = nameof(AzureStorageLiveShowDetailsService);

        #endregion

        #region Fields

        /// <summary>
        /// The application settings.
        /// </summary>
        private readonly AppSettings appSettings;

        /// <summary>
        /// The cache.
        /// </summary>
        private readonly IMemoryCache cache;

        /// <summary>
        /// The telemtry.
        /// </summary>
        private readonly TelemetryClient telemtry;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureStorageLiveShowDetailsService"/> class.
        /// </summary>
        /// <param name="appSettings">
        /// The application settings.
        /// </param>
        /// <param name="cache">
        /// The cache.
        /// </param>
        /// <param name="telemetry">
        /// The telemetry.
        /// </param>
        public AzureStorageLiveShowDetailsService(IOptions<AppSettings> appSettings, IMemoryCache cache, TelemetryClient telemetry)
        {
            this.appSettings = appSettings.Options;
            this.cache = cache;
            this.telemtry = telemetry;
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        ///     Loads the live show details.
        /// </summary>
        /// <returns>
        ///     A <see cref="Task" /> of <see cref="LiveShowDetails" /> .
        /// </returns>
        public async Task<LiveShowDetails> LoadAsync()
        {
            var liveShowDetails = this.cache.Get<LiveShowDetails>(CacheKey);

            if (liveShowDetails == null)
            {
                liveShowDetails = await this.LoadFromAzureStorage();

                this.cache.Set(
                    CacheKey,
                    liveShowDetails,
                    new MemoryCacheEntryOptions { AbsoluteExpiration = DateTimeOffset.MaxValue });
            }

            return liveShowDetails;
        }

        /// <summary>
        /// Saves the live show details.
        /// </summary>
        /// <param name="liveShowDetails">
        /// The live show details.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> .
        /// </returns>
        public async Task SaveAsync(LiveShowDetails liveShowDetails)
        {
            if (liveShowDetails == null)
            {
                throw new ArgumentNullException(nameof(liveShowDetails));
            }

            await this.SaveToAzureStorage(liveShowDetails);

            // Update the cache
            this.cache.Set(CacheKey, liveShowDetails, new MemoryCacheEntryOptions { AbsoluteExpiration = DateTimeOffset.MaxValue });
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Gets the storage container.
        /// </summary>
        /// <returns>
        ///     A <see cref="CloudBlobContainer" /> .
        /// </returns>
        private CloudBlobContainer GetStorageContainer()
        {
            var account = CloudStorageAccount.Parse(this.appSettings.AzureStorageConnectionString);
            var blobClient = account.CreateCloudBlobClient();
            var container = blobClient.GetContainerReference(this.appSettings.AzureStorageContainerName);

            return container;
        }

        /// <summary>
        ///     Loads from azure storage.
        /// </summary>
        /// <returns>
        ///     A <see cref="Task" /> of <see cref="LiveShowDetails" /> .
        /// </returns>
        private async Task<LiveShowDetails> LoadFromAzureStorage()
        {
            var container = this.GetStorageContainer();

            if (!await container.ExistsAsync())
            {
                return null;
            }

            var blockBlob = container.GetBlockBlobReference(this.appSettings.AzureStorageBlobName);
            if (!await blockBlob.ExistsAsync())
            {
                return null;
            }

            var downloadStarted = DateTimeOffset.UtcNow;
            var fileContents = await blockBlob.DownloadTextAsync();
            this.telemtry.TrackDependency(
                "Azure.BlobStorage",
                "DownloadTextAsync",
                downloadStarted,
                DateTimeOffset.UtcNow - downloadStarted,
                true);

            return JsonConvert.DeserializeObject<LiveShowDetails>(fileContents);
        }

        /// <summary>
        /// Saves to Azure storage.
        /// </summary>
        /// <param name="liveShowDetails">
        /// The live show details.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> .
        /// </returns>
        private async Task SaveToAzureStorage(LiveShowDetails liveShowDetails)
        {
            var container = this.GetStorageContainer();

            await container.CreateIfNotExistsAsync();

            var blockBlob = container.GetBlockBlobReference(this.appSettings.AzureStorageBlobName);

            var fileContents = JsonConvert.SerializeObject(liveShowDetails);

            var uploadStarted = DateTimeOffset.UtcNow;
            await blockBlob.UploadTextAsync(fileContents);
            this.telemtry.TrackDependency(
                "Azure.BlobStorage",
                "UploadTextAsync",
                uploadStarted,
                DateTimeOffset.UtcNow - uploadStarted,
                true);
        }

        #endregion
    }
}
