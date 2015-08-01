// Copyright (c) .NET Foundation. All rights reserved. 
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using live.asp.net.Models;
using live.asp.net.Services;
using live.asp.net.ViewModels;
using Microsoft.AspNet.Authorization;
using Microsoft.AspNet.Hosting;
using Microsoft.AspNet.Mvc;
using Microsoft.Framework.Caching.Memory;
using Microsoft.Framework.OptionsModel;

namespace live.asp.net.Controllers
{
    [Route("/admin")]
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
            _appSettings = appSettings.Options;
            _env = env;
        }

        [HttpGet()]
        [Authorize("Admin")]
        public async Task<IActionResult> Index(bool? useDesignData)
        {
            var model = new AdminViewModel();

            var msg = Context.Request.Cookies["msg"];
            Context.Response.Cookies.Delete("msg");

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

            model.LiveShowEmbedUrl = liveShowDetails?.LiveShowEmbedUrl;
            if (liveShowDetails?.NextShowDateUtc != null)
            {
                var nextShowDatePst = TimeZoneInfo.ConvertTimeFromUtc(
                    liveShowDetails.NextShowDateUtc.Value,
                    _pstTimeZone);
                model.NextShowDatePst = nextShowDatePst;
            }
            model.AdminMessage = liveShowDetails?.AdminMessage;
            model.AppSettings = _appSettings;
            model.EnvironmentName = _env.EnvironmentName;

            return View(model);
        }

        [HttpPost()]
        [Authorize("Admin")]
        public async Task<IActionResult> Save(AdminViewModel model)
        {
            if (ModelState.IsValid)
            {
                var liveShowDetails = new LiveShowDetails();

                if (!string.IsNullOrEmpty(model.LiveShowEmbedUrl) && model.LiveShowEmbedUrl.StartsWith("http://"))
                {
                    model.LiveShowEmbedUrl = "https://" + model.LiveShowEmbedUrl.Substring("http://".Length);
                }

                liveShowDetails.LiveShowEmbedUrl = model.LiveShowEmbedUrl;
                liveShowDetails.NextShowDateUtc = model.NextShowDatePst.HasValue
                    ? TimeZoneInfo.ConvertTimeToUtc(model.NextShowDatePst.Value, _pstTimeZone)
                    : (DateTime?)null;
                liveShowDetails.AdminMessage = model.AdminMessage;

                await _liveShowDetails.SaveAsync(liveShowDetails);

                Context.Response.Cookies.Append("msg", "1");

                return RedirectToAction("Index");
            }

            return View("Index", model);
        }

        [HttpPost("clearcache")]
        public IActionResult ClearCache()
        {
            _memoryCache.Remove(YouTubeShowsService.CacheKey);

            Context.Response.Cookies.Append("msg", "2");

            return RedirectToAction("Index");
        }
    }
}
