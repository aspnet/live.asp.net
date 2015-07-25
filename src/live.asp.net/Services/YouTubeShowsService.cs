using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using live.asp.net.Models;
using Microsoft.AspNet.Hosting;
using Microsoft.AspNet.Http;

namespace live.asp.net.Services
{
    public class YouTubeShowsService : IShowsService
    {
        private static readonly TimeSpan _pdtOffset = TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time").BaseUtcOffset;

        private static readonly Show _liveShow = new Show
        {
            ShowDate = DateTimeOffset.Now.AddHours(1),
            Title = "Upcoming Show!",
            Description = "We'll talk about things"
        };

        private static readonly IList<Show> _shows = new List<Show>
        {
            new Show
            {
                ShowDate = new DateTimeOffset(2015, 7, 21, 9, 30, 0, _pdtOffset),
                Title = "ASP.NET Community Standup - July 21st 2015",
                Provider = "YouTube",
                ProviderId = "7O81CAjmOXk",
                ThumbnailUrl = "http://img.youtube.com/vi/7O81CAjmOXk/mqdefault.jpg",
                Url = "https://www.youtube.com/watch?v=7O81CAjmOXk&index=1&list=PL0M0zPgJ3HSftTAAHttA3JQU4vOjXFquF",
                ShowNotesUrl = "http://blogs.msdn.com/b/webdev/archive/2015/07/23/asp-net-community-standup-july-21-2015.aspx"
            },
            new Show
            {
                ShowDate = new DateTimeOffset(2015, 7, 14, 15, 30, 0, _pdtOffset),
                Title = "ASP.NET Community Standup - July 14th 2015",
                Provider = "YouTube",
                ProviderId = "bFXseBPGAyQ",
                ThumbnailUrl = "http://img.youtube.com/vi/bFXseBPGAyQ/mqdefault.jpg",
                Url = "https://www.youtube.com/watch?v=bFXseBPGAyQ&index=2&list=PL0M0zPgJ3HSftTAAHttA3JQU4vOjXFquF",
                ShowNotesUrl = "http://blogs.msdn.com/b/webdev/archive/2015/07/14/asp-net-community-standup-july-14-2015.aspx"
            },

            new Show
            {
                ShowDate = new DateTimeOffset(2015, 7, 7, 15, 30, 0, _pdtOffset),
                Title = "ASP.NET Community Standup - July 7th 2015",
                Provider = "YouTube",
                ProviderId = "APagQ1CIVGA",
                ThumbnailUrl = "http://img.youtube.com/vi/APagQ1CIVGA/mqdefault.jpg",
                Url = "https://www.youtube.com/watch?v=APagQ1CIVGA&index=3&list=PL0M0zPgJ3HSftTAAHttA3JQU4vOjXFquF",
                ShowNotesUrl = "http://blogs.msdn.com/b/webdev/archive/2015/07/07/asp-net-community-standup-july-7-2015.aspx"
            },
            new Show
            {
                ShowDate = DateTimeOffset.Now.AddDays(-28),
                Title = "ASP.NET Community Standup - July 21st 2015",
                Provider = "YouTube",
                ProviderId = "7O81CAjmOXk",
                ThumbnailUrl = "http://img.youtube.com/vi/7O81CAjmOXk/mqdefault.jpg",
                Url = "https://www.youtube.com/watch?v=7O81CAjmOXk&index=1&list=PL0M0zPgJ3HSftTAAHttA3JQU4vOjXFquF",
                ShowNotesUrl = "http://blogs.msdn.com/b/webdev/archive/2015/07/23/asp-net-community-standup-july-21-2015.aspx"
            },
        };

        private readonly IHostingEnvironment _env;
        private readonly HttpRequest _request;

        public YouTubeShowsService(IHostingEnvironment env, IHttpContextAccessor httpContextAccessor)
        {
            _env = env;
            _request = httpContextAccessor.HttpContext.Request;
        }

        public Task<Show> GetLiveShowAsync()
        {
            if (UseDevData())
            {
                return Task.FromResult(_liveShow);
            }

            return Task.FromResult((Show)null);
        }

        public Task<IList<Show>> GetRecordedShowsAsync()
        {
            if (UseDevData())
            {
                return Task.FromResult((IList<Show>)Enumerable.Empty<Show>().ToList());
            }

            return Task.FromResult(_shows);
        }

        private bool UseDevData()
        {
            return !_env.IsDevelopment() && _request.Query.ContainsKey("useTestData")
                || _env.IsDevelopment() && _request.Query.ContainsKey("useTestData") && _request.Query["useTestData"] == "0";
        }
    }
}
