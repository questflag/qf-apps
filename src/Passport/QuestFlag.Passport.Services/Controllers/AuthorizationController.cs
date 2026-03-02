using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;

namespace QuestFlag.Passport.Services.Controllers;

[ApiController]
public class AuthorizationController : ControllerBase
{
    private readonly IConfiguration _config;

    public AuthorizationController(IConfiguration config)
    {
        _config = config;
    }

    [HttpGet("~/connect/authorize")]
    [HttpPost("~/connect/authorize")]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> Authorize()
    {
        var request = HttpContext.GetOpenIddictServerRequest() ??
            throw new InvalidOperationException("The OpenID Connect request cannot be retrieved.");

        // Retrieve the user principal stored in the authentication cookie.
        var result = await HttpContext.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);

        // If the user principal can't be extracted, redirect the user to the login page.
        if (!result.Succeeded)
        {
            var webAppBaseUrl = _config["Passport:WebAppBaseUrl"]
                ?? throw new InvalidOperationException("Passport:WebAppBaseUrl is required in configuration.");

            var loginUrl = $"{webAppBaseUrl}/sso";
            var query = Request.QueryString.ToString();
            
            return Redirect($"{loginUrl}{query}");
        }

        // Create a new claims principal
        var principal = result.Principal;

        // Note: in this simple implementation, we assume the user is already signed in
        // and we just return a sign-in result with the user's principal.
        return SignIn(principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }
}
