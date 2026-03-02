using QuestFlag.Communication.Domain.Entities;

namespace QuestFlag.Communication.Domain.Interfaces;

public interface IProviderResolver
{
    Task<ProviderConfig?> ResolveOptimalProviderAsync(Guid tenantId, string channelType, string strategy = "COST");
}
