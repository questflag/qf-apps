using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using QuestFlag.Passport.Core.Data;
using QuestFlag.Passport.Domain.Entities;
using QuestFlag.Passport.Domain.Interfaces;

namespace QuestFlag.Passport.Core.Repositories;

public class TenantRepository : ITenantRepository
{
    private readonly PassportDbContext _dbContext;

    public TenantRepository(PassportDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<Tenant>> GetAllAsync(CancellationToken ct = default)
    {
        return await _dbContext.Tenants.ToListAsync(ct);
    }

    public async Task<Tenant?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _dbContext.Tenants.FindAsync(new object[] { id }, ct);
    }

    public async Task<Tenant?> GetBySlugAsync(string slug, CancellationToken ct = default)
    {
        return await _dbContext.Tenants.FirstOrDefaultAsync(t => t.Slug == slug, ct);
    }

    public async Task<Tenant> AddAsync(Tenant tenant, CancellationToken ct = default)
    {
        _dbContext.Tenants.Add(tenant);
        await _dbContext.SaveChangesAsync(ct);
        return tenant;
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
