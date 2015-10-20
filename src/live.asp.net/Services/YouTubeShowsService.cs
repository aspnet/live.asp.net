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
using Microsoft.AspNet.Hosting;
using Microsoft.Framework.Caching.Memory;
using Microsoft.Framework.OptionsModel;
using Microsoft.Framework.WebEncoders;
using Google.Apis.YouTube.v3.Data;
using System.Text.RegularExpressions;
using System.Text;

namespace live.asp.net.Services
{
    public class YouTubeShowsService : IShowsService
    {
        public const string CacheKey = nameof(YouTubeShowsService);

        public int ResultsCount { get; } = 3 * 8;

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
                playlistRequest.MaxResults = ResultsCount;

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
                    videosContentDetailsDictionary.Add(item.Id, item);
                
                var result = new ShowList();

                result.Shows = playlistItems.Items.Select(playlistItem =>
                {
                    var videoContentDetails = videosContentDetailsDictionary[playlistItem.Snippet.ResourceId.VideoId];
                    return new Show
                    {
                        Provider = "YouTube",
                        ProviderId = videoContentDetails.Id,
                        Title = GetUsefulBitsFromTitle(playlistItem.Snippet.Title),
                        Description = playlistItem.Snippet.Description,
                        ShowDate = DateTimeOffset.Parse(playlistItem.Snippet.PublishedAtRaw, null, DateTimeStyles.RoundtripKind),
                        ThumbnailUrl = playlistItem.Snippet.Thumbnails.High.Url,
                        Url = GetVideoUrl(videoContentDetails.Id,
                                          playlistItem.Snippet.PlaylistId,
                                          playlistItem.Snippet.Position ?? 0),
                        DurationLabel = ParseDuration(videoContentDetails.ContentDetails.Duration)
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
            var encodedId = UrlEncoder.Default.UrlEncode(id);
            var encodedPlaylistId = UrlEncoder.Default.UrlEncode(playlistId);
            var encodedItemIndex = UrlEncoder.Default.UrlEncode(itemIndex.ToString());

            return $"https://www.youtube.com/watch?v={encodedId}&list={encodedPlaylistId}&index={encodedItemIndex}";
        }

        private static string GetPlaylistUrl(string playlistId)
        {
            var encodedPlaylistId = UrlEncoder.Default.UrlEncode(playlistId);

            return $"https://www.youtube.com/playlist?list={encodedPlaylistId}";
        }

        private static string ParseDuration(string duration)
        {
            duration = duration.ToUpper();

			// youtube api format: ISO 8601 duration
			// https://developers.google.com/youtube/v3/docs/videos#contentDetails.duration
            // we're interested in the sequence T(#H)#M#S (e.g. PT15M33S) but a more complicate format can be returned (e.g. P3W3DT20H31M21S)
            var time = Regex.Match(duration, @"T(\d+H)?\d+M\d+S");
            if (time.Success)
            {
                var digitValues = Regex.Matches(time.Value, @"(\d+)");
                StringBuilder sb = new StringBuilder();

                int x;
                foreach (var value in digitValues)
                    if (int.TryParse(value.ToString(), out x))
                        sb.Append($":{x.ToString("00")}"); // two-digits format

                var s = sb.ToString();
                if (s.Length > 0)
                    return s.Substring(1); // removes first ':'
            }

            return null;
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
                    DurationLabel = "26:51"
                },
                new Show
                {
                    ShowDate = new DateTimeOffset(2015, 7, 14, 15, 30, 0, _pdtOffset),
                    Title = "ASP.NET Community Standup - July 14th 2015",
                    Provider = "YouTube",
                    ProviderId = "bFXseBPGAyQ",
                    ThumbnailUrl = "http://img.youtube.com/vi/bFXseBPGAyQ/mqdefault.jpg",
                    Url = "https://www.youtube.com/watch?v=bFXseBPGAyQ&index=2&list=PL0M0zPgJ3HSftTAAHttA3JQU4vOjXFquF",
                    DurationLabel = "28:24"
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
                    DurationLabel = "01:26:51"
                },
            };
        }
    }
}
