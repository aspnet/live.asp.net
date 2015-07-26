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

namespace live.asp.net.Controllers
{
    [Route("/admin")]
    public class AdminController : Controller
    {
        private static readonly TimeSpan _pstOffset = TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time").BaseUtcOffset;
        private readonly AppDbContext _db;

        public AdminController(AppDbContext dbContext)
        {
            _db = dbContext;
        }

        [HttpGet()]
        [Authorize("Admin")]
        public async Task<IActionResult> Index(bool? useDesignData)
        {
            var model = new AdminViewModel();

            var msg = Context.Request.Cookies["msg"];
            Context.Response.Cookies.Delete("msg");
            model.SuccessMessage = msg == "1" ? "Saved successfully!" : null ;

            var liveShowDetails = await _db.LiveShowDetails.FirstOrDefaultAsync();

            model.LiveShowEmbedUrl = liveShowDetails?.LiveShowEmbedUrl;
            model.NextShowDate = liveShowDetails?.NextShowDate;

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

                await _db.SaveChangesAsync();

                Context.Response.Cookies.Append("msg", "1");

                return RedirectToAction("Index");
            }

            return View("Index", model);
        }
    }
}
