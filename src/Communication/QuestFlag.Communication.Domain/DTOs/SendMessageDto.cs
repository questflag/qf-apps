using QuestFlag.Communication.Domain.Enums;

namespace QuestFlag.Communication.Domain.DTOs;

public record SendMessageDto(
    string Recipient,
    ChannelType ChannelType,
    object? Payload,
    Guid TenantId);
