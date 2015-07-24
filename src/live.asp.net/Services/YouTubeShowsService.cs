using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using live.asp.net.Models;

namespace live.asp.net.Services
{
    public class YouTubeShowsService : IShowsService
    {
        public Task<Show> GetLiveShowAsync()
        {
            return Task.FromResult((Show)null);
        }

        public Task<IEnumerable<Show>> GetRecordedShowsAsync()
        {
            return Task.FromResult(Enumerable.Empty<Show>());
        }
    }
}
