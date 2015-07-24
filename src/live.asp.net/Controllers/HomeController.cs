using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using live.asp.net.Services;
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
            return View(await _showsService.GetRecordedShowsAsync());
        }

        [HttpGet("error")]
        public IActionResult Error()
        {
            return View();
        }
    }
}
