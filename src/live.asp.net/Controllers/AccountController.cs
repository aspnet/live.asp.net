// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AccountController.cs" company=".NET Foundation">
//   Copyright">Copyright (c) .NET Foundation. All rights reserved.
//   Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// </copyright>
// <summary>
//   The account controller.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace live.asp.net.Controllers
{
    using System.Threading.Tasks;

    using Microsoft.AspNet.Authentication.Cookies;
    using Microsoft.AspNet.Authentication.OpenIdConnect;
    using Microsoft.AspNet.Http.Authentication;
    using Microsoft.AspNet.Mvc;

    /// <summary>
    ///     The account controller.
    /// </summary>
    public class AccountController : Controller
    {
        #region Public Methods and Operators

        /// <summary>
        ///     GET: account/signin
        /// </summary>
        /// <returns>
        ///     An <see cref="Microsoft.AspNet.Mvc.IActionResult" /> .
        /// </returns>
        [HttpGet("signin")]
        public IActionResult SignIn()
        {
            return new ChallengeResult(
                OpenIdConnectAuthenticationDefaults.AuthenticationScheme,
                new AuthenticationProperties { RedirectUri = "/" });
        }

        /// <summary>
        ///     GET: account/signout
        /// </summary>
        /// <returns>
        ///     A Task of <see cref="Microsoft.AspNet.Mvc.IActionResult" /> .
        /// </returns>
        [HttpGet("signout")]
        public async Task<IActionResult> SignOut()
        {
            var callbackUrl = this.Url.Action(nameof(this.SignOutCallback), "Account", null, this.Request.Scheme);
            await this.Context.Authentication.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            await this.Context.Authentication.SignOutAsync(OpenIdConnectAuthenticationDefaults.AuthenticationScheme);

            return new EmptyResult();
        }

        /// <summary>
        ///     GET: account/signoutcallback
        /// </summary>
        /// <returns>
        ///     A Task of <see cref="Microsoft.AspNet.Mvc.IActionResult" /> .
        /// </returns>
        [HttpGet("signoutcallback")]
        public IActionResult SignOutCallback()
        {
            if (this.Context.User.Identity.IsAuthenticated)
            {
                // Redirect to home page if the user is authenticated.
                return this.RedirectToAction(nameof(HomeController.Index), "Home");
            }

            return this.View();
        }

        #endregion
    }
}
