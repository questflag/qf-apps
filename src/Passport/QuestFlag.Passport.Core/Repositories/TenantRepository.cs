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

public class TenantRepository : Repository<Tenant, PassportDbContext>, ITenantRepository
{
    public TenantRepository(PassportDbContext dbContext) : base(dbContext)
    {
    }



    public async Task<Tenant?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await base.GetByIdAsync(id, ct);
    }

    public async Task<Tenant?> GetBySlugAsync(string slug, CancellationToken ct = default)
    {
        return await SingleOrDefaultAsync(t => t.Slug == slug, ct);
    }

    public async Task<Tenant?> GetByDomainAsync(string host, CancellationToken ct = default)
    {
        var hostWithoutPort = host.Split(':')[0].ToLowerInvariant();

        // 1. Exact CustomDomain match
        var byCustomDomain = await SingleOrDefaultAsync(t => t.CustomDomain != null && t.CustomDomain.ToLower() == hostWithoutPort, ct);

        if (byCustomDomain != null) return byCustomDomain;

        // 2. SubdomainSlug match: first segment of host (e.g. "acme" from "acme.questflag.com")
        var firstSegment = hostWithoutPort.Split('.')[0];
        return await SingleOrDefaultAsync(t => t.SubdomainSlug != null && t.SubdomainSlug.ToLower() == firstSegment, ct);
    }



    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var tenant = await GetByIdAsync(id, ct);
        if (tenant != null)
        {
            await DeleteAsync(tenant, ct);
        }
    }
}
