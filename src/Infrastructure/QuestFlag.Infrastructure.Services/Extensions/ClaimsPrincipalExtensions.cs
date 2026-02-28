using System;
using System.Security.Claims;

namespace QuestFlag.Infrastructure.Services.Extensions;

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
}
