using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace QuestFlag.Infrastructure.Application.Features.Uploads.Commands;

public class FileItem
{
    public string OriginalFileName { get; set; } = string.Empty;
    public Stream FileStream { get; set; } = Stream.Null;
    public long SizeInBytes { get; set; }
    public string ContentType { get; set; } = string.Empty;
}

public record UploadBatchCommand(
    Guid TenantId,
    Guid UserId,
    string TenantSlug,
    string Category,
    string TaskName,
    List<FileItem> Files,
    string[] Tags,
    Dictionary<string, string> ExtraData) : IRequest<List<Guid>>;

public class UploadBatchCommandHandler : IRequestHandler<UploadBatchCommand, List<Guid>>
{
    private readonly IMediator _mediator;

    public UploadBatchCommandHandler(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task<List<Guid>> Handle(UploadBatchCommand request, CancellationToken cancellationToken)
    {
        var recordIds = new List<Guid>();

        foreach (var file in request.Files)
        {
            var command = new UploadFileCommand(
                request.TenantId,
                request.UserId,
                request.TenantSlug,
                file.OriginalFileName,
                request.Category,
                request.TaskName,
                file.FileStream,
                file.SizeInBytes,
                file.ContentType,
                request.Tags,
                request.ExtraData);

            var id = await _mediator.Send(command, cancellationToken);
            recordIds.Add(id);
        }

        return recordIds;
    }
}
