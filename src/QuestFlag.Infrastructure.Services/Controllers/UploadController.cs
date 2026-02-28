using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QuestFlag.Infrastructure.Application.Common.DTOs;
using QuestFlag.Infrastructure.Application.Common.Models;
using QuestFlag.Infrastructure.Application.Features.Uploads.Commands;
using QuestFlag.Infrastructure.Application.Features.Uploads.Queries;
using QuestFlag.Infrastructure.Services.Extensions;
using QuestFlag.Infrastructure.Services.Models;

namespace QuestFlag.Infrastructure.Services.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // Requires valid Passport JWT
public class UploadController : ControllerBase
{
    private readonly IMediator _mediator;

    public UploadController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    [RequestSizeLimit(long.MaxValue)]
    [RequestFormLimits(MultipartBodyLengthLimit = long.MaxValue)]
    public async Task<ActionResult<ApiResponse<List<Guid>>>> UploadFiles(
        [FromForm] string category,
        [FromForm] string taskName,
        [FromForm] string[]? tags,
        [FromForm] string? extraDataJson)
    {
        if (Request.Form.Files.Count == 0)
            return BadRequest(ApiResponse<List<Guid>>.Fail("No files provided."));

        var extraData = new Dictionary<string, string>();
        if (!string.IsNullOrWhiteSpace(extraDataJson))
        {
            try
            {
                extraData = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(extraDataJson) ?? new();
            }
            catch { /* Ignore invalid JSON */ }
        }

        var files = new List<FileItem>();
        foreach (var formFile in Request.Form.Files)
        {
            var ms = new MemoryStream();
            await formFile.CopyToAsync(ms);
            ms.Position = 0; // Reset for reading later

            files.Add(new FileItem
            {
                OriginalFileName = formFile.FileName,
                ContentType = formFile.ContentType,
                SizeInBytes = formFile.Length,
                FileStream = ms
            });
        }

        var command = new UploadBatchCommand(
            User.GetTenantId(),
            User.GetUserId(),
            User.GetTenantSlug(),
            category,
            taskName,
            files,
            tags ?? Array.Empty<string>(),
            extraData
        );

        var result = await _mediator.Send(command);
        return Ok(ApiResponse<List<Guid>>.Ok(result));
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<UploadRecordDto>>>> GetUploads(
        [FromQuery] Guid? userId,
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        [FromQuery] string? category,
        [FromQuery] string? status,
        [FromQuery] string sortBy = "CreatedAtUtc",
        [FromQuery] string sortDir = "desc",
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var tenantId = User.GetTenantId();
        var currentUserId = User.GetUserId();
        var role = User.GetRole();

        // Determine user filter. A normal user can only filter by their own ID, or sees no one else.
        var effectiveUserIdFilter = role == "tenant_admin" ? userId : currentUserId;

        var query = new GetUploadsQuery(
            tenantId,
            currentUserId,
            role,
            effectiveUserIdFilter,
            fromDate,
            toDate,
            category,
            status,
            sortBy,
            sortDir,
            page,
            pageSize);

        var result = await _mediator.Send(query);
        return Ok(ApiResponse<PagedResult<UploadRecordDto>>.Ok(result));
    }

    [HttpGet("{id}/download")]
    public async Task<ActionResult<ApiResponse<string>>> GetDownloadUrl(Guid id)
    {
        var query = new GetSignedDownloadUrlQuery(id, User.GetTenantId());
        var url = await _mediator.Send(query);

        if (url == null) return NotFound(ApiResponse<string>.Fail("Upload not found or access denied."));

        return Ok(ApiResponse<string>.Ok(url));
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = "TenantAdmin")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteUpload(Guid id)
    {
        var command = new DeleteUploadCommand(id, User.GetTenantId(), User.GetRole(), User.GetUserId().ToString());
        await _mediator.Send(command);
        
        return Ok(ApiResponse<bool>.Ok(true));
    }

    [HttpPost("{id}/pause")]
    public async Task<ActionResult<ApiResponse<bool>>> PauseUpload(Guid id)
    {
        var command = new PauseUploadCommand(id, User.GetTenantId(), User.GetUserId(), User.GetRole());
        await _mediator.Send(command);
        
        return Ok(ApiResponse<bool>.Ok(true));
    }

    [HttpPost("{id}/retry")]
    public async Task<ActionResult<ApiResponse<bool>>> RetryUpload(Guid id)
    {
        var command = new RetryUploadCommand(id, User.GetTenantId(), User.GetUserId(), User.GetRole());
        await _mediator.Send(command);
        
        return Ok(ApiResponse<bool>.Ok(true));
    }
}
