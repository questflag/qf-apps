using QuestFlag.Communication.Domain.Enums;

namespace QuestFlag.Communication.Application.Common.DTOs;

public record SendMessageDto(
    string Recipient,
    ChannelType ChannelType,
    object? Payload,
    Guid TenantId);

public record MessageStatusDto(
    string TransactionId,
    MessageStatus Status,
    DateTime UpdatedAt);
