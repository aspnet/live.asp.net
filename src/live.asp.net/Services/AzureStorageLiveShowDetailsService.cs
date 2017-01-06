// Copyright (c) .NET Foundation. All rights reserved. 
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using live.asp.net.Models;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
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
        private readonly TelemetryClient _telemetry;

        public AzureStorageLiveShowDetailsService(
            IOptions<AppSettings> appSettings,
            IMemoryCache cache,
            TelemetryClient telemetry)
        {
            _appSettings = appSettings.Value;
            _cache = cache;
            _telemetry = telemetry;
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

            var started = Timing.GetTimestamp();
            string fileContents = null;
            try
            {
                fileContents = await blockBlob.DownloadTextAsync();
            }
            finally
            {
                if (_telemetry.IsEnabled())
                {
                    var duration = Timing.GetDuration(started);
                    var dependency = new DependencyTelemetry
                    {
                        Type = "Storage",
                        Target = blockBlob.StorageUri.PrimaryUri.Host,
                        Name = blockBlob.Name,
                        Data = "Download",
                        Timestamp = DateTimeOffset.UtcNow,
                        Duration = duration,
                        Success = fileContents != null
                    };
                    dependency.Metrics.Add("Size", fileContents.Length);
                    dependency.Properties.Add("Storage Uri", blockBlob.StorageUri.PrimaryUri.ToString());
                    _telemetry.TrackDependency(dependency);
                }
            }

            return JsonConvert.DeserializeObject<LiveShowDetails>(fileContents);
        }

        private async Task SaveToAzureStorage(LiveShowDetails liveShowDetails)
        {
            var container = GetStorageContainer();

            await container.CreateIfNotExistsAsync();

            var blockBlob = container.GetBlockBlobReference(_appSettings.AzureStorageBlobName);

            var fileContents = JsonConvert.SerializeObject(liveShowDetails);

            var succeeded = true;
            var started = Timing.GetTimestamp();
            try
            {
                await blockBlob.UploadTextAsync(fileContents);
            }
            catch
            {
                succeeded = false;
                throw;
            }
            finally
            {
                if (_telemetry.IsEnabled())
                {
                    var duration = Timing.GetDuration(started);
                    var dependency = new DependencyTelemetry
                    {
                        Type = "Storage",
                        Target = blockBlob.StorageUri.PrimaryUri.ToString(),
                        Name = blockBlob.Name,
                        Data = "Upload",
                        Timestamp = DateTimeOffset.UtcNow,
                        Duration = duration,
                        Success = succeeded
                    };
                    dependency.Metrics.Add("Size", fileContents.Length);
                    _telemetry.TrackDependency(dependency);
                }
            }
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
