using Microsoft.EntityFrameworkCore;
using QuestFlag.Passport.Domain.Entities;
using QuestFlag.Passport.Domain.Contracts;
using QuestFlag.Passport.Core.Data;

namespace QuestFlag.Passport.Core.Implementations.Repositories;

public class TrustedDeviceRepository : ITrustedDeviceRepository
{
    private readonly PassportDbContext _dbContext;

    public TrustedDeviceRepository(PassportDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<TrustedDevice?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _dbContext.TrustedDevices.FindAsync(new object[] { id }, ct);
    }

    public async Task<IReadOnlyList<TrustedDevice>> GetByUserIdAsync(Guid userId, CancellationToken ct = default)
    {
        return await _dbContext.TrustedDevices
            .Where(d => d.UserId == userId)
            .ToListAsync(ct);
    }

    public async Task AddAsync(TrustedDevice device, CancellationToken ct = default)
    {
        await _dbContext.TrustedDevices.AddAsync(device, ct);
        await _dbContext.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(TrustedDevice device, CancellationToken ct = default)
    {
        _dbContext.TrustedDevices.Update(device);
        await _dbContext.SaveChangesAsync(ct);
    }

    public async Task RevokeAllForUserAsync(Guid userId, CancellationToken ct = default)
    {
        var devices = await _dbContext.TrustedDevices.Where(d => d.UserId == userId).ToListAsync(ct);
        foreach (var device in devices)
        {
            device.IsRevoked = true;
        }
        await _dbContext.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var device = await GetByIdAsync(id, ct);
        if (device != null)
        {
            _dbContext.TrustedDevices.Remove(device);
            await _dbContext.SaveChangesAsync(ct);
        }
    }

}
