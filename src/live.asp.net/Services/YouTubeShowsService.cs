// --------------------------------------------------------------------------------------------------------------------
// <copyright file="YouTubeShowsService.cs" company=".NET Foundation">
//   Copyright (c) .NET Foundation. All rights reserved.
//   Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace live.asp.net.Services
{
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

    /// <summary>
    ///     The YouTube shows service
    /// </summary>
    public class YouTubeShowsService : IShowsService
    {
        #region Constants

        /// <summary>
        ///     The <see cref="cache" /> key
        /// </summary>
        public const string CacheKey = nameof(YouTubeShowsService);

        #endregion

        #region Fields

        /// <summary>
        ///     The application settings
        /// </summary>
        private readonly AppSettings appSettings;

        /// <summary>
        ///     The cache.
        /// </summary>
        private readonly IMemoryCache cache;

        /// <summary>
        ///     The environment
        /// </summary>
        private readonly IHostingEnvironment env;

        /// <summary>
        ///     The telemetry client.
        /// </summary>
        private readonly TelemetryClient telemetry;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="YouTubeShowsService"/> class.
        /// </summary>
        /// <param name="env">
        /// The env.
        /// </param>
        /// <param name="appSettings">
        /// The application settings.
        /// </param>
        /// <param name="memoryCache">
        /// The memory cache.
        /// </param>
        /// <param name="telemetry">
        /// The telemetry.
        /// </param>
        public YouTubeShowsService(
            IHostingEnvironment env,
            IOptions<AppSettings> appSettings,
            IMemoryCache memoryCache,
            TelemetryClient telemetry)
        {
            this.env = env;
            this.appSettings = appSettings.Options;
            this.cache = memoryCache;
            this.telemetry = telemetry;
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// Gets the recorded shows asynchronously.
        /// </summary>
        /// <param name="user">
        /// The <paramref name="user"/>.
        /// </param>
        /// <param name="disableCache">
        /// A value indicating whether to disable the <see cref="cache"/>.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> of <see cref="ShowList"/>.
        /// </returns>
        public async Task<ShowList> GetRecordedShowsAsync(ClaimsPrincipal user, bool disableCache)
        {
            if (string.IsNullOrEmpty(this.appSettings.YouTubeApiKey))
            {
                return new ShowList { Shows = DesignData.Shows };
            }

            if (user.Identity.IsAuthenticated && disableCache)
            {
                return await this.GetShowsList();
            }

            var result = this.cache.Get<ShowList>(CacheKey);

            if (result == null)
            {
                result = await this.GetShowsList();

                this.cache.Set(
                    CacheKey,
                    result,
                    new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(1) });
            }

            return result;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the play list URL.
        /// </summary>
        /// <param name="playlistId">
        /// The play list identifier.
        /// </param>
        /// <returns>
        /// The play list URL.
        /// </returns>
        private static string GetPlaylistUrl(string playlistId)
        {
            var encodedPlaylistId = UrlEncoder.Default.UrlEncode(playlistId);

            return $"https://www.youtube.com/playlist?list={encodedPlaylistId}";
        }

        /// <summary>
        /// Gets the useful bits from <paramref name="title"/>.
        /// </summary>
        /// <param name="title">
        /// The <paramref name="title"/>.
        /// </param>
        /// <returns>
        /// The useful bits.
        /// </returns>
        private static string GetUsefulBitsFromTitle(string title)
        {
            if (title.Count(c => c == '-') < 2)
            {
                return string.Empty;
            }

            var lastHyphen = title.LastIndexOf('-');
            if (lastHyphen < 0)
            {
                return string.Empty;
            }

            var result = title.Substring(lastHyphen + 1);
            return result;
        }

        /// <summary>
        /// Gets the video URL.
        /// </summary>
        /// <param name="id">
        /// The identifier.
        /// </param>
        /// <param name="playlistId">
        /// The play list identifier.
        /// </param>
        /// <param name="itemIndex">
        /// Index of the item.
        /// </param>
        /// <returns>
        /// The video URL.
        /// </returns>
        private static string GetVideoUrl(string id, string playlistId, long itemIndex)
        {
            var encodedId = UrlEncoder.Default.UrlEncode(id);
            var encodedPlaylistId = UrlEncoder.Default.UrlEncode(playlistId);
            var encodedItemIndex = UrlEncoder.Default.UrlEncode(itemIndex.ToString());

            return $"https://www.youtube.com/watch?v={encodedId}&list={encodedPlaylistId}&index={encodedItemIndex}";
        }

        /// <summary>
        ///     Gets the shows list.
        /// </summary>
        /// <returns>A <see cref="Task"/> of <see cref="ShowList"/>.</returns>
        private async Task<ShowList> GetShowsList()
        {
            using (
                var client =
                    new YouTubeService(
                        new BaseClientService.Initializer
                            {
                                ApplicationName = this.appSettings.YouTubeApplicationName,
                                ApiKey = this.appSettings.YouTubeApiKey
                            }))
            {
                var listRequest = client.PlaylistItems.List("snippet");
                listRequest.PlaylistId = this.appSettings.YouTubePlaylistId;
                listRequest.MaxResults = 3 * 8;

                var requestStart = DateTimeOffset.UtcNow;
                var playlistItems = await listRequest.ExecuteAsync();
                this.telemetry.TrackDependency(
                    "YouTube.PlayListItemsApi",
                    "List",
                    requestStart,
                    DateTimeOffset.UtcNow - requestStart,
                    true);

                var result = new ShowList
                                 {
                                     Shows =
                                         playlistItems.Items.Select(
                                             item =>
                                             new Show
                                                 {
                                                     Provider = "YouTube",
                                                     ProviderId = item.Snippet.ResourceId.VideoId,
                                                     Title = GetUsefulBitsFromTitle(item.Snippet.Title),
                                                     Description = item.Snippet.Description,
                                                     ShowDate =
                                                         DateTimeOffset.Parse(
                                                             item.Snippet.PublishedAtRaw,
                                                             null,
                                                             DateTimeStyles.RoundtripKind),
                                                     ThumbnailUrl = item.Snippet.Thumbnails.High.Url,
                                                     Url =
                                                         GetVideoUrl(
                                                             item.Snippet.ResourceId.VideoId,
                                                             item.Snippet.PlaylistId,
                                                             item.Snippet.Position ?? 0)
                                                 }).ToList()
                                 };

                if (!string.IsNullOrEmpty(playlistItems.NextPageToken))
                {
                    result.MoreShowsUrl = GetPlaylistUrl(this.appSettings.YouTubePlaylistId);
                }

                return result;
            }
        }

        #endregion

        /// <summary>
        ///     The design data class.
        /// </summary>
        private static class DesignData
        {
            #region Static Fields

            /// <summary>
            ///     The live show
            /// </summary>
            public static readonly string LiveShow = null;

            /// <summary>
            ///     The PDT offset
            /// </summary>
            private static readonly TimeSpan PdtOffset = TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time").BaseUtcOffset;

            /// <summary>
            ///     The shows
            /// </summary>
            public static readonly IList<Show> Shows = new List<Show>
                                                           {
                                                               new Show
                                                                   {
                                                                       ShowDate = new DateTimeOffset(2015, 7, 21, 9, 30, 0, PdtOffset),
                                                                       Title = "ASP.NET Community Standup - July 21st 2015",
                                                                       Provider = "YouTube",
                                                                       ProviderId = "7O81CAjmOXk",
                                                                       ThumbnailUrl = "http://img.youtube.com/vi/7O81CAjmOXk/mqdefault.jpg",
                                                                       Url = "https://www.youtube.com/watch?v=7O81CAjmOXk&index=1&list=PL0M0zPgJ3HSftTAAHttA3JQU4vOjXFquF"
                                                                   },
                                                               new Show
                                                                   {
                                                                       ShowDate = new DateTimeOffset(2015, 7, 14, 15, 30, 0, PdtOffset),
                                                                       Title = "ASP.NET Community Standup - July 14th 2015",
                                                                       Provider = "YouTube",
                                                                       ProviderId = "bFXseBPGAyQ",
                                                                       ThumbnailUrl = "http://img.youtube.com/vi/bFXseBPGAyQ/mqdefault.jpg",
                                                                       Url = "https://www.youtube.com/watch?v=bFXseBPGAyQ&index=2&list=PL0M0zPgJ3HSftTAAHttA3JQU4vOjXFquF"
                                                                   },
                                                               new Show
                                                                   {
                                                                       ShowDate = new DateTimeOffset(2015, 7, 7, 15, 30, 0, PdtOffset),
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

            #endregion
        }
    }
}
