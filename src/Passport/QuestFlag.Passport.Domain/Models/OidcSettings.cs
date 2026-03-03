namespace QuestFlag.Passport.Domain.Models;

/// <summary>
/// Strongly-typed binding for the "Oidc" configuration section.
/// Used by web-app clients that participate in the OpenID Connect flow.
/// </summary>
public class OidcSettings
{
    public const string SectionName = "Oidc";

    /// <summary>The OpenIddict / Passport server authority URL.</summary>
    public string Authority { get; set; } = string.Empty;

    /// <summary>The client_id registered in OpenIddict for this application.</summary>
    public string ClientId { get; set; } = string.Empty;
}
