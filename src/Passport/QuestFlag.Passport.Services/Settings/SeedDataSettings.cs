using System.Collections.Generic;

namespace QuestFlag.Passport.Services.Settings;

/// <summary>
/// Strongly-typed binding for the "Seed" configuration section.
/// All startup seed data is driven from this model — no values are hardcoded in code.
/// </summary>
public class SeedDataSettings
{
    public const string SectionName = "Seed";

    /// <summary>Identity roles to ensure exist on every startup.</summary>
    public List<string> Roles { get; set; } = [];

    /// <summary>Default tenant created when the Tenants table is empty.</summary>
    public SeedTenantSettings Tenant { get; set; } = new();

    /// <summary>Bootstrap admin user seeded alongside the default tenant.</summary>
    public SeedUserSettings AdminUser { get; set; } = new();

    /// <summary>OpenID Connect client applications (authorization-code flow).</summary>
    public List<SeedOidcAppSettings> OidcApps { get; set; } = [];

    /// <summary>Service/machine agents (client-credentials or confidential).</summary>
    public List<SeedAgentSettings> Agents { get; set; } = [];
}

public class SeedTenantSettings
{
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}

public class SeedUserSettings
{
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;

    /// <summary>Identity roles to assign to this user.</summary>
    public List<string> Roles { get; set; } = [];
}

public class SeedOidcAppSettings
{
    public string ClientId { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Base URL of the client — redirect URIs are derived as
    /// {BaseUrl}/signin-oidc and {BaseUrl}/signout-callback-oidc.
    /// </summary>
    public string BaseUrl { get; set; } = string.Empty;

    /// <summary>
    /// Optional explicit permission list. When empty the standard
    /// authorization-code + refresh-token set is used automatically.
    /// </summary>
    public List<string> Permissions { get; set; } = [];

    /// <summary>
    /// Whether to require PKCE for this application.
    /// </summary>
    public bool RequirePkce { get; set; } = true;
}

public class SeedAgentSettings
{
    public string ClientId { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string? ClientSecret { get; set; }
    public string Type { get; set; } = "confidential";
    public List<string> Permissions { get; set; } = [];
    public List<string> RedirectUris { get; set; } = [];
    public List<string> PostLogoutRedirectUris { get; set; } = [];
}
