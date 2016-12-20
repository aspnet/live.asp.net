// Copyright (c) .NET Foundation. All rights reserved. 
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
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
            _logger.LogInformation("Priming the cache");
            PrimeCacheImpl("/");
        }

        private void PrimeCacheImpl(string currentPath)
        {
            _logger.LogTrace("Priming cache for {currentPath}", currentPath);
            var prefix = string.Equals(currentPath, "/", StringComparison.OrdinalIgnoreCase) ? "/" : currentPath + "/";
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
            private IFileInfo _fileInfo;
            private byte[] _contents;
            private readonly ILogger _logger;

            public CachedFileInfo(ILogger logger, IFileProvider fileProvider, string subpath)
            {
                _logger = logger;
                _fileProvider = fileProvider;
                _subpath = subpath;

                Refresh();
            }

            private void Refresh()
            {
                _logger.LogDebug("Refreshing contents for {subpath}", _subpath);

                lock (this)
                {
                    _contents = null;
                    // TODO: Handle file load exceptions here
                    _fileInfo = _fileProvider.GetFileInfo(_subpath); 
                    IDisposable callback = null;
                    callback = _fileProvider.Watch(_subpath).RegisterChangeCallback(_ =>
                    {
                        _logger.LogDebug("Change detected for {subpath}", _subpath);
                        Refresh();
                        callback.Dispose();
                    }, null);
                }
            }

            public bool Exists => _fileInfo.Exists;

            public bool IsDirectory => _fileInfo.IsDirectory;

            public DateTimeOffset LastModified => _fileInfo.LastModified;

            public long Length => _fileInfo.Length;

            public string Name => _fileInfo.Name;

            public string PhysicalPath => _fileInfo.PhysicalPath;

            public Stream CreateReadStream()
            {
                if (Length >= _fileSizeLimit)
                {
                    // TODO: The length limit check should really be done in the CachedWebRootFileProvider itself
                    //       as this implementation can result in the FileInfo meta-data being cached while the stream
                    //       (and thus the file contents) itself not being cached, so things like length won't match.s
                    _logger.LogTrace("Stream for {subpath} not cached as it's over the file size limit of {fileSizeLimit}", _subpath, _fileSizeLimit);
                    return _fileInfo.CreateReadStream();
                }

                lock (this)
                {
                    if (_contents == null)
                    {
                        _logger.LogTrace("Caching file contents for {subpath}", _subpath);
                        using (var fs = _fileInfo.CreateReadStream())
                        using (var ms = new MemoryStream((int)fs.Length))
                        {
                            fs.CopyTo(ms);
                            _contents = ms.ToArray();
                        }
                    }

                    return new MemoryStream(_contents);
                }
            }
        }
    }
}