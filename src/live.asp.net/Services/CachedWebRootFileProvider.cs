// Copyright (c) .NET Foundation. All rights reserved. 
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
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
            // TODO: Log time taken to prime the cache
            _logger.LogInformation("Priming the cache");
            PrimeCacheImpl("/");
        }

        private void PrimeCacheImpl(string currentPath)
        {
            _logger.LogTrace("Priming cache for {currentPath}", currentPath);

            // TODO: Normalize the currentPath here, e.g. strip/always-add leading slashes, ensure slash consistency, etc.
            var prefix = string.Equals(currentPath, "/", StringComparison.OrdinalIgnoreCase) ? "/" : currentPath + "/";

            // TODO: Parallel.ForEach?
            foreach (var fileInfo in GetDirectoryContents(currentPath))
            {
                if (fileInfo.IsDirectory)
                {
                    PrimeCacheImpl(prefix + fileInfo.Name);
                }
                else
                {
                    GetFileInfo(prefix + fileInfo.Name).CreateReadStream().Dispose();
                }
            }
        }

        // TODO: Should move the lookup of the FileInfo here such that we can check length against limit and
        //       whether IFileInfo.Exists is false and not cache in that case.

        public IDirectoryContents GetDirectoryContents(string subpath) =>
            _cache.GetOrCreate(nameof(GetDirectoryContents) + "_" + subpath, ce =>
            {
                ce.RegisterPostEvictionCallback((key, value, reason, s) =>
                    _logger.LogTrace("Cache entry {key} was evicted due to {reason}", key, reason));
                return _fileProvider.GetDirectoryContents(subpath);
            });

        public IFileInfo GetFileInfo(string subpath) =>
            _cache.GetOrCreate(nameof(GetFileInfo) + "_" + subpath, ce =>
            {
                ce.RegisterPostEvictionCallback((key, value, reason, s) =>
                    _logger.LogTrace("Cache entry {key} was evicted due to {reason}", key, reason));
                return new CachedFileInfo(_logger, _fileProvider, subpath);
            });

        public IChangeToken Watch(string filter)
        {
            return _fileProvider.Watch(filter);
        }

        private class CachedFileInfo : IFileInfo
        {
            private static readonly int _fileSizeLimit = 256 * 1024; // bytes
            private readonly IFileProvider _fileProvider;
            private readonly string _subpath;
            private CacheEntry _cacheEntry;
            private readonly ILogger _logger;

            public CachedFileInfo(ILogger logger, IFileProvider fileProvider, string subpath)
            {
                _logger = logger;
                _fileProvider = fileProvider;
                _subpath = subpath;

                LoadFileInfoFromUnderlyingProvider();
            }

            private void LoadFileInfoFromUnderlyingProvider()
            {
                // TODO: Handle file load exceptions here
                var existingCacheEntry = _cacheEntry;
                var newCacheEntry = new CacheEntry(_fileProvider.GetFileInfo(_subpath), null);
                var oldCacheEntry = Interlocked.CompareExchange(ref _cacheEntry, newCacheEntry, existingCacheEntry);
                if (oldCacheEntry == existingCacheEntry)
                {
                    _logger.LogDebug("Refreshed contents for {subpath} located at {filepath}", _subpath, newCacheEntry.FileInfo.PhysicalPath);
                    _fileProvider.Watch(_subpath).RegisterChangeCallback(OnChange, this);
                }
            }

            private static void OnChange(object state)
            {
                var self = (CachedFileInfo)state;
                self._logger.LogDebug("Change detected for {subpath} located at {filepath}", self._subpath, self._cacheEntry.FileInfo.PhysicalPath);
                self.LoadFileInfoFromUnderlyingProvider();
            }

            public bool Exists => _cacheEntry.FileInfo.Exists;

            public bool IsDirectory => _cacheEntry.FileInfo.IsDirectory;

            public DateTimeOffset LastModified => _cacheEntry.FileInfo.LastModified;

            public long Length => _cacheEntry.FileInfo.Length;

            public string Name => _cacheEntry.FileInfo.Name;

            public string PhysicalPath => _cacheEntry.FileInfo.PhysicalPath;

            public Stream CreateReadStream()
            {
                if (Length >= _fileSizeLimit)
                {
                    // TODO: The length limit check should really be done in the CachedWebRootFileProvider itself
                    //       as this implementation can result in the FileInfo meta-data being cached while the stream
                    //       (and thus the file contents) itself not being cached, so things like length won't match.
                    _logger.LogTrace("Stream for {subpath} not cached as it's over the file size limit of {fileSizeLimit}", _subpath, _fileSizeLimit);
                    return _cacheEntry.FileInfo.CreateReadStream();
                }

                var contents = _cacheEntry.Contents;
                if (contents != null)
                {
                    return new MemoryStream(contents);
                }
                else
                {
                    _logger.LogTrace("Reading file contents for {subpath} located at {filepath}", _subpath, _cacheEntry.FileInfo.PhysicalPath);
                    using (var fs = _cacheEntry.FileInfo.CreateReadStream())
                    using (var ms = new MemoryStream((int)fs.Length))
                    {
                        fs.CopyTo(ms);
                        contents = ms.ToArray();
                    }

                    _cacheEntry.TrySetContents(contents);

                    return new MemoryStream(contents);
                }
            }

            private class CacheEntry
            {
                private byte[] _contents;

                public CacheEntry(IFileInfo fileInfo, byte[] contents)
                {
                    FileInfo = fileInfo;
                    _contents = contents;
                }

                public IFileInfo FileInfo { get; }

                public byte[] Contents => _contents;

                public bool TrySetContents(byte[] contents)
                {
                    return Interlocked.CompareExchange(ref _contents, contents, null) == null;
                }
            }
        }
    }
}