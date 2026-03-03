namespace QuestFlag.Communication.Client.DTOs;

public record UploadRecordDto(
    Guid Id,
    Guid TenantId,
    Guid UserId,
    string TaskName,
    string OriginalFileName,
    string StoredFileName,
    string Category,
    long FileSizeBytes,
    int Status,
    string[] Tags,
    DateTime CreatedAtUtc,
    DateTime? UploadCompletedAtUtc
);
