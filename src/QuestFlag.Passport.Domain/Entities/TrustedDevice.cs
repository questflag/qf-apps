using System;

namespace QuestFlag.Passport.Domain.Entities;

/// <summary>
/// Represents a browser/device that a user has chosen to trust after completing 2FA.
/// A valid, non-revoked, non-expired TrustedDevice allows skipping the 2FA step at login.
/// </summary>
public class TrustedDevice
{
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>FK → ApplicationUser</summary>
    public Guid UserId { get; set; }
    public ApplicationUser User { get; set; } = default!;

    /// <summary>
    /// Long random token (e.g. 64-char hex) stored as an HttpOnly cookie on the browser.
    /// Stored as SHA-256 hash in the database — never stored raw.
    /// </summary>
    public string DeviceTokenHash { get; set; } = string.Empty;

    /// <summary>Human-readable device name parsed from User-Agent (e.g. "Chrome on Windows").</summary>
    public string DeviceName { get; set; } = string.Empty;

    /// <summary>IP address at the time the device was trusted.</summary>
    public string IpAddress { get; set; } = string.Empty;

    public DateTime TrustedAtUtc { get; init; } = DateTime.UtcNow;

    /// <summary>Defaults to 30 days from TrustedAtUtc.</summary>
    public DateTime ExpiresAtUtc { get; set; }

    /// <summary>Set to true when the user or admin explicitly revokes this device.</summary>
    public bool IsRevoked { get; set; }
}
