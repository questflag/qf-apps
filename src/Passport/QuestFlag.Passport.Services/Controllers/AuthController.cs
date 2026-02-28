using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using QuestFlag.Passport.Domain.Entities;

namespace QuestFlag.Passport.Services.Controllers;

[ApiController]
[Route("connect")]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;

    public AuthController(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    [HttpPost("token"), IgnoreAntiforgeryToken]
    [Consumes("application/x-www-form-urlencoded")]
    public async Task<IActionResult> Exchange()
    {
        var request = HttpContext.GetOpenIddictServerRequest();
        if (request == null)
            return BadRequest(new { error = "Invalid token request." });

        if (request.IsPasswordGrantType())
        {
            var user = await _userManager.FindByNameAsync(request.Username ?? string.Empty);
            if (user == null || !user.IsActive)
                return Forbid(authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);

            var result = await _userManager.CheckPasswordAsync(user, request.Password ?? string.Empty);
            if (!result)
                return Forbid(authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);

            var identity = new ClaimsIdentity(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
            
            // Core Identity claims
            identity.AddClaim(OpenIddictConstants.Claims.Subject, user.Id.ToString());
            identity.AddClaim(OpenIddictConstants.Claims.Username, user.UserName ?? string.Empty);
            
            // Custom Claims for QF
            identity.AddClaim("tenant_id", user.TenantId.ToString(), OpenIddictConstants.Destinations.AccessToken);
            identity.AddClaim("user_id", user.Id.ToString(), OpenIddictConstants.Destinations.AccessToken);

            var roles = await _userManager.GetRolesAsync(user);
            foreach (var role in roles)
            {
                identity.AddClaim(OpenIddictConstants.Claims.Role, role, OpenIddictConstants.Destinations.AccessToken);
            }

            var principal = new ClaimsPrincipal(identity);
            
            // Set required scopes
            principal.SetScopes(new[]
            {
                OpenIddictConstants.Scopes.Roles,
                OpenIddictConstants.Scopes.OfflineAccess // enable refresh_token
            });

            return SignIn(principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }
        else if (request.IsRefreshTokenGrantType())
        {
            // Retrieve the claims principal stored in the refresh token.
            var authResult = await HttpContext.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
            if (authResult.Principal == null)
                return Forbid(authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);

            var user = await _userManager.FindByIdAsync(authResult.Principal.GetClaim(OpenIddictConstants.Claims.Subject) ?? string.Empty);
            if (user == null || !user.IsActive)
                return Forbid(authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);

            var identity = new ClaimsIdentity(authResult.Principal.Claims, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            return SignIn(principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }

        return BadRequest(new { error = "The specified grant type is not supported." });
    }

    [HttpPost("logout"), Authorize]
    public async Task<IActionResult> Logout()
    {
        // For JWT, "logging out" client side means deleting the token, but server side
        // we can optionally revoke the refresh token (if using token revocation feature in OpenIddict).
        // Since we are strictly using bare JWT + refresh token here via SignOut:
        await HttpContext.SignOutAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        return Ok(new { message = "Logged out successfully." });
    }
}
