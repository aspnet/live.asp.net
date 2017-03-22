// Copyright (c) .NET Foundation. All rights reserved. 
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Threading.Tasks;
using live.asp.net.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;

namespace live.asp.net.Services
{
    public class FileSystemLiveShowDetailsService : ILiveShowDetailsService
    {
        private static readonly string CacheKey = nameof(FileSystemLiveShowDetailsService);
        private static readonly string FileName = "ShowDetails.json";

        private readonly IMemoryCache _cache;
        private readonly string _filePath;

        public FileSystemLiveShowDetailsService(IHostingEnvironment hostingEnv, IMemoryCache cache)
        {
            _cache = cache;
            _filePath = Path.Combine(hostingEnv.ContentRootPath, FileName);
        }

        public async Task LoadAsync(ILiveShowDetails liveShowDetails)
        {
            var result = _cache.Get<string>(CacheKey);

            if (result == null)
            {
                await LoadFromFile(liveShowDetails);
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
                return;
            }

            var fileContents = JsonConvert.SerializeObject(liveShowDetails);
            using (var fileWriter = new StreamWriter(new FileStream(_filePath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, useAsync: true)))
            {
                await fileWriter.WriteAsync(fileContents);
            }

            _cache.Remove(CacheKey);
        }

        private async Task LoadFromFile(ILiveShowDetails liveShowDetails)
        {
            if (!File.Exists(_filePath))
            {
                return;
            }

            string fileContents;
            using (var fileReader = new StreamReader(new FileStream(_filePath, FileMode.OpenOrCreate, FileAccess.Read, FileShare.Read, 4096, useAsync: true)))
            {
                fileContents = await fileReader.ReadToEndAsync();
            }

            JsonConvert.PopulateObject(fileContents, liveShowDetails);
        }
    }
}
