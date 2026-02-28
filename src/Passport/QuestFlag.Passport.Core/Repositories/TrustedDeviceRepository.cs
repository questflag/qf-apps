using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using QuestFlag.Passport.Core.Data;
using QuestFlag.Passport.Domain.Entities;
using QuestFlag.Passport.Domain.Interfaces;

namespace QuestFlag.Passport.Core.Repositories;

public class TrustedDeviceRepository : ITrustedDeviceRepository
{
    private readonly PassportDbContext _db;

    public TrustedDeviceRepository(PassportDbContext db) => _db = db;

    public async Task<TrustedDevice?> GetByTokenHashAsync(string tokenHash, CancellationToken ct = default)
        => await _db.TrustedDevices
               .FirstOrDefaultAsync(d => d.DeviceTokenHash == tokenHash && !d.IsRevoked && d.ExpiresAtUtc > DateTime.UtcNow, ct);

    public async Task<IReadOnlyList<TrustedDevice>> GetByUserIdAsync(Guid userId, CancellationToken ct = default)
        => await _db.TrustedDevices
               .Where(d => d.UserId == userId && !d.IsRevoked && d.ExpiresAtUtc > DateTime.UtcNow)
               .OrderByDescending(d => d.TrustedAtUtc)
               .ToListAsync(ct);

    public async Task<TrustedDevice?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _db.TrustedDevices.FindAsync(new object[] { id }, ct);

    public async Task AddAsync(TrustedDevice device, CancellationToken ct = default)
    {
        _db.TrustedDevices.Add(device);
        await _db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(TrustedDevice device, CancellationToken ct = default)
    {
        _db.TrustedDevices.Update(device);
        await _db.SaveChangesAsync(ct);
    }

    public async Task RevokeAllForUserAsync(Guid userId, CancellationToken ct = default)
    {
        var devices = await _db.TrustedDevices
            .Where(d => d.UserId == userId && !d.IsRevoked)
            .ToListAsync(ct);

        foreach (var d in devices)
            d.IsRevoked = true;

        await _db.SaveChangesAsync(ct);
    }
}
