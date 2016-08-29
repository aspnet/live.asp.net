﻿// Copyright (c) .NET Foundation. All rights reserved. 
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

        public async Task<LiveShowDetails> LoadAsync()
        {
            var result = _cache.Get<LiveShowDetails>(CacheKey);

            if (result == null)
            {
                result = await LoadFromFile();

                _cache.Set(CacheKey, result, new MemoryCacheEntryOptions
                {
                    AbsoluteExpiration = DateTimeOffset.MaxValue
                });
            }

            return result;
        }

        public async Task SaveAsync(LiveShowDetails liveShowDetails)
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

        private async Task<LiveShowDetails> LoadFromFile()
        {
            if (!File.Exists(_filePath))
            {
                return null;
            }

            string fileContents;
            using (var fileReader = new StreamReader(new FileStream(_filePath, FileMode.OpenOrCreate, FileAccess.Read, FileShare.Read, 4096, useAsync: true)))
            {
                fileContents = await fileReader.ReadToEndAsync();
            }

            return JsonConvert.DeserializeObject<LiveShowDetails>(fileContents);
        }
    }
}
