using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using QuestFlag.Passport.Domain.Entities;

namespace QuestFlag.Passport.Domain.Interfaces;

public interface ITrustedDeviceRepository
{
    /// <summary>Look up a device by its SHA-256 token hash.</summary>
    Task<TrustedDevice?> GetByTokenHashAsync(string tokenHash, CancellationToken ct = default);

    /// <summary>List all active (non-revoked, non-expired) devices for a user.</summary>
    Task<IReadOnlyList<TrustedDevice>> GetByUserIdAsync(Guid userId, CancellationToken ct = default);

    Task<TrustedDevice?> GetByIdAsync(Guid id, CancellationToken ct = default);

    Task AddAsync(TrustedDevice device, CancellationToken ct = default);

    Task UpdateAsync(TrustedDevice device, CancellationToken ct = default);

    /// <summary>Revoke all devices for a user (e.g. on password change or admin action).</summary>
    Task RevokeAllForUserAsync(Guid userId, CancellationToken ct = default);
}
