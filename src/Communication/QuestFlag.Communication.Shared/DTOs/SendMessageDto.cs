using QuestFlag.Communication.Domain.Enums;

namespace QuestFlag.Communication.Shared.DTOs;

public record SendMessageDto(
    string Recipient,
    ChannelType ChannelType,
    object? Payload,
    Guid TenantId);
