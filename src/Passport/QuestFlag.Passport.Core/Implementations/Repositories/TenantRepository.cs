using Microsoft.EntityFrameworkCore;
using QuestFlag.Passport.Domain.Entities;
using QuestFlag.Passport.Domain.Contracts;
using QuestFlag.Passport.Core.Data;

namespace QuestFlag.Passport.Core.Implementations.Repositories;

public class TenantRepository : ITenantRepository
{
    private readonly PassportDbContext _dbContext;

    public TenantRepository(PassportDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Tenant?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _dbContext.Tenants.FindAsync(new object[] { id }, ct);
    }

    public async Task<Tenant?> GetBySlugAsync(string slug, CancellationToken ct = default)
    {
        return await _dbContext.Tenants.FirstOrDefaultAsync(t => t.Slug == slug, ct);
    }

    public async Task<Tenant?> GetByDomainAsync(string domain, CancellationToken ct = default)
    {
        return await _dbContext.Tenants
            .FirstOrDefaultAsync(t => t.CustomDomain == domain || t.SubdomainSlug == domain, ct);
    }

    public async Task<IReadOnlyList<Tenant>> GetAllAsync(CancellationToken ct = default)
    {
        return await _dbContext.Tenants.ToListAsync(ct);
    }

    public async Task AddAsync(Tenant tenant, CancellationToken ct = default)
    {
        await _dbContext.Tenants.AddAsync(tenant, ct);
        await _dbContext.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Tenant tenant, CancellationToken ct = default)
    {
        _dbContext.Tenants.Update(tenant);
        await _dbContext.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var tenant = await GetByIdAsync(id, ct);
        if (tenant != null)
        {
            _dbContext.Tenants.Remove(tenant);
            await _dbContext.SaveChangesAsync(ct);
        }
    }
}
