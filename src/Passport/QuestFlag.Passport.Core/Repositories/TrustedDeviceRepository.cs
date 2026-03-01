using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using QuestFlag.Passport.Core.Data;
using QuestFlag.Passport.Domain.Entities;
using QuestFlag.Passport.Domain.Interfaces;

using QuestFlag.Infrastructure.Core.Repositories;

namespace QuestFlag.Passport.Core.Repositories;

public class TrustedDeviceRepository : Repository<TrustedDevice, PassportDbContext>, ITrustedDeviceRepository
{
    public TrustedDeviceRepository(PassportDbContext db) : base(db) {}

    public async Task<TrustedDevice?> GetByTokenHashAsync(string tokenHash, CancellationToken ct = default)
        => await SingleOrDefaultAsync(d => d.DeviceTokenHash == tokenHash && !d.IsRevoked && d.ExpiresAtUtc > DateTime.UtcNow, ct);

    public async Task<IReadOnlyList<TrustedDevice>> GetByUserIdAsync(Guid userId, CancellationToken ct = default)
    {
        var devices = await FindAsync(d => d.UserId == userId && !d.IsRevoked && d.ExpiresAtUtc > DateTime.UtcNow, ct);
        return devices.OrderByDescending(d => d.TrustedAtUtc).ToList();
    }

    public async Task<TrustedDevice?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await base.GetByIdAsync(id, ct);

    public async Task RevokeAllForUserAsync(Guid userId, CancellationToken ct = default)
    {
        var devices = await FindAsync(d => d.UserId == userId && !d.IsRevoked, ct);

        foreach (var d in devices)
            d.IsRevoked = true;

        DbContext.TrustedDevices.UpdateRange(devices);
        await DbContext.SaveChangesAsync(ct);
    }
}
