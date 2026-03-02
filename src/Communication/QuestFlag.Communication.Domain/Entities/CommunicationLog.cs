using QuestFlag.Communication.Domain.Enums;

namespace QuestFlag.Communication.Domain.Entities;

public class CommunicationLog
{
    public Guid Id { get; set; }
    public string TransactionId { get; set; } = string.Empty;
    public string Recipient { get; set; } = string.Empty;
    public MessageStatus Status { get; set; }
    public ChannelType ChannelUsed { get; set; }
    public string ProviderId { get; set; } = string.Empty;
    public object? Payload { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
