using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;

namespace QuestFlag.Infrastructure.Client;

// Redefining basic DTOs for the client to parse JSON without Application references
public record UploadRecordDto(
    Guid Id,
    Guid TenantId,
    Guid UserId,
    string TaskName,
    string OriginalFileName,
    string StoredFileName,
    string Category,
    long FileSizeBytes,
    int Status,
    string[] Tags,
    DateTime CreatedAtUtc,
    DateTime? UploadCompletedAtUtc
);

public record PagedResult<T>(IReadOnlyList<T> Items, int TotalCount, int Page, int PageSize);

public record DownloadUrlResponse(string Url, DateTime ExpiresAtUtc);

public class UploadApiService
{
    private readonly HttpClient _httpClient;

    public UploadApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    /// <summary>
    /// Authenticate the client with a Bearer token received from Passport API.
    /// In Blazor WA, this might be handled by an HttpMessageHandler (DelegatingHandler).
    /// </summary>
    public void SetBearerToken(string token)
    {
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    public async Task<PagedResult<UploadRecordDto>> GetUploadsAsync(
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

        var result = await response.Content.ReadFromJsonAsync<PagedResult<UploadRecordDto>>(cancellationToken: ct);
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
        content.Add(fileContent, "File", fileName);

        // 2. Add metadata
        content.Add(new StringContent(taskName), "TaskName");
        content.Add(new StringContent(category ?? "default"), "Category");
        
        if (tags != null && tags.Length > 0)
        {
            foreach (var tag in tags)
            {
                content.Add(new StringContent(tag), "Tags");
            }
        }
        
        if (!string.IsNullOrWhiteSpace(extraData))
        {
            content.Add(new StringContent(extraData), "ExtraData");
        }

        var response = await _httpClient.PostAsync("/api/upload/file", content, ct);
        
        if (!response.IsSuccessStatusCode)
        {
            var err = await response.Content.ReadAsStringAsync(ct);
            throw new Exception($"Upload failed: {response.StatusCode} {err}");
        }

        var json = await response.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>(cancellationToken: ct);
        return json.GetProperty("data").GetProperty("recordId").GetGuid();
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

        var result = await response.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>(cancellationToken: ct);
        var urlStr = result.GetProperty("data").GetProperty("url").GetString();
        return urlStr ?? throw new Exception("URL missing from response");
    }
}
