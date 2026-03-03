using QuestFlag.Communication.Domain.Entities;

namespace QuestFlag.Communication.Domain.Contracts;

public interface IConversationThreadRepository
{
    Task<ConversationThread?> GetActiveByParticipantAsync(string tenantId, string participantId); // Simplified for now
    Task<ConversationThread?> GetByIdAsync(Guid id);
    Task AddAsync(ConversationThread thread);
    Task UpdateAsync(ConversationThread thread);
    Task ArchiveAsync(Guid id);
}
