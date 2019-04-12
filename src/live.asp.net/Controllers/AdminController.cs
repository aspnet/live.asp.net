// Copyright (c) .NET Foundation. All rights reserved. 
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using live.asp.net.Models;
using live.asp.net.Services;
using live.asp.net.ViewModels;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace live.asp.net.Controllers
{
    [Route("/admin")]
    [Authorize("Admin")]
    public class AdminController : Controller
    {
        private readonly ILiveShowDetailsService _liveShowDetails;
        private readonly IMemoryCache _memoryCache;
        private readonly AppSettings _appSettings;
        private readonly IHostingEnvironment _env;
        private readonly TelemetryClient _telemetry;
        private readonly IObjectMapper _mapper;

        public AdminController(
            IHostingEnvironment env,
            ILiveShowDetailsService liveShowDetails,
            IMemoryCache memoryCache,
            IOptions<AppSettings> appSettings,
            IObjectMapper mapper,
            TelemetryClient telemetry)
        {
            _liveShowDetails = liveShowDetails;
            _memoryCache = memoryCache;
            _appSettings = appSettings.Value;
            _env = env;
            _mapper = mapper;
            _telemetry = telemetry;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var model = new AdminViewModel
            {
                SuccessMessage = (string)TempData[nameof(AdminViewModel.SuccessMessage)]
            };

            var liveShowDetails = await _liveShowDetails.LoadAsync();
            if (liveShowDetails == null)
            {
                throw new InvalidOperationException("Cannot find live show details.");
            }

            UpdateAdminViewModel(model, liveShowDetails);

            return View(model);
        }

        [ModelMetadataType(typeof(AdminViewModel))]
        public class AdminInputModel
        {
            public string? LiveShowEmbedUrl { get; set; }

            public string? LiveShowHtml { get; set; }

            public DateTime? NextShowDatePst { get; set; }

            public string? AdminMessage { get; set; }
        }

        [HttpPost]
        public async Task<IActionResult> Save(AdminInputModel input)
        {
            var liveShowDetails = await _liveShowDetails.LoadAsync() ?? new LiveShowDetails();

            if (!ModelState.IsValid)
            {
                // Model validation error, just return and let the error render
                var viewModel = new AdminViewModel();
                UpdateAdminViewModel(viewModel, liveShowDetails);

                return View(nameof(Index), viewModel);
            }

            if (!string.IsNullOrEmpty(input.LiveShowEmbedUrl))
            {
                // Convert live show url to HTTPS if need be
                if (input.LiveShowEmbedUrl.StartsWith("http://"))
                {
                    input.LiveShowEmbedUrl = "https://" + input.LiveShowEmbedUrl.Substring("http://".Length);
                }

                // Convert watch URL to embed URL
                if (input.LiveShowEmbedUrl.StartsWith("https://www.youtube.com/watch?v=", StringComparison.OrdinalIgnoreCase))
                {
                    var queryIndex = input.LiveShowEmbedUrl.LastIndexOf("?v=", StringComparison.OrdinalIgnoreCase);
                    var showIdLength = input.LiveShowEmbedUrl.IndexOf('&', queryIndex) - 3 - queryIndex;
                    var showId = showIdLength > 0 ? input.LiveShowEmbedUrl.Substring(queryIndex + 3, showIdLength) : input.LiveShowEmbedUrl.Substring(queryIndex + 3);
                    input.LiveShowEmbedUrl = $"https://www.youtube.com/embed/{showId}";
                }
            }

            TrackShowEvent(input, liveShowDetails);

            _mapper.Map(input, liveShowDetails);
            liveShowDetails.NextShowDateUtc = input.NextShowDatePst?.ConvertFromPtcToUtc();

            await _liveShowDetails.SaveAsync(liveShowDetails);

            TempData[nameof(AdminViewModel.SuccessMessage)] = "Live show details saved successfully!";

            return RedirectToAction(nameof(Index));
        }

        [HttpPost("clearcache")]
        public IActionResult ClearCache()
        {
            _memoryCache.Remove(YouTubeShowsService.CacheKey);

            TempData[nameof(AdminViewModel.SuccessMessage)] = "YouTube cache cleared successfully!";

            return RedirectToAction(nameof(Index));
        }

        private void TrackShowEvent(AdminInputModel input, LiveShowDetails liveShowDetails)
        {
            if (_telemetry.IsEnabled())
            {
                var showStarted = string.IsNullOrEmpty(liveShowDetails.LiveShowEmbedUrl) && !string.IsNullOrEmpty(input.LiveShowEmbedUrl);
                var showEnded = !string.IsNullOrEmpty(liveShowDetails.LiveShowEmbedUrl) && string.IsNullOrEmpty(input.LiveShowEmbedUrl);

                if (showStarted || showEnded)
                {
                    var showEvent = new EventTelemetry(showStarted ? "Show Started" : "Show Ended");
                    showEvent.Properties.Add("Show Embed URL", showStarted ? input.LiveShowEmbedUrl : liveShowDetails.LiveShowEmbedUrl);
                    _telemetry.TrackEvent(showEvent);
                }
            }
        }

        private void UpdateAdminViewModel(AdminViewModel model, LiveShowDetails liveShowDetails)
        {
            _mapper.Map(liveShowDetails, model);
            model.NextShowDatePst = liveShowDetails?.NextShowDateUtc?.ConvertFromUtcToPst();

            var nextTuesday = GetNextTuesday();
            model.NextShowDateSuggestionPstAM = nextTuesday.AddHours(10).ToString("yyyy-MM-ddTHH:mm");
            model.NextShowDateSuggestionPstPM = nextTuesday.AddHours(15).AddMinutes(45).ToString("yyyy-MM-ddTHH:mm");

            model.AppSettings = _appSettings;
            model.EnvironmentName = _env.EnvironmentName;
        }

        private DateTime GetNextTuesday()
        {
            var nowPst = DateTime.UtcNow.ConvertFromUtcToPst();
            var remainingDays = 7 - ((int) nowPst.DayOfWeek + 5) % 7;
            var nextTuesday = nowPst.AddDays(remainingDays);

            return nextTuesday.Date;
        }
    }
}
