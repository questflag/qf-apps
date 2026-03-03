using QuestFlag.Communication.Domain.DTOs;

namespace QuestFlag.Communication.Client.Contracts;

public interface ICommunicationClient
{
    Task<string> SendMessageAsync(SendMessageDto dto);
    Task<MessageStatusDto?> GetMessageStatusAsync(string transactionId);
}
