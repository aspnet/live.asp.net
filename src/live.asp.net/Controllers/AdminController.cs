using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using live.asp.net.Data;
using live.asp.net.Models;
using live.asp.net.Services;
using live.asp.net.ViewModels;
using Microsoft.AspNet.Authorization;
using Microsoft.AspNet.Mvc;
using Microsoft.Data.Entity;
using Microsoft.Framework.Caching.Memory;

namespace live.asp.net.Controllers
{
    [Route("/admin")]
    public class AdminController : Controller
    {
        private static readonly TimeSpan _pstOffset = TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time").BaseUtcOffset;
        private readonly AppDbContext _db;
        private readonly IMemoryCache _memoryCache;

        public AdminController(AppDbContext dbContext, IMemoryCache memoryCache)
        {
            _db = dbContext;
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

            var liveShowDetails = await _db.LiveShowDetails.FirstOrDefaultAsync();

            model.LiveShowEmbedUrl = liveShowDetails?.LiveShowEmbedUrl;
            model.NextShowDate = liveShowDetails?.NextShowDate;
            model.AdminMessage = liveShowDetails?.AdminMessage;

            return View(model);
        }

        [HttpPost()]
        [Authorize("Admin")]
        public async Task<IActionResult> Save(AdminViewModel model)
        {
            if (ModelState.IsValid)
            {
                var liveShowDetails = await _db.LiveShowDetails.FirstOrDefaultAsync();

                if (liveShowDetails == null)
                {
                    liveShowDetails = new LiveShowDetails();
                    _db.LiveShowDetails.Add(liveShowDetails);
                }

                if (!string.IsNullOrEmpty(model.LiveShowEmbedUrl) && model.LiveShowEmbedUrl.StartsWith("http://"))
                {
                    model.LiveShowEmbedUrl = "https://" + model.LiveShowEmbedUrl.Substring("http://".Length);
                }

                liveShowDetails.LiveShowEmbedUrl = model.LiveShowEmbedUrl;
                liveShowDetails.NextShowDate = model.NextShowDate;
                liveShowDetails.AdminMessage = model.AdminMessage;

                await _db.SaveChangesAsync();

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
