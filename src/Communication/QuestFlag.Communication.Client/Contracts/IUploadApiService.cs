using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using QuestFlag.Communication.Domain.DTOs;

namespace QuestFlag.Communication.Client.Contracts;

public interface IUploadApiService
{
    void SetBearerToken(string token);

    Task<QuestFlag.Communication.Domain.DTOs.PagedResult<UploadRecordDto>> GetUploadsAsync(
        string? tenantSlug = null,
        string? userIdFilter = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        string? category = null,
        string? status = null,
        string sortBy = "CreatedAtUtc",
        string sortDir = "desc",
        int page = 1,
        int pageSize = 50,
        CancellationToken ct = default);

    Task<Guid> UploadFileAsync(
        Stream fileStream,
        string fileName,
        string taskName,
        string category,
        string[]? tags = null,
        string? extraData = null,
        CancellationToken ct = default);

    Task RetryUploadAsync(Guid id, CancellationToken ct = default);
    Task PauseUploadAsync(Guid id, CancellationToken ct = default);
    Task DeleteUploadAsync(Guid id, CancellationToken ct = default);
    Task<string> GetSignedDownloadUrlAsync(Guid id, CancellationToken ct = default);
}
