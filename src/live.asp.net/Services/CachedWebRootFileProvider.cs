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

        // TODO: Should move the lookup of the FileInfo here such that we can check length against limit and
        //       whether IFileInfo.Exists is false and not cache in that case.
        //       Indeed it might be possible to simplify this a lot by doing all the logic here and letting IMemoryCache
        //       deal with concurrency checks, etc. rather than CachedFileInfo doing it internally.

        public IDirectoryContents GetDirectoryContents(string subpath) =>
            // TODO: Normalize the subpath here, e.g. strip/always-add leading slashes, ensure slash consistency, etc.
            _cache.GetOrCreate(nameof(GetDirectoryContents) + "_" + subpath, ce =>
            {
                ce.RegisterPostEvictionCallback((key, value, reason, s) =>
                    _logger.LogTrace("Cache entry {key} was evicted due to {reason}", key, reason));
                return _fileProvider.GetDirectoryContents(subpath);
            });

        public IFileInfo GetFileInfo(string subpath) =>
            // TODO: Normalize the subpath here, e.g. strip/always-add leading slashes, ensure slash consistency, etc.
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
                var existingCacheEntry = _cacheEntry;
                var newCacheEntry = new CacheEntry(_fileProvider.GetFileInfo(_subpath), null);
                var oldCacheEntry = Interlocked.CompareExchange(ref _cacheEntry, newCacheEntry, existingCacheEntry);
                if (oldCacheEntry == existingCacheEntry)
                {
                    _logger.LogDebug("Loaded file info for {subpath} located at {filepath}", _subpath, newCacheEntry.FileInfo.PhysicalPath);
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
                    _logger.LogTrace("File contents for {subpath} will not be cached as it's over the file size limit of {fileSizeLimit}", _subpath, _fileSizeLimit);
                    return _cacheEntry.FileInfo.CreateReadStream();
                }

                var contents = _cacheEntry.Contents;
                if (contents != null)
                {
                    _logger.LogTrace("Returning cached file contents for {subpath} located at {filepath}", _subpath, _cacheEntry.FileInfo.PhysicalPath);
                    return new MemoryStream(contents);
                }
                else
                {
                    _logger.LogTrace("Loading file contents for {subpath} located at {filepath}", _subpath, _cacheEntry.FileInfo.PhysicalPath);
                    MemoryStream ms;
                    using (var fs = _cacheEntry.FileInfo.CreateReadStream())
                    {
                        ms = new MemoryStream((int)fs.Length);
                        fs.CopyTo(ms);
                        contents = ms.ToArray();
                        ms.Position = 0;
                    }

                    if (_cacheEntry.TrySetContents(contents))
                    {
                        _logger.LogTrace("Cached file contents for {subpath} located at {filepath}", _subpath, _cacheEntry.FileInfo.PhysicalPath);
                    }

                    return ms;
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