namespace QuestFlag.Infrastructure.ApiCore.Constants;

/// <summary>
/// Centralised claim type names used across QuestFlag services and Blazor clients.
/// Keeping them here prevents silent breakage when a claim key is renamed.
/// </summary>
public static class QuestFlagClaimTypes
{
    public const string UserId     = "user_id";
    public const string TenantId   = "tenant_id";
    public const string TenantSlug = "tenant_slug";
    public const string Role       = "role";
}
