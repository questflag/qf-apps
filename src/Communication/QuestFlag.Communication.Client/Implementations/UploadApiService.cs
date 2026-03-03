using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using QuestFlag.Communication.Client.Contracts;
using QuestFlag.Communication.Domain.DTOs;
using QuestFlag.Infrastructure.Domain.Models;

namespace QuestFlag.Communication.Client.Implementations;

public class UploadApiService : IUploadApiService
{
    private readonly HttpClient _httpClient;

    public UploadApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public void SetBearerToken(string token)
    {
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    public async Task<QuestFlag.Communication.Domain.DTOs.PagedResult<UploadRecordDto>> GetUploadsAsync(
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
        CancellationToken ct = default)
    {
        var query = new List<string>
        {
            $"SortBy={Uri.EscapeDataString(sortBy)}",
            $"SortDir={Uri.EscapeDataString(sortDir)}",
            $"Page={page}",
            $"PageSize={pageSize}"
        };

        if (!string.IsNullOrEmpty(tenantSlug)) query.Add($"TenantSlug={Uri.EscapeDataString(tenantSlug)}");
        if (!string.IsNullOrEmpty(userIdFilter)) query.Add($"UserIdFilter={Uri.EscapeDataString(userIdFilter)}");
        if (fromDate.HasValue) query.Add($"FromDate={Uri.EscapeDataString(fromDate.Value.ToString("O"))}");
        if (toDate.HasValue) query.Add($"ToDate={Uri.EscapeDataString(toDate.Value.ToString("O"))}");
        if (!string.IsNullOrEmpty(category)) query.Add($"Category={Uri.EscapeDataString(category)}");
        if (!string.IsNullOrEmpty(status)) query.Add($"Status={Uri.EscapeDataString(status)}");

        var queryString = string.Join("&", query);
        var url = $"/api/upload?{queryString}";

        var response = await _httpClient.GetAsync(url, ct);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<QuestFlag.Communication.Domain.DTOs.PagedResult<UploadRecordDto>>(cancellationToken: ct);
        return result!;
    }

    public async Task<Guid> UploadFileAsync(
        Stream fileStream,
        string fileName,
        string taskName,
        string category,
        string[]? tags = null,
        string? extraData = null,
        CancellationToken ct = default)
    {
        using var content = new MultipartFormDataContent();

        // 1. Add file content
        var fileContent = new StreamContent(fileStream);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
        content.Add(fileContent, "category", category ?? "default"); // Assuming category is a form field
        content.Add(new StringContent(taskName), "taskName");
        
        // Wait, the original code had:
        // content.Add(fileContent, "File", fileName);
        // content.Add(new StringContent(taskName), "TaskName");
        // content.Add(new StringContent(category ?? "default"), "Category");
        // But the controller expects:
        // [FromForm] string category, [FromForm] string taskName, [FromForm] string[]? tags, [FromForm] string? extraDataJson
        // And the files are in Request.Form.Files
        
        content.Add(fileContent, "files", fileName); // Controller uses Request.Form.Files
        content.Add(new StringContent(category ?? "default"), "category");
        content.Add(new StringContent(taskName), "taskName");
        
        if (tags != null && tags.Length > 0)
        {
            foreach (var tag in tags)
            {
                content.Add(new StringContent(tag), "tags");
            }
        }
        
        if (!string.IsNullOrWhiteSpace(extraData))
        {
            content.Add(new StringContent(extraData), "extraDataJson");
        }

        var response = await _httpClient.PostAsync("/api/upload", content, ct); // Controller route is [Route("api/[controller]")] which is /api/upload
        
        if (!response.IsSuccessStatusCode)
        {
            var err = await response.Content.ReadAsStringAsync(ct);
            throw new Exception($"Upload failed: {response.StatusCode} {err}");
        }

        var json = await response.Content.ReadFromJsonAsync<QuestFlag.Infrastructure.Domain.Models.ApiResponse<List<Guid>>>(cancellationToken: ct);
        return json?.Data?.FirstOrDefault() ?? Guid.Empty;
    }

    public async Task RetryUploadAsync(Guid id, CancellationToken ct = default)
    {
        var response = await _httpClient.PostAsync($"/api/upload/{id}/retry", null, ct);
        response.EnsureSuccessStatusCode();
    }

    public async Task PauseUploadAsync(Guid id, CancellationToken ct = default)
    {
        var response = await _httpClient.PostAsync($"/api/upload/{id}/pause", null, ct);
        response.EnsureSuccessStatusCode();
    }

    public async Task DeleteUploadAsync(Guid id, CancellationToken ct = default)
    {
        var response = await _httpClient.DeleteAsync($"/api/upload/{id}", ct);
        response.EnsureSuccessStatusCode();
    }

    public async Task<string> GetSignedDownloadUrlAsync(Guid id, CancellationToken ct = default)
    {
        var response = await _httpClient.GetAsync($"/api/upload/{id}/download", ct);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<QuestFlag.Infrastructure.Domain.Models.ApiResponse<string>>(cancellationToken: ct);
        return result?.Data ?? throw new Exception("URL missing from response");
    }
}
