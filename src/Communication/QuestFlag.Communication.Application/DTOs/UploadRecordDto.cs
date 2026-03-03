using QuestFlag.Communication.Domain.Enums;

namespace QuestFlag.Communication.Application.DTOs;

public record UploadRecordDto(
    Guid Id,
    Guid TenantId,
    Guid UserId,
    string OriginalFileName,
    string TaskName,
    string Category,
    long SizeInBytes,
    string[] Tags,
    Dictionary<string, string> ExtraData,
    UploadStatus Status,
    string? ErrorMessage,
    DateTime CreatedAtUtc,
    DateTime? CompletedAtUtc
);
