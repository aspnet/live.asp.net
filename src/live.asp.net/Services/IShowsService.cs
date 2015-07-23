using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using live.asp.net.Models;

namespace live.asp.net.Services
{
    public interface IShowsService
    {
        Task<Show> GetLiveShowAsync();

        Task<IList<Show>> GetRecordedShowsAsync();
    }
}
