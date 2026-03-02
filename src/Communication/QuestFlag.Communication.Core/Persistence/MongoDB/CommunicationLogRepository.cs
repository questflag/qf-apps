using MongoDB.Driver;
using QuestFlag.Communication.Domain.Entities;
using QuestFlag.Communication.Domain.Interfaces;

namespace QuestFlag.Communication.Core.Persistence.MongoDB;

public class CommunicationLogRepository : ICommunicationLogRepository
{
    private readonly IMongoCollection<CommunicationLog> _collection;

    public CommunicationLogRepository(IMongoDatabase database)
    {
        _collection = database.GetCollection<CommunicationLog>("communications_log");
    }

    public async Task<CommunicationLog?> GetByIdAsync(Guid id) =>
        await _collection.Find(x => x.Id == id).FirstOrDefaultAsync();

    public async Task<CommunicationLog?> GetByTransactionIdAsync(string transactionId) =>
        await _collection.Find(x => x.TransactionId == transactionId).FirstOrDefaultAsync();

    public async Task AddAsync(CommunicationLog log) =>
        await _collection.InsertOneAsync(log);

    public async Task UpdateAsync(CommunicationLog log) =>
        await _collection.ReplaceOneAsync(x => x.Id == log.Id, log);
}
