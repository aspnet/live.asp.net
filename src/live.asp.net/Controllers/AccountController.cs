// Copyright (c) .NET Foundation. All rights reserved. 
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Mvc;

namespace live.asp.net.Controllers
{
    public class AccountController : Controller
    {
        [HttpGet("signin")]
        public IActionResult SignIn()
        {
            return Challenge(
                new AuthenticationProperties { RedirectUri = Url.Page("/Index") }, OpenIdConnectDefaults.AuthenticationScheme);
        }

        [HttpGet("signout")]
        public IActionResult SignOut()
        {
            var callbackUrl = Url.Page("/SignedOut", pageHandler: null, values: null, protocol: Request.Scheme);
            return SignOut(new AuthenticationProperties { RedirectUri = callbackUrl },
                CookieAuthenticationDefaults.AuthenticationScheme, OpenIdConnectDefaults.AuthenticationScheme);
        }
    }
}
