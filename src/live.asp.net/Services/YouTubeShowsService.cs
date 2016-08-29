// Copyright (c) .NET Foundation. All rights reserved. 
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using live.asp.net.Models;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using System.Text.Encodings.Web;
using Google.Apis.YouTube.v3.Data;
using static System.Xml.XmlConvert;

namespace live.asp.net.Services
{
    public class YouTubeShowsService : IShowsService
    {
        public const string CacheKey = nameof(YouTubeShowsService);

        private const string _providerName = "YouTube";

        private const int _resultsCount = 3 * 8;

        private readonly IHostingEnvironment _env;
        private readonly AppSettings _appSettings;
        private readonly IMemoryCache _cache;
        private readonly TelemetryClient _telemetry;

        public YouTubeShowsService(
            IHostingEnvironment env,
            IOptions<AppSettings> appSettings,
            IMemoryCache memoryCache,
            TelemetryClient telemetry)
        {
            _env = env;
            _appSettings = appSettings.Value;
            _cache = memoryCache;
            _telemetry = telemetry;
        }

        public async Task<ShowList> GetRecordedShowsAsync(ClaimsPrincipal user, bool disableCache)
        {
            if (string.IsNullOrEmpty(_appSettings.YouTubeApiKey))
            {
                return new ShowList { Shows = DesignData.Shows };
            }

            if (user.Identity.IsAuthenticated && disableCache)
            {
                return await GetShowsList();
            }

            var result = _cache.Get<ShowList>(CacheKey);

            if (result == null)
            {
                result = await GetShowsList();

                _cache.Set(CacheKey, result, new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(1)
                });
            }

            return result;
        }

        private async Task<ShowList> GetShowsList()
        {
            using (var client = new YouTubeService(new BaseClientService.Initializer
            {
                ApplicationName = _appSettings.YouTubeApplicationName,
                ApiKey = _appSettings.YouTubeApiKey
            }))
            {
                var playlistRequest = client.PlaylistItems.List("snippet");
                playlistRequest.PlaylistId = _appSettings.YouTubePlaylistId;
                playlistRequest.MaxResults = _resultsCount;

                var requestStart = DateTimeOffset.UtcNow;
                var playlistItems = await playlistRequest.ExecuteAsync();
                _telemetry.TrackDependency("YouTube.PlaylistItemsApi", "List", requestStart, DateTimeOffset.UtcNow - requestStart, true);
                
                var videosRequest = client.Videos.List("contentDetails");
                var ids = string.Join(",", playlistItems.Items.Select(item => item.Snippet.ResourceId.VideoId));
                videosRequest.Id = ids;

                requestStart = DateTimeOffset.UtcNow;
                var videos = await videosRequest.ExecuteAsync();
                _telemetry.TrackDependency("YouTube.VideosApi", "List", requestStart, DateTimeOffset.UtcNow - requestStart, true);
                
                var videosContentDetailsDictionary = new Dictionary<string, Video>(videos.Items.Count);
                foreach (var item in videos.Items)
                {
                    videosContentDetailsDictionary.Add(item.Id, item);
                }
                
                var result = new ShowList();

                result.Shows = playlistItems.Items.Select(playlistItem =>
                {
                    var videoContentDetails = videosContentDetailsDictionary[playlistItem.Snippet.ResourceId.VideoId];
                    return new Show
                    {
                        Provider = _providerName,
                        ProviderId = playlistItem.Snippet.ResourceId.VideoId,
                        Title = GetUsefulBitsFromTitle(playlistItem.Snippet.Title),
                        Description = playlistItem.Snippet.Description,
                        ShowDate = DateTimeOffset.Parse(playlistItem.Snippet.PublishedAtRaw, null, DateTimeStyles.RoundtripKind),
                        ThumbnailUrl = playlistItem.Snippet.Thumbnails.High.Url,
                        Url = GetVideoUrl(playlistItem.Snippet.ResourceId.VideoId,
                                          playlistItem.Snippet.PlaylistId,
                                          playlistItem.Snippet.Position ?? 0),
                        Duration = ParseDuration(videoContentDetails.ContentDetails.Duration)
                    };
                }).ToList();

                if (!string.IsNullOrEmpty(playlistItems.NextPageToken))
                {
                    result.MoreShowsUrl = GetPlaylistUrl(_appSettings.YouTubePlaylistId);
                }

                return result;
            }
        }

