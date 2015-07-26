using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using live.asp.net.Services;
using live.asp.net.ViewModels;
using Microsoft.AspNet.Authorization;
using Microsoft.AspNet.Mvc;

namespace live.asp.net.Controllers
{
    public class HomeController : Controller
    {
        private readonly IShowsService _showsService;

        public HomeController(IShowsService showsService)
        {
            _showsService = showsService;
        }

        [Route("/")]
        public async Task<IActionResult> Index(bool? disableCache, bool? useDesignData)
        {
            var showList = await _showsService.GetRecordedShowsAsync(User, disableCache ?? false, useDesignData ?? false);

            return View(new HomeViewModel
            {
                NextShowDate = await _showsService.GetNextShowDateTime(),
                LiveShowEmbedUrl = await _showsService.GetLiveShowEmbedUrlAsync(useDesignData ?? false),
                PreviousShows = showList.Shows,
                MoreShowsUrl = showList.MoreShowsUrl
            });
        }

        [HttpGet("policy")]
        [Authorize()]
        public IActionResult Policy()
        {
            return View();
        }

        [HttpGet("error")]
        public IActionResult Error()
        {
            return View();
        }
    }
}
