// Copyright (c) .NET Foundation. All rights reserved. 
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using live.asp.net.Models;
using live.asp.net.Services;
using live.asp.net.ViewModels;
using Microsoft.AspNetCore.Mvc;

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
            var homeViewModel = new HomeViewModel();
            await _liveShowDetails.LoadAsync(homeViewModel);
            await _showsService.PopulateRecordedShowsAsync(homeViewModel, User, disableCache ?? false);

            return View(homeViewModel);
        }

        [HttpGet("/ical")]
        [Produces("text/calendar")]
        public async Task<ILiveShowDetails> GetiCal()
        {
            ILiveShowDetails liveShowDetails = new HomeViewModel();

            await _liveShowDetails.LoadAsync(liveShowDetails);

            return liveShowDetails;
        }

        [Route("/error")]
        public IActionResult Error()
        {
            return View();
        }
    }
}
