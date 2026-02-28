using System;

namespace QuestFlag.Infrastructure.Domain.Events;

public record UploadCompletedEvent(
    Guid UploadId,
    Guid TenantId,
    Guid UserId,
    string TaskName,
    string ObjectKey,
    long SizeInBytes,
    DateTime CompletedAtUtc
);
