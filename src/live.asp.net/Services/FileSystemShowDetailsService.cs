// Copyright (c) .NET Foundation. All rights reserved. 
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using live.asp.net.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading.Tasks;

namespace live.asp.net.Services
{
    public class FileSystemShowDetailsService : IShowDetailsService
    {
        private static readonly string CacheKey = nameof(FileSystemShowDetailsService);
        private static readonly string FileName = "ShowDetails_{0}.json";

        private readonly IMemoryCache _cache;
        private readonly DirectoryInfo _contentRootDirectoy;

        public FileSystemShowDetailsService(IHostingEnvironment hostingEnv, IMemoryCache cache)
        {
            _cache = cache;
            _contentRootDirectoy = new DirectoryInfo(hostingEnv.ContentRootPath);
        }

        public async Task<ShowDetails> LoadAsync(string showId)
        {
            var result = _cache.Get<ShowDetails>(GetCacheKey(showId));

            if (result == null)
            {
                result = await LoadFromFile(showId);

                _cache.Set(GetCacheKey(showId), result, new MemoryCacheEntryOptions
                {
                    AbsoluteExpiration = DateTimeOffset.MaxValue
                });
            }

            return result;
        }

        public async Task SaveAsync(ShowDetails showDetails)
        {
            if (showDetails == null)
            {
                return;
            }

            var fileContents = JsonConvert.SerializeObject(showDetails);
            using (var fileWriter = new StreamWriter(new FileStream(GetFilePath(showDetails.ShowId), FileMode.Create, FileAccess.Write, FileShare.None, 4096, useAsync: true)))
            {
                await fileWriter.WriteAsync(fileContents);
            }

            _cache.Remove(GetCacheKey(showDetails.ShowId));
        }

        public Task DeleteAsync(string showId)
        {
            if (string.IsNullOrWhiteSpace(showId))
            {
                return Task.CompletedTask;
            }

            var file = new FileInfo(GetFilePath(showId));

            if (file.Exists)
            {
                file.Delete();
            }

            _cache.Remove(GetCacheKey(showId));

            return Task.CompletedTask;
        }

        private string GetCacheKey(string showId)
        {
            return $"{CacheKey}_{showId}";
        }

        private string GetFilePath(string showId)
        {
            return Path.Combine(_contentRootDirectoy.FullName, string.Format(FileName, showId));
        }

        private async Task<ShowDetails> LoadFromFile(string showId)
        {
            string filePath = GetFilePath(showId);

            if (!File.Exists(filePath))
            {
                return null;
            }

            string fileContents;
            using (var fileReader = new StreamReader(new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Read, FileShare.Read, 4096, useAsync: true)))
            {
                fileContents = await fileReader.ReadToEndAsync();
            }

            return JsonConvert.DeserializeObject<ShowDetails>(fileContents);
        }
    }
}