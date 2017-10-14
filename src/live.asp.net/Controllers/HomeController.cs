// Copyright (c) .NET Foundation. All rights reserved. 
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using live.asp.net.Models;
using live.asp.net.Services;
using live.asp.net.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace live.asp.net.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILiveShowDetailsService _liveShowDetails;
        private readonly IShowsService _showsService;
        private readonly IObjectMapper _mapper;
        private readonly AppSettings _appSettings;

        public HomeController(
            IShowsService showsService,
            ILiveShowDetailsService liveShowDetails,
            IObjectMapper mapper,
            IOptions<AppSettings> appSettings)
        {
            _showsService = showsService;
            _liveShowDetails = liveShowDetails;
            _mapper = mapper;
            _appSettings = appSettings.Value;
        }

        [Route("/")]
        public async Task<IActionResult> Index(bool? disableCache)
        {
            var liveShowDetails = await _liveShowDetails.LoadAsync();
            var showList = await _showsService.GetRecordedShowsAsync(User, disableCache ?? false);

            var homeViewModel = new HomeViewModel();
            _mapper.Map(liveShowDetails, homeViewModel);
            _mapper.Map(showList, homeViewModel);
            homeViewModel.YouTubeChannelId = _appSettings.YouTubeChannelId;

            return View(homeViewModel);
        }

        [HttpGet("/ical")]
        [Produces("text/calendar")]
        public async Task<LiveShowDetails> GetiCal()
        {
            var liveShowDetails = await _liveShowDetails.LoadAsync();

            return liveShowDetails;
        }

        [Route("/error")]
        public IActionResult Error()
        {
            return View();
        }
    }
}
