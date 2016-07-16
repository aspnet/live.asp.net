using live.asp.net.Models;
using System.Threading.Tasks;

namespace live.asp.net.Services
{
    public interface IShowDetailsService
    {
        Task<ShowDetails> LoadAsync(string showId);
        Task SaveAsync(ShowDetails showDetails);
        Task DeleteAsync(string showId);
    }
}