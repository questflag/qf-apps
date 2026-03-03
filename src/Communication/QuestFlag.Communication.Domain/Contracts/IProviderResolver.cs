using QuestFlag.Communication.Domain.Entities;

namespace QuestFlag.Communication.Domain.Contracts;

public interface IProviderResolver
{
    Task<ProviderConfig?> ResolveOptimalProviderAsync(Guid tenantId, string channelType, string strategy = "COST");
}