        private static string GetUsefulBitsFromTitle(string title)
        {
            if (title.Count(c => c == '-') < 2)
            {
                return string.Empty;
            }

            var lastHyphen = title.LastIndexOf('-');
            if (lastHyphen >= 0)
            {
                var result = title.Substring(lastHyphen + 1);
                return result;
            }

            return string.Empty;
        }

        private static string GetVideoUrl(string id, string playlistId, long itemIndex)
        {
            var encodedId = UrlEncoder.Default.Encode(id);
            var encodedPlaylistId = UrlEncoder.Default.Encode(playlistId);
            var encodedItemIndex = UrlEncoder.Default.Encode(itemIndex.ToString());

            return $"https://www.youtube.com/watch?v={encodedId}&list={encodedPlaylistId}&index={encodedItemIndex}";
        }

        private static string GetPlaylistUrl(string playlistId)
        {
            var encodedPlaylistId = UrlEncoder.Default.Encode(playlistId);

            return $"https://www.youtube.com/playlist?list={encodedPlaylistId}";
        }

        private static string ParseDuration(string duration)
        {
            // Youtube api format: ISO 8601 duration
            // https://developers.google.com/youtube/v3/docs/videos#contentDetails.duration
            // Youtube api returns the notation which includes "weeks"
            try
            {
                // Conversion based on the W3C XML Schema Part 2: Datatypes recommendation for duration
                // http://www.w3.org/TR/xmlschema-2/#duration
                var timeSpan = ToTimeSpan(duration);
                var hours = timeSpan.Hours > 0 ? $"{timeSpan.Hours.ToString("00")}:" : string.Empty;
                return $"{hours}{timeSpan.Minutes.ToString("00")}:{timeSpan.Seconds.ToString("00")}";
            } catch (FormatException)
            {
                // Duration doesn't match the format P#Y#M#DT#H#M#S (possibly includes weeks)
                return null;
            }
        }

        private static class DesignData
        {
            private static readonly TimeSpan _pdtOffset = TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time").BaseUtcOffset;

            public static readonly string LiveShow = null;

            public static readonly IList<Show> Shows = new List<Show>
            {
                new Show
                {
                    ShowDate = new DateTimeOffset(2015, 7, 21, 9, 30, 0, _pdtOffset),
                    Title = "ASP.NET Community Standup - July 21st 2015",
                    Provider = "YouTube",
                    ProviderId = "7O81CAjmOXk",
                    ThumbnailUrl = "http://img.youtube.com/vi/7O81CAjmOXk/mqdefault.jpg",
                    Url = "https://www.youtube.com/watch?v=7O81CAjmOXk&index=1&list=PL0M0zPgJ3HSftTAAHttA3JQU4vOjXFquF",
                    Duration = "26:51"
                },
                new Show
                {
                    ShowDate = new DateTimeOffset(2015, 7, 14, 15, 30, 0, _pdtOffset),
                    Title = "ASP.NET Community Standup - July 14th 2015",
                    Provider = "YouTube",
                    ProviderId = "bFXseBPGAyQ",
                    ThumbnailUrl = "http://img.youtube.com/vi/bFXseBPGAyQ/mqdefault.jpg",
                    Url = "https://www.youtube.com/watch?v=bFXseBPGAyQ&index=2&list=PL0M0zPgJ3HSftTAAHttA3JQU4vOjXFquF",
                    Duration = "28:24"
                },

                new Show
                {
                    ShowDate = new DateTimeOffset(2015, 7, 7, 15, 30, 0, _pdtOffset),
                    Title = "ASP.NET Community Standup - July 7th 2015",
                    Provider = "YouTube",
                    ProviderId = "APagQ1CIVGA",
                    ThumbnailUrl = "http://img.youtube.com/vi/APagQ1CIVGA/mqdefault.jpg",
                    Url = "https://www.youtube.com/watch?v=APagQ1CIVGA&index=3&list=PL0M0zPgJ3HSftTAAHttA3JQU4vOjXFquF"
                },
                new Show
                {
                    ShowDate = DateTimeOffset.Now.AddDays(-28),
                    Title = "ASP.NET Community Standup - July 21st 2015",
                    Provider = "YouTube",
                    ProviderId = "7O81CAjmOXk",
                    ThumbnailUrl = "http://img.youtube.com/vi/7O81CAjmOXk/mqdefault.jpg",
                    Url = "https://www.youtube.com/watch?v=7O81CAjmOXk&index=1&list=PL0M0zPgJ3HSftTAAHttA3JQU4vOjXFquF",
                    Duration = "01:26:51"
                },
            };
        }
    }
}
