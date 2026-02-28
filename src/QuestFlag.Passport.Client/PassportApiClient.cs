using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace QuestFlag.Passport.Client;

public record TokenResponse(
    [property: JsonPropertyName("access_token")] string AccessToken,
    [property: JsonPropertyName("token_type")] string TokenType,
    [property: JsonPropertyName("expires_in")] int ExpiresIn,
    [property: JsonPropertyName("refresh_token")] string RefreshToken,
    [property: JsonPropertyName("id_token")] string IdToken
);

public record TenantDto(Guid Id, string Name, string Slug);
public record UserSummaryDto(Guid Id, string DisplayName, string Username);

/// <summary>
/// Typed HttpClient for communicating with Passport.Services API.
/// </summary>
public class PassportApiClient
{
    private readonly HttpClient _httpClient;

    public PassportApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<TokenResponse?> LoginAsync(string tenantSlug, string username, string password, CancellationToken ct = default)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "/connect/token");
        request.Content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["grant_type"] = "password",
            ["username"] = username + "@" + tenantSlug, // Convention used by our Auth query
            ["password"] = password,
            ["scope"] = "openid offline_access roles"
        });

        var response = await _httpClient.SendAsync(request, ct);
        
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync(ct);
            throw new Exception($"Login failed: {error}");
        }

        return await response.Content.ReadFromJsonAsync<TokenResponse>(cancellationToken: ct);
    }

    public async Task<TokenResponse?> RefreshTokenAsync(string refreshToken, CancellationToken ct = default)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "/connect/token");
        request.Content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["grant_type"] = "refresh_token",
            ["refresh_token"] = refreshToken,
            ["scope"] = "openid offline_access roles"
        });

        var response = await _httpClient.SendAsync(request, ct);
        
        if (!response.IsSuccessStatusCode)
        {
            throw new Exception("Token refresh failed.");
        }

        return await response.Content.ReadFromJsonAsync<TokenResponse>(cancellationToken: ct);
    }

    public async Task<IReadOnlyList<TenantDto>> GetTenantsAsync(CancellationToken ct = default)
    {
        var response = await _httpClient.GetAsync("/api/passport/tenants", ct);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<List<TenantDto>>(cancellationToken: ct);
        return result ?? new List<TenantDto>();
    }

    public async Task<IReadOnlyList<UserSummaryDto>> GetUsersByTenantAsync(Guid tenantId, CancellationToken ct = default)
    {
        var response = await _httpClient.GetAsync($"/api/passport/tenants/{tenantId}/users", ct);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<List<UserSummaryDto>>(cancellationToken: ct);
        return result ?? new List<UserSummaryDto>();
    }
}
