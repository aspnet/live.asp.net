using System.Security.Claims;
using System.Threading.Tasks;

namespace live.asp.net.Services
{
    public interface IShowsService
    {
        Task<ShowList> GetRecordedShowsAsync(ClaimsPrincipal user, bool disableCache, bool useDesignData);
    }
}
