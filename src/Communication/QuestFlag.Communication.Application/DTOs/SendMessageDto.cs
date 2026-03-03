using QuestFlag.Communication.Domain.Enums;

namespace QuestFlag.Communication.Application.DTOs;

public record SendMessageDto(
    string Recipient,
    ChannelType ChannelType,
    object? Payload,
    Guid TenantId);
