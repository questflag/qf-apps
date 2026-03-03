using QuestFlag.Communication.Domain.Enums;

namespace QuestFlag.Communication.Domain.DTOs;

public record MessageStatusDto(
    string TransactionId,
    MessageStatus Status,
    DateTime UpdatedAt);
