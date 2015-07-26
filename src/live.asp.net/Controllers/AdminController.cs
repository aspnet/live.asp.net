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
    [Route("/admin")]
    public class AdminController : Controller
    {
        private static readonly TimeSpan _pstOffset = TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time").BaseUtcOffset;
        private readonly IShowsService _showsService;

        public AdminController(IShowsService showsService)
        {
            _showsService = showsService;
        }

        [HttpGet()]
        [Authorize("Admin")]
        public async Task<IActionResult> Index(bool? useDesignData)
        {
            var model = new AdminViewModel();

            var msg = Context.Request.Cookies["msg"];
            Context.Response.Cookies.Delete("msg");
            model.SuccessMessage = msg == "1" ? "Saved successfully!" : null ;
            model.LiveShowEmbedUrl = await _showsService.GetLiveShowEmbedUrlAsync(useDesignData ?? false);
            var nextShowDateTime = await _showsService.GetNextShowDateTime();
            model.NextShowDate = nextShowDateTime?.DateTime;

            return View(model);
        }

        [HttpPost()]
        [Authorize("Admin")]
        public async Task<IActionResult> Save(AdminViewModel model)
        {
            if (ModelState.IsValid)
            {
                await _showsService.SetLiveShowEmbedUrlAsync(model.LiveShowEmbedUrl);
                DateTimeOffset? nextShowDateTime = null;
                if (model.NextShowDate.HasValue)
                {
                    nextShowDateTime = new DateTimeOffset(model.NextShowDate.Value, _pstOffset);
                }
                await _showsService.SetNextShowDateTime(nextShowDateTime);

                Context.Response.Cookies.Append("msg", "1");

                return RedirectToAction("Index");
            }

            return View("Index", model);
        }
    }
}
