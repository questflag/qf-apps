using System;
using System.Collections.Generic;

namespace QuestFlag.Communication.Shared.DTOs;


public class DownloadUrlResponse
{
    public string DownloadUrl { get; set; } = string.Empty;
}

public class UploadRecordDto
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid UserId { get; set; }
    public string OriginalFileName { get; set; } = string.Empty;
    public string StoredFileName { get; set; } = string.Empty;
    public string TaskName { get; set; } = string.Empty;
    public string Category { get; set; } = "default";
    public long SizeInBytes { get; set; }
    public string[] Tags { get; set; } = Array.Empty<string>();
    public Dictionary<string, string> ExtraData { get; set; } = new();
    public int Status { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? CompletedAtUtc { get; set; }

    public UploadRecordDto() { }

    public UploadRecordDto(Guid id, Guid tenantId, Guid userId, string originalFileName, string taskName, 
        string category, long sizeInBytes, string[] tags, Dictionary<string, string> extraData, int status, 
        string? errorMessage, DateTime createdAtUtc, DateTime? completedAtUtc)
    {
        Id = id;
        TenantId = tenantId;
        UserId = userId;
        OriginalFileName = originalFileName;
        TaskName = taskName;
        Category = category;
        SizeInBytes = sizeInBytes;
        Tags = tags;
        ExtraData = extraData;
        Status = status;
        ErrorMessage = errorMessage;
        CreatedAtUtc = createdAtUtc;
        CompletedAtUtc = completedAtUtc;
    }
}
