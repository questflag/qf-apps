using System;
using System.Security.Claims;
using OpenIddict.Abstractions;

namespace QuestFlag.Passport.ApiCore.Extensions;

public static class ClaimsPrincipalExtensions
{
    /// <summary>
    /// Returns the authenticated user's ID from the OpenIddict Subject or standard NameIdentifier claim.
    /// Returns null if the claim is absent or cannot be parsed as a Guid.
    /// </summary>
    public static Guid? GetCurrentUserId(this ClaimsPrincipal principal)
    {
        var value = principal.FindFirst(OpenIddictConstants.Claims.Subject)?.Value
                 ?? principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(value, out var id) ? id : null;
    }
}
