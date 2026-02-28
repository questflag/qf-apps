using System;
using Microsoft.AspNetCore.Identity;

namespace QuestFlag.Passport.Domain.Entities;

public class ApplicationUser : IdentityUser<Guid>
{
    public Guid TenantId { get; set; }
    public Tenant Tenant { get; set; } = default!;

    public string DisplayName { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAtUtc { get; init; } = DateTime.UtcNow;
    public DateTime? LastLogoutAtUtc { get; set; }

    // Inherited from IdentityUser — used for SSO identity lifecycle:
    // PhoneNumber, PhoneNumberConfirmed  → used for SMS 2FA
    // TwoFactorEnabled                  → gates the 2FA step at login
    // Email, EmailConfirmed             → email-verification / invite flow
    // SecurityStamp                     → force-logout: changed on password reset / 2FA change
}
