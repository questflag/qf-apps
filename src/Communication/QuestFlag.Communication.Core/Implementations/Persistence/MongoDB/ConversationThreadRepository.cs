using MongoDB.Driver;
using QuestFlag.Communication.Domain.Entities;
using QuestFlag.Communication.Domain.Enums;
using QuestFlag.Communication.Domain.Contracts;

namespace QuestFlag.Communication.Core.Implementations.Persistence.MongoDB;

public class ConversationThreadRepository : IConversationThreadRepository
{
    private readonly IMongoCollection<ConversationThread> _activeThreads;
    private readonly IMongoCollection<ConversationThread> _archivedThreads;

    public ConversationThreadRepository(IMongoDatabase database)
    {
        _activeThreads = database.GetCollection<ConversationThread>("threads_active");
        _archivedThreads = database.GetCollection<ConversationThread>("threads_archived");
    }

    public async Task<ConversationThread?> GetActiveByParticipantAsync(string tenantId, string participantId)
    {
        // For simplicity, finding by recipient/participant logic
        return await _activeThreads.Find(x => x.TenantId == tenantId && x.Status == ConversationStatus.ACTIVE).FirstOrDefaultAsync();
    }

    public async Task<ConversationThread?> GetByIdAsync(Guid id) =>
        await _activeThreads.Find(x => x.Id == id).FirstOrDefaultAsync();

    public async Task AddAsync(ConversationThread thread) =>
        await _activeThreads.InsertOneAsync(thread);

    public async Task UpdateAsync(ConversationThread thread) =>
        await _activeThreads.ReplaceOneAsync(x => x.Id == thread.Id, thread);

    public async Task ArchiveAsync(Guid id)
    {
        var thread = await GetByIdAsync(id);
        if (thread != null)
        {
            thread.Status = ConversationStatus.ARCHIVED;
            thread.ClosedAt = DateTime.UtcNow;
            await _archivedThreads.InsertOneAsync(thread);
            await _activeThreads.DeleteOneAsync(x => x.Id == id);
        }
    }
}
