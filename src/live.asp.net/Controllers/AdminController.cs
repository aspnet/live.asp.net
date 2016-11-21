// Copyright (c) .NET Foundation. All rights reserved. 
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using live.asp.net.Models;
using live.asp.net.Services;
using live.asp.net.ViewModels;
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
        private const string PST = "Pacific Standard Time";
        private static readonly TimeZoneInfo _pstTimeZone = TimeZoneInfo.FindSystemTimeZoneById(PST);
        private static readonly TimeSpan _pstOffset = _pstTimeZone.BaseUtcOffset;
        private readonly ILiveShowDetailsService _liveShowDetails;
        private readonly IMemoryCache _memoryCache;
        private readonly AppSettings _appSettings;
        private readonly IHostingEnvironment _env;

        public AdminController(
            IHostingEnvironment env,
            ILiveShowDetailsService liveShowDetails,
            IMemoryCache memoryCache,
            IOptions<AppSettings> appSettings)
        {
            _liveShowDetails = liveShowDetails;
            _memoryCache = memoryCache;
            _appSettings = appSettings.Value;
            _env = env;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var model = new AdminViewModel();

            var msg = HttpContext.Request.Cookies["msg"];
            HttpContext.Response.Cookies.Delete("msg");

            switch (msg)
            {
                case "1":
                    model.SuccessMessage = "Live show details saved successfully!";
                    break;
                case "2":
                    model.SuccessMessage = "YouTube cache cleared successfully!";
                    break;
            }

            var liveShowDetails = await _liveShowDetails.LoadAsync();

            UpdateAdminViewModel(model, liveShowDetails);

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Save(AdminViewModel model)
        {
            LiveShowDetails liveShowDetails;

            if (!ModelState.IsValid)
            {
                // Model validation error, just return and let the error render
                liveShowDetails = await _liveShowDetails.LoadAsync();
                UpdateAdminViewModel(model, liveShowDetails);

                return View("Index", model);
            }

            if (!string.IsNullOrEmpty(model.LiveShowEmbedUrl) && model.LiveShowEmbedUrl.StartsWith("http://"))
            {
                model.LiveShowEmbedUrl = "https://" + model.LiveShowEmbedUrl.Substring("http://".Length);
            }

            liveShowDetails = new LiveShowDetails();
            liveShowDetails.LiveShowEmbedUrl = model.LiveShowEmbedUrl;
            liveShowDetails.LiveShowHtml = model.LiveShowHtml;
            liveShowDetails.NextShowDateUtc = model.NextShowDatePst.HasValue
                ? TimeZoneInfo.ConvertTime(model.NextShowDatePst.Value, _pstTimeZone, TimeZoneInfo.Utc)
                : (DateTime?)null;
            liveShowDetails.AdminMessage = model.AdminMessage;

            await _liveShowDetails.SaveAsync(liveShowDetails);

            HttpContext.Response.Cookies.Append("msg", "1");

            return RedirectToAction("Index");
        }

        [HttpPost("clearcache")]
        public IActionResult ClearCache()
        {
            _memoryCache.Remove(YouTubeShowsService.CacheKey);

            HttpContext.Response.Cookies.Append("msg", "2");

            return RedirectToAction("Index");
        }

        private void UpdateAdminViewModel(AdminViewModel model, LiveShowDetails liveShowDetails)
        {
            model.LiveShowEmbedUrl = liveShowDetails?.LiveShowEmbedUrl;
            model.LiveShowHtml = liveShowDetails?.LiveShowHtml;
            if (liveShowDetails?.NextShowDateUtc != null)
            {
                var nextShowDatePst = TimeZoneInfo.ConvertTime(
                    liveShowDetails.NextShowDateUtc.Value,
                    TimeZoneInfo.Utc,
                    _pstTimeZone);
                model.NextShowDatePst = nextShowDatePst;
            }
            model.AdminMessage = liveShowDetails?.AdminMessage;
            var nextTuesday = GetNextTuesday();
            model.NextShowDateSuggestionPstAM = nextTuesday.AddHours(10).ToString("MM/dd/yyyy HH:mm");
            model.NextShowDateSuggestionPstPM = nextTuesday.AddHours(15).AddMinutes(45).ToString("MM/dd/yyyy HH:mm");
            model.AppSettings = _appSettings;
            model.EnvironmentName = _env.EnvironmentName;
        }

        private DateTime GetNextTuesday()
        {
            var nowPst = TimeZoneInfo.ConvertTime(DateTime.UtcNow, TimeZoneInfo.Utc, _pstTimeZone);
            var dayOfWeek = (int)nowPst.DayOfWeek;
            return nowPst.AddDays(dayOfWeek == 2 ? 7 : (dayOfWeek < 2 ? 2 - dayOfWeek : 9 - dayOfWeek));
        }
    }
}
