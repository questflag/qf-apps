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
using Microsoft.AspNetCore.Identity;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using QuestFlag.Passport.Domain.Entities;
using QuestFlag.Passport.Domain.Contracts;
using QuestFlag.Infrastructure.ApiCore.Constants;

namespace QuestFlag.Passport.Services.Controllers;

[ApiController]
public class AuthorizationController : ControllerBase
{
    private readonly IConfiguration _config;
    private readonly IUserRepository _userRepository;
    private readonly SignInManager<ApplicationUser> _signInManager;

    public AuthorizationController(
        IConfiguration config,
        IUserRepository userRepository,
        SignInManager<ApplicationUser> signInManager)
    {
        _config = config;
        _userRepository = userRepository;
        _signInManager = signInManager;
    }

    [HttpGet("~/connect/authorize")]
    [HttpPost("~/connect/authorize")]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> Authorize()
    {
        var request = HttpContext.GetOpenIddictServerRequest() ??
            throw new InvalidOperationException("The OpenID Connect request cannot be retrieved.");

        // 1. If it's a POST and we have credentials, try to sign the user in
        if (Request.Method == "POST" && !string.IsNullOrEmpty(Request.Form["username"]) && !string.IsNullOrEmpty(Request.Form["password"]))
        {
            var user = await _userRepository.GetByUsernameAsync(Request.Form["username"]!);
            if (user != null && await _userRepository.CheckPasswordAsync(user, Request.Form["password"]!))
            {
                await _signInManager.SignInAsync(user, isPersistent: true);
                
                // Refresh the request so OpenIddict sees the new identity
                return Redirect(Request.Path + Request.QueryString);
            }

            ModelState.AddModelError(string.Empty, "Invalid username or password.");
        }

        // 2. Retrieve the user principal stored in the authentication cookie (Identity).
        var result = await HttpContext.AuthenticateAsync(IdentityConstants.ApplicationScheme);

        // 3. If the user principal can't be extracted, redirect the user to the login page.
        if (!result.Succeeded || result.Principal == null || !result.Principal.Identity!.IsAuthenticated)
        {
            var webAppBaseUrl = _config["Passport:WebAppBaseUrl"]
                ?? throw new InvalidOperationException("Passport:WebAppBaseUrl is required in configuration.");

            var loginUrl = $"{webAppBaseUrl}/sso";
            var query = Request.QueryString.ToString();
            
            return Redirect($"{loginUrl}{query}");
        }

        // 4. Create a new claims principal for OpenIddict
        var userId = result.Principal.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
        {
            return Forbid(authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }

        var loggedInUser = await _userRepository.GetByIdAsync(Guid.Parse(userId));
        if (loggedInUser == null)
        {
            return Forbid(authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }

        var identity = new ClaimsIdentity(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);

        identity.AddClaim(new Claim(OpenIddictConstants.Claims.Subject, loggedInUser.Id.ToString())
            .SetDestinations(OpenIddictConstants.Destinations.AccessToken, OpenIddictConstants.Destinations.IdentityToken));
            
        identity.AddClaim(new Claim(OpenIddictConstants.Claims.Username, loggedInUser.UserName ?? string.Empty)
            .SetDestinations(OpenIddictConstants.Destinations.AccessToken, OpenIddictConstants.Destinations.IdentityToken));

        identity.AddClaim(new Claim(QuestFlagClaimTypes.TenantId, loggedInUser.TenantId.ToString())
            .SetDestinations(OpenIddictConstants.Destinations.AccessToken, OpenIddictConstants.Destinations.IdentityToken));

        identity.AddClaim(new Claim(QuestFlagClaimTypes.UserId, loggedInUser.Id.ToString())
            .SetDestinations(OpenIddictConstants.Destinations.AccessToken, OpenIddictConstants.Destinations.IdentityToken));

        var roles = await _userRepository.GetRolesAsync(loggedInUser);
        foreach (var role in roles)
        {
            identity.AddClaim(new Claim(OpenIddictConstants.Claims.Role, role)
                .SetDestinations(OpenIddictConstants.Destinations.AccessToken, OpenIddictConstants.Destinations.IdentityToken));
                
            identity.AddClaim(new Claim(System.Security.Claims.ClaimTypes.Role, role)
                .SetDestinations(OpenIddictConstants.Destinations.AccessToken, OpenIddictConstants.Destinations.IdentityToken));
        }

        var openIdPrincipal = new ClaimsPrincipal(identity);
        openIdPrincipal.SetScopes(request.GetScopes());

        return SignIn(openIdPrincipal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }
}
