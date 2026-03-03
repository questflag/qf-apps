using QuestFlag.Communication.Domain.Enums;

namespace QuestFlag.Communication.Application.DTOs;

public record MessageStatusDto(
    string TransactionId,
    MessageStatus Status,
    DateTime UpdatedAt);
