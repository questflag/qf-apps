using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using QuestFlag.Infrastructure.Domain.Enums;
using QuestFlag.Infrastructure.Domain.Interfaces;

namespace QuestFlag.Infrastructure.Application.Features.Uploads.Commands;

public record DeleteUploadCommand(
    Guid UploadId,
    Guid TenantId,
    string Role,
    string DeletedByUserId) : IRequest;

public class DeleteUploadCommandHandler : IRequestHandler<DeleteUploadCommand>
{
    private readonly IUploadRepository _repository;
    private readonly IStorageService _storageService;

    public DeleteUploadCommandHandler(
        IUploadRepository repository,
        IStorageService storageService)
    {
        _repository = repository;
        _storageService = storageService;
    }

    public async Task Handle(DeleteUploadCommand request, CancellationToken cancellationToken)
    {
        // 1. Enforce Role: Only tenant admin can delete
        if (!string.Equals(request.Role, UserRole.TenantAdmin, StringComparison.OrdinalIgnoreCase))
        {
            throw new UnauthorizedAccessException("Only Tenant Admins can delete upload records.");
        }

        var record = await _repository.GetByIdAsync(request.UploadId, cancellationToken);
        if (record == null || record.IsDeleted || record.TenantId != request.TenantId)
        {
            return; // Not found or already deleted or cross-tenant access attempt
        }

        // 2. Soft delete in DB
        await _repository.DeleteAsync(request.UploadId, request.DeletedByUserId, cancellationToken);

        // 3. Hard delete from Storage
        try
        {
            await _storageService.DeleteObjectAsync(record.BucketName, record.ObjectKey, cancellationToken);
        }
        catch (Exception)
        {
            // Log warning: orphaned file in storage, but DB is updated.
        }
    }
}
