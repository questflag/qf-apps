using System;

namespace QuestFlag.Passport.Domain.Entities;

public class Tenant
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAtUtc { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// Optional fully-qualified custom domain (e.g. "acme.questflag.com" or "auth.acmecorp.com").
    /// When set the SSO login page auto-resolves this tenant from the Host header.
    /// </summary>
    public string? CustomDomain { get; set; }

    /// <summary>
    /// Optional sub-domain slug (e.g. "acme" → recognized at "acme.questflag.com").
    /// Used as a fallback when <see cref="CustomDomain"/> is not set.
    /// </summary>
    public string? SubdomainSlug { get; set; }
}
