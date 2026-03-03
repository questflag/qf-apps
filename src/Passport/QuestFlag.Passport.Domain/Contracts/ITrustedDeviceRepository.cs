using QuestFlag.Passport.Domain.Entities;

namespace QuestFlag.Passport.Domain.Contracts;

public interface ITrustedDeviceRepository
{
    Task<TrustedDevice?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<TrustedDevice>> GetByUserIdAsync(Guid userId, CancellationToken ct = default);
    Task AddAsync(TrustedDevice device, CancellationToken ct = default);
    Task UpdateAsync(TrustedDevice device, CancellationToken ct = default);
    Task RevokeAllForUserAsync(Guid userId, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
