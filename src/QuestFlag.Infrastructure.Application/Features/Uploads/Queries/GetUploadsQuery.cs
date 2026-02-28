using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using QuestFlag.Infrastructure.Application.Common.DTOs;
using QuestFlag.Infrastructure.Application.Common.Models;
using QuestFlag.Infrastructure.Domain.Interfaces;

namespace QuestFlag.Infrastructure.Application.Features.Uploads.Queries;

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
    int PageSize = 10) : IRequest<PagedResult<UploadRecordDto>>;

public class GetUploadsQueryHandler : IRequestHandler<GetUploadsQuery, PagedResult<UploadRecordDto>>
{
    private readonly IUploadRepository _repository;

    public GetUploadsQueryHandler(IUploadRepository repository)
    {
        _repository = repository;
    }

    public async Task<PagedResult<UploadRecordDto>> Handle(GetUploadsQuery request, CancellationToken cancellationToken)
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

        return new PagedResult<UploadRecordDto>(dtos, totalCount, request.PageIndex, request.PageSize);
    }
}
