// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AdminController.cs" company=".NET Foundation">
//   Copyright (c) .NET Foundation. All rights reserved.
//   Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace live.asp.net.Controllers
{
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

    /// <summary>
    ///     Class AdminController.
    /// </summary>
    [Route("/admin")]
    public class AdminController : Controller
    {
        #region Constants

        /// <summary>
        ///     Pacific Standard Time
        /// </summary>
        private const string PST = "Pacific Standard Time";

        #endregion

        #region Static Fields

        /// <summary>
        ///     The <see cref="PST" /> time zone
        /// </summary>
        private static readonly TimeZoneInfo PSTTimeZone = TimeZoneInfo.FindSystemTimeZoneById(PST);

        /// <summary>
        ///     The <see cref="PST" /> offset
        /// </summary>
        private static readonly TimeSpan PSTOffset = PSTTimeZone.BaseUtcOffset;

        #endregion

        #region Fields

        /// <summary>
        ///     The application settings
        /// </summary>
        private readonly AppSettings appSettings;

        /// <summary>
        ///     The environment.
        /// </summary>
        private readonly IHostingEnvironment env;

        /// <summary>
        ///     The live show details
        /// </summary>
        private readonly ILiveShowDetailsService liveShowDetails;

        /// <summary>
        ///     The memory cache
        /// </summary>
        private readonly IMemoryCache memoryCache;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="AdminController"/> class.
        /// </summary>
        /// <param name="env">
        /// The env.
        /// </param>
        /// <param name="liveShowDetails">
        /// The live show details.
        /// </param>
        /// <param name="memoryCache">
        /// The memory cache.
        /// </param>
        /// <param name="appSettings">
        /// The application settings.
        /// </param>
        public AdminController(
            IHostingEnvironment env,
            ILiveShowDetailsService liveShowDetails,
            IMemoryCache memoryCache,
            IOptions<AppSettings> appSettings)
        {
            this.liveShowDetails = liveShowDetails;
            this.memoryCache = memoryCache;
            this.appSettings = appSettings.Options;
            this.env = env;
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        ///     POST: /Admin/ClearCache
        /// </summary>
        /// <returns>An <see cref="IActionResult" />.</returns>
        [HttpPost("clearcache")]
        public IActionResult ClearCache()
        {
            this.memoryCache.Remove(YouTubeShowsService.CacheKey);

            this.Context.Response.Cookies.Append("msg", "2");

            return this.RedirectToAction(nameof(this.Index));
        }

        /// <summary>
        ///     GET: /Admin/
        ///     GET: /Admin/Index
        /// </summary>
        /// <returns>A <see cref="Task" /> of <see cref="IActionResult" />.</returns>
        [HttpGet]
        [Authorize("Admin")]
        public async Task<IActionResult> Index()
        {
            const string Key = "msg";
            var model = new AdminViewModel();

            var message = this.Context.Request.Cookies[Key];
            this.Context.Response.Cookies.Delete(Key);

            switch (message)
            {
                case "1":
                    model.SuccessMessage = "Live show details saved successfully!";
                    break;
                case "2":
                    model.SuccessMessage = "YouTube cache cleared successfully!";
                    break;
            }

            var loadedLiveShowDetails = await this.liveShowDetails.LoadAsync();

            this.UpdateAdminViewModel(model, loadedLiveShowDetails);

            return this.View(model);
        }

        /// <summary>
        /// POST: /Admin/Save
        /// </summary>
        /// <param name="model">
        /// The <paramref name="model"/>.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> of <see cref="IActionResult"/>.
        /// </returns>
        [HttpPost]
        [Authorize("Admin")]
        public async Task<IActionResult> Save(AdminViewModel model)
        {
            LiveShowDetails loadedLiveShowDetails;

            if (!this.ModelState.IsValid)
            {
                // Model validation error, just return and let the error render
                loadedLiveShowDetails = await this.liveShowDetails.LoadAsync();
                this.UpdateAdminViewModel(model, loadedLiveShowDetails);

                return this.View(nameof(this.Index), model);
            }

            if (!string.IsNullOrEmpty(model.LiveShowEmbedUrl)
                && model.LiveShowEmbedUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase))
            {
                model.LiveShowEmbedUrl = "https://" + model.LiveShowEmbedUrl.Substring("http://".Length);
            }

            loadedLiveShowDetails = new LiveShowDetails
                                        {
                                            LiveShowEmbedUrl = model.LiveShowEmbedUrl,
                                            NextShowDateUtc =
                                                model.NextShowDatePst.HasValue
                                                    ? TimeZoneInfo.ConvertTimeToUtc(
                                                        model.NextShowDatePst.Value,
                                                        PSTTimeZone)
                                                    : (DateTime?)null,
                                            AdminMessage = model.AdminMessage
                                        };

            await this.liveShowDetails.SaveAsync(loadedLiveShowDetails);

            this.Context.Response.Cookies.Append("msg", "1");

            return this.RedirectToAction(nameof(this.Index));
        }

        #endregion

        #region Methods

        /// <summary>
        /// Updates the admin view <paramref name="model"/>.
        /// </summary>
        /// <param name="model">
        /// The <paramref name="model"/>.
        /// </param>
        /// <param name="theLiveShowDetails">
        /// The live show details.
        /// </param>
        private void UpdateAdminViewModel(AdminViewModel model, LiveShowDetails theLiveShowDetails)
        {
            model.LiveShowEmbedUrl = theLiveShowDetails?.LiveShowEmbedUrl;
            if (theLiveShowDetails?.NextShowDateUtc != null)
            {
                var nextShowDatePst = TimeZoneInfo.ConvertTimeFromUtc(theLiveShowDetails.NextShowDateUtc.Value, PSTTimeZone);
                model.NextShowDatePst = nextShowDatePst;
            }

            model.AdminMessage = theLiveShowDetails?.AdminMessage;
            model.AppSettings = this.appSettings;
            model.EnvironmentName = this.env.EnvironmentName;
        }

        #endregion
    }
}