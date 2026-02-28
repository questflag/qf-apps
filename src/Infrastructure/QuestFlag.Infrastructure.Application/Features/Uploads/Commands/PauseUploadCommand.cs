using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using QuestFlag.Infrastructure.Domain.Enums;
using QuestFlag.Infrastructure.Domain.Interfaces;

namespace QuestFlag.Infrastructure.Application.Features.Uploads.Commands;

public record PauseUploadCommand(Guid UploadId, Guid TenantId, Guid UserId, string Role) : IRequest;

public class PauseUploadCommandHandler : IRequestHandler<PauseUploadCommand>
{
    private readonly IUploadRepository _repository;

    public PauseUploadCommandHandler(IUploadRepository repository)
    {
        _repository = repository;
    }

    public async Task Handle(PauseUploadCommand request, CancellationToken cancellationToken)
    {
        var record = await _repository.GetByIdAsync(request.UploadId, cancellationToken);
        if (record == null || record.IsDeleted || record.TenantId != request.TenantId)
            return;

        // Check ownership or admin
        bool isAdmin = string.Equals(request.Role, UserRole.TenantAdmin, StringComparison.OrdinalIgnoreCase);
        if (!isAdmin && record.UserId != request.UserId)
            throw new UnauthorizedAccessException("You can only pause your own uploads.");

        if (record.Status == UploadStatus.Uploading || record.Status == UploadStatus.Pending)
        {
            record.Status = UploadStatus.Paused;
            await _repository.UpdateAsync(record, cancellationToken);
        }
    }
}
