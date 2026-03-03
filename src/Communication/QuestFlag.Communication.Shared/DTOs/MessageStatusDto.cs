using QuestFlag.Communication.Domain.Enums;

namespace QuestFlag.Communication.Shared.DTOs;

public record MessageStatusDto(
    string TransactionId,
    MessageStatus Status,
    DateTime UpdatedAt);
