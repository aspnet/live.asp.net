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

namespace live.asp.net.Services
{
    public class YouTubeShowsService : IShowsService
    {
        public const string CacheKey = nameof(YouTubeShowsService);

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

        public async Task<Show> GetShowAsync(string providerId, ClaimsPrincipal user, bool disableCache)
        {
            if (string.IsNullOrEmpty(_appSettings.YouTubeApiKey))
            {
                return DesignData.Shows.Where(s => s.ProviderId == providerId).FirstOrDefault();
            }

            if (user.Identity.IsAuthenticated && disableCache)
            {
                return await GetShowDetails(providerId);
            }

            var showCacheKey = $"{CacheKey}_Show_{providerId}";
            var result = _cache.Get<Show>(showCacheKey);

            if (result == null)
            {
                result = await GetShowDetails(providerId);

                _cache.Set(showCacheKey, result, new MemoryCacheEntryOptions
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
                var listRequest = client.PlaylistItems.List("snippet");
                listRequest.PlaylistId = _appSettings.YouTubePlaylistId;
                listRequest.MaxResults = 3 * 8;

                var requestStart = DateTimeOffset.UtcNow;
                var playlistItems = await listRequest.ExecuteAsync();
                _telemetry.TrackDependency("YouTube.PlayListItemsApi", "List", requestStart, DateTimeOffset.UtcNow - requestStart, true);

                var result = new ShowList();

                result.Shows = playlistItems.Items.Select(item => new Show
                {
                    Provider = "YouTube",
                    ProviderId = item.Snippet.ResourceId.VideoId,
                    Title = GetUsefulBitsFromTitle(item.Snippet.Title),
                    Description = item.Snippet.Description,
                    ShowDate = DateTimeOffset.Parse(item.Snippet.PublishedAtRaw, null, DateTimeStyles.RoundtripKind),
                    ThumbnailUrl = item.Snippet.Thumbnails.High.Url,
                    Url = GetVideoUrl(item.Snippet.ResourceId.VideoId, item.Snippet.PlaylistId, item.Snippet.Position ?? 0)
                }).ToList();

                if (!string.IsNullOrEmpty(playlistItems.NextPageToken))
                {
                    result.MoreShowsUrl = GetPlaylistUrl(_appSettings.YouTubePlaylistId);
                }

                return result;
            }
        }

        private async Task<Show> GetShowDetails(string providerId)
        {
            using (var client = new YouTubeService(new BaseClientService.Initializer
            {
                ApplicationName = _appSettings.YouTubeApplicationName,
                ApiKey = _appSettings.YouTubeApiKey
            }))
            {
                var videoRequest = client.Videos.List("snippet, recordingDetails");
                videoRequest.Id = providerId;
                var requestStart = DateTimeOffset.UtcNow;
                var videoDetails = await videoRequest.ExecuteAsync();
                _telemetry.TrackDependency("YouTube.PlayListItemsApi", "List", requestStart, DateTimeOffset.UtcNow - requestStart, true);

                var result = videoDetails.Items.Select(item => new Show
                {
                    Provider = "YouTube",
                    ProviderId = item.Id,
                    Title = GetUsefulBitsFromTitle(item.Snippet.Title),
                    Description = item.Snippet.Description,
                    ShowDate = DateTimeOffset.Parse(item.Snippet.PublishedAtRaw, null, DateTimeStyles.RoundtripKind),
                    ThumbnailUrl = item.Snippet.Thumbnails.High.Url,
                    Url = GetVideoUrl(item.Id)
                }).FirstOrDefault();

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

        private static string GetVideoUrl(string id)
        {
            var encodedId = UrlEncoder.Default.Encode(id);

            return $"https://www.youtube.com/watch?v={encodedId}";
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
                    Url = "https://www.youtube.com/watch?v=7O81CAjmOXk&index=1&list=PL0M0zPgJ3HSftTAAHttA3JQU4vOjXFquF"
                },
                new Show
                {
                    ShowDate = new DateTimeOffset(2015, 7, 14, 15, 30, 0, _pdtOffset),
                    Title = "ASP.NET Community Standup - July 14th 2015",
                    Provider = "YouTube",
                    ProviderId = "bFXseBPGAyQ",
                    ThumbnailUrl = "http://img.youtube.com/vi/bFXseBPGAyQ/mqdefault.jpg",
                    Url = "https://www.youtube.com/watch?v=bFXseBPGAyQ&index=2&list=PL0M0zPgJ3HSftTAAHttA3JQU4vOjXFquF"
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
                    Url = "https://www.youtube.com/watch?v=7O81CAjmOXk&index=1&list=PL0M0zPgJ3HSftTAAHttA3JQU4vOjXFquF"
                },
            };

            public static readonly IList<ShowDetails> ShowDetails = new List<ShowDetails>
            {
                new ShowDetails
                {
                    ShowId = "7O81CAjmOXk",
                    Description = "Blah, blah, blah",
                    TableOfContents = new TableOfContents
                    {
                        Groups = new List<TableOfContentsGroup>
                        {
                            new TableOfContentsGroup
                            {
                                GroupHeader = "Pre-show",
                                Items = new List<TableOfContentsItem>
                                {
                                    new TableOfContentsItem
                                    {
                                        Position = TimeSpan.Zero,
                                        Description = "Scott Hanselman talks about..."
                                    },
                                    new TableOfContentsItem
                                    {
                                        Position = new TimeSpan(0, 1, 40),
                                        Description = "Scott Hanselman shows his new 3D printed stuff..."
                                    },
                                    new TableOfContentsItem
                                    {
                                        Position = new TimeSpan(0, 2, 40),
                                        Description = "Damian Edwards talks about..."
                                    }
                                }
                            },
                            new TableOfContentsGroup
                            {
                                GroupHeader = "Welcome",
                                Items = new List<TableOfContentsItem>
                                {
                                    new TableOfContentsItem
                                    {
                                        Position = new TimeSpan(0, 6, 20),
                                        Description = "Welcome by Scott Hanselman..."
                                    }
                                }
                            },
                            new TableOfContentsGroup
                            {
                                GroupHeader = "Community content",
                                Items = new List<TableOfContentsItem>
                                {
                                    new TableOfContentsItem
                                    {
                                        Position = new TimeSpan(0, 7, 50),
                                        Description = "Jon Galloway presents site #1..."
                                    },
                                    new TableOfContentsItem
                                    {
                                        Position = new TimeSpan(0, 8, 0),
                                        Description = "Site #2 about..."
                                    },
                                    new TableOfContentsItem
                                    {
                                        Position = new TimeSpan(0, 8, 10),
                                        Description = "Site #3 about..."
                                    }
                                }
                            },
                            new TableOfContentsGroup
                            {
                                GroupHeader = "Todays standup topic",
                                Items = new List<TableOfContentsItem>
                                {
                                    new TableOfContentsItem
                                    {
                                        Position = new TimeSpan(0, 9, 50),
                                        Description = "Damian Edwards introduces..."
                                    },
                                    new TableOfContentsItem
                                    {
                                        Position = new TimeSpan(0, 10, 40),
                                        Description = "Guest talks about..."
                                    }
                                }
                            },
                            new TableOfContentsGroup
                            {
                                GroupHeader = "Q &amp; A",
                                Items = new List<TableOfContentsItem>
                                {
                                    new TableOfContentsItem
                                    {
                                        Position = new TimeSpan(0, 40, 41),
                                        Description = "Question #1 about..."
                                    },
                                    new TableOfContentsItem
                                    {
                                        Position = new TimeSpan(0, 43, 20),
                                        Description = "Question #2 about..."
                                    },
                                    new TableOfContentsItem
                                    {
                                        Position = new TimeSpan(0, 50, 34),
                                        Description = "Question #3 about..."
                                    }
                                }
                            },
                            new TableOfContentsGroup
                            {
                                GroupHeader = "Wrap up",
                                Items = new List<TableOfContentsItem>
                                {
                                    new TableOfContentsItem
                                    {
                                        Position = new TimeSpan(0, 55, 30),
                                        Description = "Damian Edwards talks about..."
                                    },
                                    new TableOfContentsItem
                                    {
                                        Position = new TimeSpan(0, 57, 24),
                                        Description = "Dramatic zoom out..."
                                    }
                                }
                            }
                    }  // TableOfContent Groups for show 7O81CAjmOXk
                    }  // TableOfContents for show 7O81CAjmOXk
                }  // ShowDetails for show 7O81CAjmOXk
            };  // ShowDetails readonly field
        }  // DesignData
    }
}
