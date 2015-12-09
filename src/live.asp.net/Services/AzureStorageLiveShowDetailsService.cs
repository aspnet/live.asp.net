// Copyright (c) .NET Foundation. All rights reserved. 
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using live.asp.net.Models;
using Microsoft.ApplicationInsights;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.OptionsModel;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;

namespace live.asp.net.Services
{
    public class AzureStorageLiveShowDetailsService : ILiveShowDetailsService
    {
        private static readonly string CacheKey = nameof(AzureStorageLiveShowDetailsService);

        private readonly AppSettings _appSettings;
        private readonly IMemoryCache _cache;
        private readonly TelemetryClient _telemtry;

        public AzureStorageLiveShowDetailsService(
            IOptions<AppSettings> appSettings,
            IMemoryCache cache,
            TelemetryClient telemetry)
        {
            _appSettings = appSettings.Value;
            _cache = cache;
            _telemtry = telemetry;
        }

        public async Task<LiveShowDetails> LoadAsync()
        {
            var liveShowDetails = _cache.Get<LiveShowDetails>(CacheKey);

            if (liveShowDetails == null)
            {
                liveShowDetails = await LoadFromAzureStorage();

                _cache.Set(CacheKey, liveShowDetails, new MemoryCacheEntryOptions
                {
                    AbsoluteExpiration = DateTimeOffset.MaxValue
                });
            }

            return liveShowDetails;
        }

        public async Task SaveAsync(LiveShowDetails liveShowDetails)
        {
            if (liveShowDetails == null)
            {
                throw new ArgumentNullException(nameof(liveShowDetails));
            }

            await SaveToAzureStorage(liveShowDetails);

            // Update the cache
            _cache.Set(CacheKey, liveShowDetails, new MemoryCacheEntryOptions
            {
                AbsoluteExpiration = DateTimeOffset.MaxValue
            });
        }

        private async Task<LiveShowDetails> LoadFromAzureStorage()
        {
            var container = GetStorageContainer();

            if (!await container.ExistsAsync())
            {
                return null;
            }

            var blockBlob = container.GetBlockBlobReference(_appSettings.AzureStorageBlobName);
            if (!await blockBlob.ExistsAsync())
            {
                return null;
            }

            var downloadStarted = DateTimeOffset.UtcNow;
            var fileContents = await blockBlob.DownloadTextAsync();
            _telemtry.TrackDependency("Azure.BlobStorage", "DownloadTextAsync", downloadStarted, DateTimeOffset.UtcNow - downloadStarted, true);

            return JsonConvert.DeserializeObject<LiveShowDetails>(fileContents);
        }

        private async Task SaveToAzureStorage(LiveShowDetails liveShowDetails)
        {
            var container = GetStorageContainer();

            await container.CreateIfNotExistsAsync();

            var blockBlob = container.GetBlockBlobReference(_appSettings.AzureStorageBlobName);

            var fileContents = JsonConvert.SerializeObject(liveShowDetails);

            var uploadStarted = DateTimeOffset.UtcNow;
            await blockBlob.UploadTextAsync(fileContents);
            _telemtry.TrackDependency("Azure.BlobStorage", "UploadTextAsync", uploadStarted, DateTimeOffset.UtcNow - uploadStarted, true);
        }
        
        private CloudBlobContainer GetStorageContainer()
        {
            var account = CloudStorageAccount.Parse(_appSettings.AzureStorageConnectionString);
            var blobClient = account.CreateCloudBlobClient();
            var container = blobClient.GetContainerReference(_appSettings.AzureStorageContainerName);

            return container;
        }
    }
}
