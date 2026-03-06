using System;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.DependencyInjection;
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

    [HttpPost("~/connect/token")]
    public async Task<IActionResult> Exchange()
    {
        var request = HttpContext.GetOpenIddictServerRequest() ??
            throw new InvalidOperationException("The OpenID Connect request cannot be retrieved.");

        if (request.IsAuthorizationCodeGrantType() || request.IsRefreshTokenGrantType())
        {
            // Retrieve the claims principal stored in the authorization code/refresh token.
            var result = await HttpContext.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);

            // Return a SignInResult to ask OpenIddict to issue the appropriate access/identity tokens.
            return SignIn(result.Principal!, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }

        if (request.IsClientCredentialsGrantType())
        {
            var application = await HttpContext.RequestServices.GetRequiredService<IOpenIddictApplicationManager>()
                .FindByClientIdAsync(request.ClientId!);

            if (application == null)
            {
                return Forbid(
                    authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                    properties: new AuthenticationProperties(new Dictionary<string, string?>
                    {
                        [OpenIddictServerAspNetCoreConstants.Properties.Error] = OpenIddictConstants.Errors.InvalidClient,
                        [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "The client application was not found in the directory."
                    }));
            }

            var identity = new ClaimsIdentity(
                authenticationType: TokenValidationParameters.DefaultAuthenticationType,
                nameType: OpenIddictConstants.Claims.Name,
                roleType: OpenIddictConstants.Claims.Role);

            identity.SetClaim(OpenIddictConstants.Claims.Subject, request.ClientId)
                    .SetClaim(OpenIddictConstants.Claims.Name, await HttpContext.RequestServices.GetRequiredService<IOpenIddictApplicationManager>().GetDisplayNameAsync(application))
                    .SetClaim(QuestFlagClaimTypes.TenantId, Guid.Empty.ToString());

            identity.SetDestinations(GetDestinations);

            var principal = new ClaimsPrincipal(identity);
            principal.SetScopes(request.GetScopes());

            return SignIn(principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }

        throw new InvalidOperationException("The specified grant type is not supported.");
    }

    [HttpGet("~/connect/logout")]
    [HttpPost("~/connect/logout")]
    public async Task<IActionResult> Logout()
    {
        // Ask ASP.NET Core Identity to delete the local and external cookies created
        // when the user agent is redirected from the external identity provider
        // after a successful authentication flow (e.g Google or Facebook).
        await _signInManager.SignOutAsync();

        // Returning a SignOutResult will ask OpenIddict to redirect the user agent
        // to the post_logout_redirect_uri specified by the client application or to
        // the RedirectUri specified in the authentication properties if none was set.
        return SignOut(
            authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
            properties: new AuthenticationProperties
            {
                RedirectUri = "/"
            });
    }

    private static IEnumerable<string> GetDestinations(Claim claim)
    {
        // Note: by default, claims are NOT automatically included in the access and identity tokens.
        // To allow OpenIddict to serialize them, you must attach them a destination, that specifies
        // whether they should be included in access tokens, in identity tokens or in both.

        switch (claim.Type)
        {
            case OpenIddictConstants.Claims.Name:
                yield return OpenIddictConstants.Destinations.AccessToken;

                if (claim.Subject?.HasScope(OpenIddictConstants.Scopes.Profile) == true)
                    yield return OpenIddictConstants.Destinations.IdentityToken;

                yield break;

            case OpenIddictConstants.Claims.Email:
                yield return OpenIddictConstants.Destinations.AccessToken;

                if (claim.Subject?.HasScope(OpenIddictConstants.Scopes.Email) == true)
                    yield return OpenIddictConstants.Destinations.IdentityToken;

                yield break;

            case OpenIddictConstants.Claims.Role:
                yield return OpenIddictConstants.Destinations.AccessToken;

                if (claim.Subject?.HasScope(OpenIddictConstants.Scopes.Roles) == true)
                    yield return OpenIddictConstants.Destinations.IdentityToken;

                yield break;

            // Never include the security stamp in the access and identity tokens, as it's a secret value.
            case "AspNet.Identity.SecurityStamp": yield break;

            default:
                yield return OpenIddictConstants.Destinations.AccessToken;
                yield break;
        }
    }
}
