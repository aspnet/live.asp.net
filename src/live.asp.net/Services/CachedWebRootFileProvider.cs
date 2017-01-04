// Copyright (c) .NET Foundation. All rights reserved. 
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;

namespace live.asp.net.Services
{
    public class CachedWebRootFileProvider : IFileProvider
    {
        private static readonly int _fileSizeLimit = 256 * 1024; // bytes
        private static readonly double TimestampToTicks = TimeSpan.TicksPerSecond / (double)Stopwatch.Frequency;

        private readonly ILogger<CachedWebRootFileProvider> _logger;
        private readonly IFileProvider _fileProvider;
        private readonly IMemoryCache _cache;

        public CachedWebRootFileProvider(ILogger<CachedWebRootFileProvider> logger, IHostingEnvironment hostingEnv, IMemoryCache memoryCache)
        {
            _logger = logger;
            _fileProvider = hostingEnv.WebRootFileProvider;
            _cache = memoryCache;
        }

        public void PrimeCache()
        {
            var startTimestamp = _logger.IsEnabled(LogLevel.Information) ? Stopwatch.GetTimestamp() : 0;

            _logger.LogInformation("Priming the cache");
            var cacheSize = PrimeCacheImpl("/");

            if (startTimestamp != 0)
            {
                var currentTimestamp = Stopwatch.GetTimestamp();
                var elapsed = new TimeSpan((long)(TimestampToTicks * (currentTimestamp - startTimestamp)));
                _logger.LogInformation("Cache primed with {cacheEntriesCount} entries totalling {cacheEntriesSizeBytes} bytes in {elapsed}", cacheSize.Item1, cacheSize.Item2, elapsed);
            }
        }

        private Tuple<int, long> PrimeCacheImpl(string currentPath)
        {
            _logger.LogTrace("Priming cache for {currentPath}", currentPath);
            var cacheEntriesAdded = 0;
            var bytesCached = (long)0;

            // TODO: Normalize the currentPath here, e.g. strip/always-add leading slashes, ensure slash consistency, etc.
            var prefix = string.Equals(currentPath, "/", StringComparison.OrdinalIgnoreCase) ? "/" : currentPath + "/";

            foreach (var fileInfo in GetDirectoryContents(currentPath))
            {
                if (fileInfo.IsDirectory)
                {
                    var cacheSize = PrimeCacheImpl(prefix + fileInfo.Name);
                    cacheEntriesAdded += cacheSize.Item1;
                    bytesCached += cacheSize.Item2;
                }
                else
                {
                    var stream = GetFileInfo(prefix + fileInfo.Name).CreateReadStream();
                    bytesCached += stream.Length;
                    stream.Dispose();
                    cacheEntriesAdded++;
                }
            }

            return Tuple.Create(cacheEntriesAdded, bytesCached);
        }

        public IDirectoryContents GetDirectoryContents(string subpath)
        {
            // TODO: Normalize the subpath here, e.g. strip/always-add leading slashes, ensure slash consistency, etc.
            var key = nameof(GetDirectoryContents) + "_" + subpath;
            IDirectoryContents cachedResult;
            if (_cache.TryGetValue(key, out cachedResult))
            {
                // Item already exists in cache, just return it
                return cachedResult;
            }
            
            var directoryContents = _fileProvider.GetDirectoryContents(subpath);
            if (!directoryContents.Exists)
            {
                // Requested subpath doesn't exist, just return
                return directoryContents;
            }

            // Create the cache entry and return
            var cacheEntry = _cache.CreateEntry(key);
            cacheEntry.Value = directoryContents;
            cacheEntry.RegisterPostEvictionCallback((k, value, reason, s) =>
                _logger.LogTrace("Cache entry {key} was evicted due to {reason}", k, reason));
            return directoryContents;
        }

        public IFileInfo GetFileInfo(string subpath)
        {
            // TODO: Normalize the subpath here, e.g. strip/always-add leading slashes, ensure slash consistency, etc.
            var key = nameof(GetFileInfo) + "_" + subpath;
            IFileInfo cachedResult;
            if (_cache.TryGetValue(key, out cachedResult))
            {
                // Item already exists in cache, just return it
                return cachedResult;
            }

            var fileInfo = _fileProvider.GetFileInfo(subpath);
            if (!fileInfo.Exists)
            {
                // Requested subpath doesn't exist, just return it
                return fileInfo;
            }

            if (fileInfo.Length > _fileSizeLimit)
            {
                // File is too large to cache, just return it
                _logger.LogTrace("File contents for {subpath} will not be cached as it's over the file size limit of {fileSizeLimit}", subpath, _fileSizeLimit);
                return fileInfo;
            }

            // Create the cache entry and return
            var cachedFileInfo = new CachedFileInfo(_logger, fileInfo, subpath);
            var fileChangedToken = Watch(subpath);
            fileChangedToken.RegisterChangeCallback(_ => _logger.LogDebug("Change detected for {subpath} located at {filepath}", subpath, fileInfo.PhysicalPath), null);
            var cacheEntry = _cache.CreateEntry(key)
                .RegisterPostEvictionCallback((k, value, reason, s) =>
                    _logger.LogTrace("Cache entry {key} was evicted due to {reason}", k, reason))
                .AddExpirationToken(fileChangedToken)
                .SetValue(cachedFileInfo);
            // You have to call Dispose() to actually add the item to the underlying cache. Yeah, I know.
            cacheEntry.Dispose();
            return cachedFileInfo;
        }

        public IChangeToken Watch(string filter)
        {
            return _fileProvider.Watch(filter);
        }

        private class CachedFileInfo : IFileInfo
        {
            private readonly ILogger _logger;
            private readonly IFileInfo _fileInfo;
            private readonly string _subpath;
            private byte[] _contents;

            public CachedFileInfo(ILogger logger, IFileInfo fileInfo, string subpath)
            {
                _logger = logger;
                _fileInfo = fileInfo;
                _subpath = subpath;
            }

            public bool Exists => _fileInfo.Exists;

            public bool IsDirectory => _fileInfo.IsDirectory;

            public DateTimeOffset LastModified => _fileInfo.LastModified;

            public long Length => _fileInfo.Length;

            public string Name => _fileInfo.Name;

            public string PhysicalPath => _fileInfo.PhysicalPath;

            public Stream CreateReadStream()
            {
                var contents = _contents;
                if (contents != null)
                {
                    _logger.LogTrace("Returning cached file contents for {subpath} located at {filepath}", _subpath, _fileInfo.PhysicalPath);
                    return new MemoryStream(contents);
                }
                else
                {
                    _logger.LogTrace("Loading file contents for {subpath} located at {filepath}", _subpath, _fileInfo.PhysicalPath);
                    MemoryStream ms;
                    using (var fs = _fileInfo.CreateReadStream())
                    {
                        ms = new MemoryStream((int)fs.Length);
                        fs.CopyTo(ms);
                        contents = ms.ToArray();
                        ms.Position = 0;
                    }

                    if (Interlocked.CompareExchange(ref _contents, contents, null) == null)
                    {
                        _logger.LogTrace("Cached file contents for {subpath} located at {filepath}", _subpath, _fileInfo.PhysicalPath);
                    }

                    return ms;
                }
            }
        }
    }
}