using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using QuestFlag.Passport.Domain.Entities;

namespace QuestFlag.Passport.Domain.Interfaces;

public interface ITenantRepository
{
    Task<IReadOnlyList<Tenant>> GetAllAsync(CancellationToken ct = default);
    Task<Tenant?> GetByIdAsync(System.Guid id, CancellationToken ct = default);
    Task<Tenant?> GetBySlugAsync(string slug, CancellationToken ct = default);
    Task<Tenant> AddAsync(Tenant tenant, CancellationToken ct = default);
    Task UpdateAsync(Tenant tenant, CancellationToken ct = default);
    Task DeleteAsync(System.Guid id, CancellationToken ct = default);
}
