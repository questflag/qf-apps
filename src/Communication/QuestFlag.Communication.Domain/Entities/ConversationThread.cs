using QuestFlag.Communication.Domain.Enums;
using QuestFlag.Communication.Domain.ValueObjects;

namespace QuestFlag.Communication.Domain.Entities;

public class ConversationThread
{
    public Guid Id { get; set; }
    public string TenantId { get; set; } = string.Empty;
    public string AgentId { get; set; } = string.Empty;
    public ConversationStatus Status { get; set; }
    public ChannelType ChannelUsed { get; set; }
    public string ProviderId { get; set; } = string.Empty;
    public List<Message> Messages { get; set; } = new();
    public Analytics? Analytics { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ClosedAt { get; set; }
}
