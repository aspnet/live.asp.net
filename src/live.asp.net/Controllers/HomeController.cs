// --------------------------------------------------------------------------------------------------------------------
// <copyright file="HomeController.cs" company=".NET Foundation">
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// </copyright>
// <summary>
//   Class HomeController.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace live.asp.net.Controllers
{
    using System.Threading.Tasks;

    using live.asp.net.Models;
    using live.asp.net.Services;
    using live.asp.net.ViewModels;

    using Microsoft.AspNet.Mvc;

    /// <summary>
    ///     The home controller.
    /// </summary>
    public class HomeController : Controller
    {
        #region Fields

        /// <summary>
        ///     The live show details.
        /// </summary>
        private readonly ILiveShowDetailsService liveShowDetails;

        /// <summary>
        ///     The shows service.
        /// </summary>
        private readonly IShowsService showsService;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="HomeController"/> class.
        /// </summary>
        /// <param name="showsService">
        /// The shows service.
        /// </param>
        /// <param name="liveShowDetails">
        /// The live show details.
        /// </param>
        public HomeController(IShowsService showsService, ILiveShowDetailsService liveShowDetails)
        {
            this.showsService = showsService;
            this.liveShowDetails = liveShowDetails;
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        ///     VERB: /error
        /// </summary>
        /// <returns>
        ///     An <see cref="IActionResult" /> .
        /// </returns>
        [Route("/error")]
        public IActionResult Error()
        {
            return this.View();
        }

        /// <summary>
        ///     GET: /ical
        /// </summary>
        /// <returns>
        ///     A <see cref="Task" /> of <see cref="LiveShowDetails" /> .
        /// </returns>
        [HttpGet("/ical")]
        [Produces("text/calendar")]
        public async Task<LiveShowDetails> GetiCal()
        {
            var loadedLiveShowDetails = await this.liveShowDetails.LoadAsync();

            return loadedLiveShowDetails;
        }

        /// <summary>
        /// VERB: /
        /// </summary>
        /// <param name="disableCache">
        /// A value indicating whether to disable the cache.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> of <see cref="IActionResult"/> .
        /// </returns>
        [Route("/")]
        public async Task<IActionResult> Index(bool? disableCache)
        {
            var loadedLiveShowDetails = await this.liveShowDetails.LoadAsync();
            var showList = await this.showsService.GetRecordedShowsAsync(this.User, disableCache ?? false);

            return
                this.View(
                    new HomeViewModel
                        {
                            AdminMessage = loadedLiveShowDetails?.AdminMessage,
                            NextShowDateUtc = loadedLiveShowDetails?.NextShowDateUtc,
                            LiveShowEmbedUrl = loadedLiveShowDetails?.LiveShowEmbedUrl,
                            PreviousShows = showList.Shows,
                            MoreShowsUrl = showList.MoreShowsUrl
                        });
        }

        #endregion
    }
}