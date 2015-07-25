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
    [Route("/")]
    public class HomeController : Controller
    {
        private readonly IShowsService _showsService;

        public HomeController(IShowsService showsService)
        {
            _showsService = showsService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var showList = await _showsService.GetRecordedShowsAsync();
            return View(new HomeViewModel { PreviousShows = showList.Shows, MoreShowsUrl = showList.MoreShowsUrl });
        }

        [HttpGet("policy")]
        [Authorize()]
        public IActionResult Policy()
        {
            return View();
        }

        [HttpGet("admin")]
        [Authorize("Admin")]
        public IActionResult Admin()
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
