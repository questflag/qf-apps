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

    /// <summary>
    /// Resolves a tenant by matching the given host value against
    /// <see cref="Tenant.CustomDomain"/> or <see cref="Tenant.SubdomainSlug"/>.
    /// Used by the SSO login page to auto-detect the tenant from the request Host header.
    /// </summary>
    Task<Tenant?> GetByDomainAsync(string host, CancellationToken ct = default);

    Task<Tenant> AddAsync(Tenant tenant, CancellationToken ct = default);
    Task UpdateAsync(Tenant tenant, CancellationToken ct = default);
    Task DeleteAsync(System.Guid id, CancellationToken ct = default);
}

