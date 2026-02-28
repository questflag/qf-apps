using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using QuestFlag.Infrastructure.Domain.Enums;
using QuestFlag.Infrastructure.Domain.Interfaces;

namespace QuestFlag.Infrastructure.Application.Features.Uploads.Commands;

public record RetryUploadCommand(Guid UploadId, Guid TenantId, Guid UserId, string Role) : IRequest;

public class RetryUploadCommandHandler : IRequestHandler<RetryUploadCommand>
{
    private readonly IUploadRepository _repository;

    public RetryUploadCommandHandler(IUploadRepository repository)
    {
        _repository = repository;
    }

    public async Task Handle(RetryUploadCommand request, CancellationToken cancellationToken)
    {
        var record = await _repository.GetByIdAsync(request.UploadId, cancellationToken);
        if (record == null || record.IsDeleted || record.TenantId != request.TenantId)
            return;

        // Check ownership or admin
        bool isAdmin = string.Equals(request.Role, UserRole.TenantAdmin, StringComparison.OrdinalIgnoreCase);
        if (!isAdmin && record.UserId != request.UserId)
            throw new UnauthorizedAccessException("You can only retry your own uploads.");

        // We can only retry if paused or failed
        if (record.Status == UploadStatus.Failed || record.Status == UploadStatus.Paused)
        {
            record.Status = UploadStatus.Pending;
            record.ErrorMessage = null;
            record.RetryCount++;
            await _repository.UpdateAsync(record, cancellationToken);
        }
    }
}
