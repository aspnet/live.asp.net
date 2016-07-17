// Copyright (c) .NET Foundation. All rights reserved. 
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using live.asp.net.Models;
using Microsoft.ApplicationInsights;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace live.asp.net.Services
{
    public class AzureStorageShowDetailsService : IShowDetailsService
    {
        private static readonly string CacheKey = nameof(AzureStorageShowDetailsService);

        private readonly AppSettings _appSettings;
        private readonly IMemoryCache _cache;
        private readonly TelemetryClient _telemtry;

        public AzureStorageShowDetailsService(
            IOptions<AppSettings> appSettings,
            IMemoryCache cache,
            TelemetryClient telemetry)
        {
            _appSettings = appSettings.Value;
            _cache = cache;
            _telemtry = telemetry;
        }

        public async Task<ShowDetails> LoadAsync(string showId)
        {
            var showDetails = _cache.Get<ShowDetails>(GetCacheKey(showId));

            if (showDetails == null)
            {
                showDetails = await LoadFromAzureStorage(showId);

                _cache.Set(CacheKey, showDetails, new MemoryCacheEntryOptions
                {
                    AbsoluteExpiration = DateTimeOffset.MaxValue
                });
            }

            return showDetails;
        }

        public async Task SaveAsync(ShowDetails showDetails)
        {
            if (showDetails == null)
            {
                throw new ArgumentNullException(nameof(showDetails));
            }

            await SaveToAzureStorage(showDetails);

            // Update the cache
            _cache.Set(GetCacheKey(showDetails.ShowId), showDetails, new MemoryCacheEntryOptions
            {
                AbsoluteExpiration = DateTimeOffset.MaxValue
            });
        }

        public async Task DeleteAsync(string showId)
        {
            if (string.IsNullOrWhiteSpace(showId))
            {
                throw new ArgumentNullException(nameof(showId));
            }

            await DeleteFromAzureStorage(showId);

            // Remove from cache
            _cache.Remove(GetCacheKey(showId));
        }

        private string GetCacheKey(string showId)
        {
            return $"{CacheKey}_{showId}";
        }

        private string GetBlobName(string showId)
        {
            return $"ShowDetails_{showId}";
        }

        private async Task<ShowDetails> LoadFromAzureStorage(string showId)
        {
            var container = GetStorageContainer();

            if (!await container.ExistsAsync())
            {
                return null;
            }

            var blockBlob = container.GetBlockBlobReference(GetBlobName(showId));

            if (!await blockBlob.ExistsAsync())
            {
                return null;
            }

            var downloadStarted = DateTimeOffset.UtcNow;
            var fileContents = await blockBlob.DownloadTextAsync();
            _telemtry.TrackDependency("Azure.BlobStorage", "DownloadTextAsync", downloadStarted, DateTimeOffset.UtcNow - downloadStarted, true);

            return JsonConvert.DeserializeObject<ShowDetails>(fileContents);
        }

        private async Task SaveToAzureStorage(ShowDetails showDetails)
        {
            var container = GetStorageContainer();

            await container.CreateIfNotExistsAsync();

            var blockBlob = container.GetBlockBlobReference(GetBlobName(showDetails.ShowId));

            var fileContents = JsonConvert.SerializeObject(showDetails);

            var uploadStarted = DateTimeOffset.UtcNow;
            await blockBlob.UploadTextAsync(fileContents);
            _telemtry.TrackDependency("Azure.BlobStorage", "UploadTextAsync", uploadStarted, DateTimeOffset.UtcNow - uploadStarted, true);
        }

        private async Task DeleteFromAzureStorage(string showId)
        {
            var container = GetStorageContainer();

            await container.CreateIfNotExistsAsync();

            var blockBlob = container.GetBlockBlobReference(GetBlobName(showId));
            
            var deleteStarted = DateTimeOffset.UtcNow;
            await blockBlob.DeleteIfExistsAsync();
            _telemtry.TrackDependency("Azure.BlobStorage", "DeleteIfExistsAsync", deleteStarted, DateTimeOffset.UtcNow - deleteStarted, true);
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