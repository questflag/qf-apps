using QuestFlag.Communication.Domain.Entities;
using QuestFlag.Communication.Domain.Enums;

namespace QuestFlag.Communication.Domain.Interfaces;

public interface ICommunicationLogRepository
{
    Task<CommunicationLog?> GetByIdAsync(Guid id);
    Task<CommunicationLog?> GetByTransactionIdAsync(string transactionId);
    Task AddAsync(CommunicationLog log);
    Task UpdateAsync(CommunicationLog log);
}
