using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using QuestFlag.Communication.Application.DTOs;
using QuestFlag.Communication.Domain.Contracts;

namespace QuestFlag.Communication.Application.Features.Uploads.Queries;

public record GetUploadsQuery(
    Guid TenantId,
    Guid? UserId,
    string Role,
    Guid? UserIdFilter = null,
    DateTime? FromDate = null,
    DateTime? ToDate = null,
    string? Category = null,
    string? Status = null,
    string SortBy = "CreatedAtUtc",
    string SortDir = "desc",
    int PageIndex = 1,
    int PageSize = 10) : IRequest<(IReadOnlyList<UploadRecordDto> Items, int TotalCount)>;

public class GetUploadsQueryHandler : IRequestHandler<GetUploadsQuery, (IReadOnlyList<UploadRecordDto> Items, int TotalCount)>
{
    private readonly IUploadRepository _repository;

    public GetUploadsQueryHandler(IUploadRepository repository)
    {
        _repository = repository;
    }

    public async Task<(IReadOnlyList<UploadRecordDto> Items, int TotalCount)> Handle(GetUploadsQuery request, CancellationToken cancellationToken)
    {
        bool descending = request.SortDir.Equals("desc", StringComparison.OrdinalIgnoreCase);

        // Map request to repository
        var (items, totalCount) = await _repository.GetListAsync(
            request.TenantId,
            request.UserIdFilter,
            request.FromDate,
            request.ToDate,
            request.Category,
            request.Status,
            request.Role,
            request.SortBy,
            descending,
            request.PageIndex,
            request.PageSize,
            cancellationToken);

        // Map to DTOs
        var dtos = items.Select(r => new UploadRecordDto(
            r.Id,
            r.TenantId,
            r.UserId,
            r.OriginalFileName,
            r.TaskName,
            r.Category,
            r.SizeInBytes,
            r.Tags,
            r.ExtraData,
            r.Status,
            r.ErrorMessage,
            r.CreatedAtUtc,
            r.CompletedAtUtc
        )).ToList();

        return (dtos, totalCount);
    }
}
