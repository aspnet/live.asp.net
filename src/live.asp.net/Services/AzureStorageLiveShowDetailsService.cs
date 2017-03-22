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

        public async Task LoadAsync(ILiveShowDetails liveShowDetails)
        {
            var result = _cache.Get<string>(CacheKey);

            if (result == null)
            {
                await LoadFromAzureStorage(liveShowDetails);
                result = JsonConvert.SerializeObject(liveShowDetails);

                _cache.Set(CacheKey, result, new MemoryCacheEntryOptions
                {
                    AbsoluteExpiration = DateTimeOffset.MaxValue
                });
            }
            else
            {
                JsonConvert.PopulateObject(result, liveShowDetails);
            }
        }

        public async Task SaveAsync(ILiveShowDetails liveShowDetails)
        {
            if (liveShowDetails == null)
            {
                throw new ArgumentNullException(nameof(liveShowDetails));
            }

            await SaveToAzureStorage(liveShowDetails);
            var result = JsonConvert.SerializeObject(liveShowDetails);

            // Update the cache
            _cache.Set(CacheKey, result, new MemoryCacheEntryOptions
            {
                AbsoluteExpiration = DateTimeOffset.MaxValue
            });
        }

        private async Task LoadFromAzureStorage(ILiveShowDetails liveShowDetails)
        {
            var container = GetStorageContainer();

            if (!await container.ExistsAsync())
            {
                return;
            }

            var blockBlob = container.GetBlockBlobReference(_appSettings.AzureStorageBlobName);
            if (!await blockBlob.ExistsAsync())
            {
                return;
            }

            string fileContents = null;
            var started = Timing.GetTimestamp();
            try
            {
                fileContents = await blockBlob.DownloadTextAsync();
            }
            finally
            {
                TrackDependency(blockBlob, "Download", fileContents.Length, started, succeeded: fileContents != null);
            }

            JsonConvert.PopulateObject(fileContents, liveShowDetails);
        }

        private async Task SaveToAzureStorage(ILiveShowDetails liveShowDetails)
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
                TrackDependency(blockBlob, "Upload", fileContents.Length, started, succeeded);
            }
        }

        private void TrackDependency(CloudBlockBlob blockBlob, string operation, long length, long started, bool succeeded)
        {
            if (_telemetry.IsEnabled())
            {
                var duration = Timing.GetDuration(started);
                var dependency = new DependencyTelemetry
                {
                    Type = "Storage",
                    Target = blockBlob.StorageUri.PrimaryUri.Host,
                    Name = blockBlob.Name,
                    Data = operation,
                    Timestamp = DateTimeOffset.UtcNow,
                    Duration = duration,
                    Success = succeeded
                };
                dependency.Metrics.Add("Size", length);
                dependency.Properties.Add("Storage Uri", blockBlob.StorageUri.PrimaryUri.ToString());
                _telemetry.TrackDependency(dependency);
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
