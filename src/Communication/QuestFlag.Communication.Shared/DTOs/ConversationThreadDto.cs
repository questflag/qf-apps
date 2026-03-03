using QuestFlag.Communication.Domain.Enums;
using QuestFlag.Communication.Domain.ValueObjects;

namespace QuestFlag.Communication.Shared.DTOs;

public record ConversationThreadDto(
    Guid Id,
    string TenantId,
    string AgentId,
    ConversationStatus Status,
    ChannelType ChannelUsed,
    List<Message> Messages,
    Analytics? Analytics,
    DateTime CreatedAt,
    DateTime? ClosedAt);
