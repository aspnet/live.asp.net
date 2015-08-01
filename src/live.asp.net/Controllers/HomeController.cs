using System.Threading.Tasks;
using live.asp.net.Services;
using live.asp.net.ViewModels;
using Microsoft.AspNet.Mvc;

namespace live.asp.net.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILiveShowDetailsService _liveShowDetails;
        private readonly IShowsService _showsService;

        public HomeController(IShowsService showsService, ILiveShowDetailsService liveShowDetails)
        {
            _showsService = showsService;
            _liveShowDetails = liveShowDetails;
        }

        [Route("/")]
        public async Task<IActionResult> Index(bool? disableCache)
        {
            var liveShowDetails = await _liveShowDetails.LoadAsync();
            var showList = await _showsService.GetRecordedShowsAsync(User, disableCache ?? false);

            return View(new HomeViewModel
            {
                AdminMessage = liveShowDetails?.AdminMessage,
                NextShowDateUtc = liveShowDetails?.NextShowDateUtc,
                LiveShowEmbedUrl = liveShowDetails?.LiveShowEmbedUrl,
                PreviousShows = showList.Shows,
                MoreShowsUrl = showList.MoreShowsUrl
            });
        }

        [HttpGet("error")]
        public IActionResult Error()
        {
            return View();
        }
    }
}
