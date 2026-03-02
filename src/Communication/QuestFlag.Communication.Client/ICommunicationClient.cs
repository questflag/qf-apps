using QuestFlag.Communication.Application.Common.DTOs;

namespace QuestFlag.Communication.Client;

public interface ICommunicationClient
{
    Task<string> SendMessageAsync(SendMessageDto dto);
    Task<MessageStatusDto?> GetMessageStatusAsync(string transactionId);
}
