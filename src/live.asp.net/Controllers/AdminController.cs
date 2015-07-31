using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using live.asp.net.Models;
using live.asp.net.Services;
using live.asp.net.ViewModels;
using Microsoft.AspNet.Authorization;
using Microsoft.AspNet.Mvc;
using Microsoft.Framework.Caching.Memory;

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

        public AdminController(ILiveShowDetailsService liveShowDetails, IMemoryCache memoryCache)
        {
            _liveShowDetails = liveShowDetails;
            _memoryCache = memoryCache;
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
