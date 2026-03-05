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
using OpenIddict.Validation.AspNetCore;
using QuestFlag.Passport.Domain.Entities;
using QuestFlag.Passport.Domain.Contracts;
using QuestFlag.Infrastructure.ApiCore.Constants;

namespace QuestFlag.Passport.Services.Controllers;

[ApiController]
[Route("connect")]
public class AuthController : ControllerBase
{
    private readonly IUserRepository _userRepository;
    private readonly SignInManager<ApplicationUser> _signInManager;

    public AuthController(
        IUserRepository userRepository,
        SignInManager<ApplicationUser> signInManager)
    {
        _userRepository = userRepository;
        _signInManager = signInManager;
    }

    [HttpPost("token")]
    [Consumes("application/x-www-form-urlencoded")]
    public async Task<IActionResult> Exchange()
    {
        var request = HttpContext.GetOpenIddictServerRequest();
        if (request == null)
            return BadRequest(new { error = "Invalid token request." });

        if (request.IsPasswordGrantType())
        {
            var user = await _userRepository.GetByUsernameAsync(request.Username ?? string.Empty);
            if (user == null || !user.IsActive)
                return Forbid(authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);

            var result = await _userRepository.CheckPasswordAsync(user, request.Password ?? string.Empty);
            if (!result)
                return Forbid(authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);

            var identity = new ClaimsIdentity(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
            
            // Core Identity claims
            identity.AddClaim(new Claim(OpenIddictConstants.Claims.Subject, user.Id.ToString())
                .SetDestinations(OpenIddictConstants.Destinations.AccessToken, OpenIddictConstants.Destinations.IdentityToken));
            
            identity.AddClaim(new Claim(OpenIddictConstants.Claims.Username, user.UserName ?? string.Empty)
                .SetDestinations(OpenIddictConstants.Destinations.AccessToken, OpenIddictConstants.Destinations.IdentityToken));
            
            // Custom Claims for QF
            identity.AddClaim(new Claim(QuestFlagClaimTypes.TenantId, user.TenantId.ToString())
                .SetDestinations(OpenIddictConstants.Destinations.AccessToken, OpenIddictConstants.Destinations.IdentityToken));
            
            identity.AddClaim(new Claim(QuestFlagClaimTypes.UserId, user.Id.ToString())
                .SetDestinations(OpenIddictConstants.Destinations.AccessToken, OpenIddictConstants.Destinations.IdentityToken));

            var roles = await _userRepository.GetRolesAsync(user);
            foreach (var role in roles)
            {
                identity.AddClaim(new Claim(OpenIddictConstants.Claims.Role, role)
                    .SetDestinations(OpenIddictConstants.Destinations.AccessToken, OpenIddictConstants.Destinations.IdentityToken));
                    
                identity.AddClaim(new Claim(ClaimTypes.Role, role)
                    .SetDestinations(OpenIddictConstants.Destinations.AccessToken, OpenIddictConstants.Destinations.IdentityToken));
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
        else if (request.IsAuthorizationCodeGrantType() || request.IsRefreshTokenGrantType())
        {
            // Retrieve the claims principal stored in the authorization code / refresh token.
            var authResult = await HttpContext.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
            if (authResult.Principal == null)
                return Forbid(authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);

            var user = await _userRepository.GetByIdAsync(Guid.Parse(authResult.Principal.GetClaim(OpenIddictConstants.Claims.Subject) ?? Guid.Empty.ToString()));
            if (user == null || !user.IsActive)
                return Forbid(authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);

            var identity = new ClaimsIdentity(authResult.Principal.Claims, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
            
            // Re-apply destinations to ensure they are carried over to the token payload
            foreach (var claim in identity.Claims)
            {
                // Core claims
                if (claim.Type == OpenIddictConstants.Claims.Subject ||
                    claim.Type == OpenIddictConstants.Claims.Username ||
                    claim.Type == QuestFlagClaimTypes.TenantId ||
                    claim.Type == QuestFlagClaimTypes.UserId ||
                    claim.Type == OpenIddictConstants.Claims.Role ||
                    claim.Type == ClaimTypes.Role)
                {
                    claim.SetDestinations(OpenIddictConstants.Destinations.AccessToken, OpenIddictConstants.Destinations.IdentityToken);
                }
            }
            
            var principal = new ClaimsPrincipal(identity);

            return SignIn(principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }

        return BadRequest(new { error = "The specified grant type is not supported." });
    }

    [HttpGet("userinfo"), HttpPost("userinfo"), Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
    public async Task<IActionResult> UserInfo()
    {
        var user = await _userRepository.GetByIdAsync(Guid.Parse(User.GetClaim(OpenIddictConstants.Claims.Subject) ?? Guid.Empty.ToString()));
        if (user == null)
        {
            return Challenge(
                authenticationSchemes: OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme,
                properties: new AuthenticationProperties(new Dictionary<string, string?>
                {
                    [OpenIddictServerAspNetCoreConstants.Properties.Error] = OpenIddictConstants.Errors.InvalidToken,
                    [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] =
                        "The specified access token is no longer valid."
                }));
        }

        var claims = new Dictionary<string, object>(StringComparer.Ordinal)
        {
            [OpenIddictConstants.Claims.Subject] = user.Id.ToString()
        };

        if (User.HasScope(OpenIddictConstants.Scopes.Email))
        {
            claims[OpenIddictConstants.Claims.Email] = user.Email ?? string.Empty;
            claims[OpenIddictConstants.Claims.EmailVerified] = user.EmailConfirmed;
        }

        if (User.HasScope(OpenIddictConstants.Scopes.Phone))
        {
            claims[OpenIddictConstants.Claims.PhoneNumber] = user.PhoneNumber ?? string.Empty;
            claims[OpenIddictConstants.Claims.PhoneNumberVerified] = user.PhoneNumberConfirmed;
        }

        if (User.HasScope(OpenIddictConstants.Scopes.Roles))
        {
            claims[OpenIddictConstants.Claims.Role] = await _userRepository.GetRolesAsync(user);
        }

        // Add custom claims
        claims[QuestFlagClaimTypes.TenantId] = user.TenantId.ToString();
        claims[QuestFlagClaimTypes.UserId] = user.Id.ToString();

        return Ok(claims);
    }

    [HttpGet("logout"), HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        // 1. Sign out from ASP.NET Core Identity (deletes the application cookie).
        await _signInManager.SignOutAsync();

        // 2. Return a SignOutResult which instructs OpenIddict to handle the 
        // OIDC post-logout redirection logic.
        return SignOut(
            authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
            properties: new AuthenticationProperties
            {
                RedirectUri = "/"
            });
    }
}
