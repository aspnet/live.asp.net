using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using live.asp.net.Data;
using live.asp.net.Services;
using live.asp.net.ViewModels;
using Microsoft.AspNet.Authorization;
using Microsoft.AspNet.Mvc;
using Microsoft.Data.Entity;

namespace live.asp.net.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _db;
        private readonly IShowsService _showsService;

        public HomeController(IShowsService showsService, AppDbContext dbContext)
        {
            _showsService = showsService;
            _db = dbContext;
        }

        [Route("/")]
        public async Task<IActionResult> Index(bool? disableCache, bool? useDesignData)
        {
            var liveShowDetails = await _db.LiveShowDetails.FirstOrDefaultAsync();
            var showList = await _showsService.GetRecordedShowsAsync(User, disableCache ?? false, useDesignData ?? false);

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
