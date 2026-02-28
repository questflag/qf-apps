using System;
using System.Security.Claims;

namespace QuestFlag.Infrastructure.ApiCore.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static Guid GetUserId(this ClaimsPrincipal principal)
    {
        var claim = principal.FindFirst("user_id");
        return claim != null && Guid.TryParse(claim.Value, out var id) ? id : Guid.Empty;
    }

    public static Guid GetTenantId(this ClaimsPrincipal principal)
    {
        var claim = principal.FindFirst("tenant_id");
        return claim != null && Guid.TryParse(claim.Value, out var id) ? id : Guid.Empty;
    }

    public static string GetTenantSlug(this ClaimsPrincipal principal)
    {
        return principal.FindFirst("tenant_slug")?.Value ?? string.Empty;
    }

    public static string GetRole(this ClaimsPrincipal principal)
    {
        return principal.FindFirst("role")?.Value ?? string.Empty;
    }

    /// <summary>
    /// Returns the authenticated user's ID, or null if the claim is absent/invalid.
    /// Works for both custom "user_id" claims and standard NameIdentifier/Subject claims.
    /// </summary>
    public static Guid? GetCurrentUserId(this ClaimsPrincipal principal)
    {
        var value = principal.FindFirst("user_id")?.Value
                 ?? principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(value, out var id) ? id : null;
    }
}
