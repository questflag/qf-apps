using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using QuestFlag.Infrastructure.Domain.Interfaces;

namespace QuestFlag.Infrastructure.Application.Features.Uploads.Queries;

public record GetSignedDownloadUrlQuery(Guid UploadId, Guid TenantId) : IRequest<string?>;

public class GetSignedDownloadUrlQueryHandler : IRequestHandler<GetSignedDownloadUrlQuery, string?>
{
    private readonly IUploadRepository _repository;
    private readonly IStorageService _storageService;

    public GetSignedDownloadUrlQueryHandler(
        IUploadRepository repository,
        IStorageService storageService)
    {
        _repository = repository;
        _storageService = storageService;
    }

    public async Task<string?> Handle(GetSignedDownloadUrlQuery request, CancellationToken cancellationToken)
    {
        // 1. Get record and ensure tenant access
        var record = await _repository.GetByIdAsync(request.UploadId, cancellationToken);
        if (record == null || record.IsDeleted || record.TenantId != request.TenantId)
        {
            return null; // Not found or denied
        }

        // 2. Generate expirable signed URL (e.g. 1 hour)
        var expiration = TimeSpan.FromHours(1);
        var signedUrl = await _storageService.GetSignedDownloadUrlAsync(
            record.BucketName, 
            record.ObjectKey, 
            expiration, 
            cancellationToken);

        return signedUrl;
    }
}
