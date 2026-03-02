using QuestFlag.Communication.Domain.Entities;
using QuestFlag.Communication.Domain.Interfaces;
using QuestFlag.Communication.Core.Persistence.PostgreSQL;
using Microsoft.EntityFrameworkCore;

namespace QuestFlag.Communication.Core.Providers;

public class ProviderResolver : IProviderResolver
{
    private readonly CommunicationDbContext _context;

    public ProviderResolver(CommunicationDbContext context)
    {
        _context = context;
    }

    public async Task<ProviderConfig?> ResolveOptimalProviderAsync(Guid tenantId, string channelType, string strategy = "COST")
    {
        // Simple priority-based resolution for now
        return await _context.ProviderConfigs
            .Where(x => x.TenantId == tenantId && x.ProviderType == channelType)
            .OrderByDescending(x => x.Priority)
            .FirstOrDefaultAsync();
    }
}
