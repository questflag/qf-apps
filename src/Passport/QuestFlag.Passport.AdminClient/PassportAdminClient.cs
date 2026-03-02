using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;

namespace QuestFlag.Passport.AdminClient;

// ── DTOs ──────────────────────────────────────────────────────────────────────
public record TenantAdminDto(Guid Id, string Name, string Slug, bool IsActive, string? CustomDomain, string? SubdomainSlug);
public record UserAdminDto(Guid Id, Guid TenantId, string TenantName, string Username, string DisplayName, string Email, bool IsActive, bool TwoFactorEnabled, bool EmailConfirmed, string Role);
public record RoleDto(Guid Id, string Name);
public record DeviceAdminDto(Guid Id, string DeviceName, string IpAddress, DateTime TrustedAtUtc, DateTime ExpiresAtUtc);
public record AgentDto(string ClientId, string DisplayName, string Type, HashSet<string> Permissions, HashSet<Uri> RedirectUris, HashSet<Uri> PostLogoutRedirectUris);
public record CreateAgentRequest(string ClientId, string DisplayName, string? ClientSecret, string Type, HashSet<string> Permissions, HashSet<Uri> RedirectUris, HashSet<Uri> PostLogoutRedirectUris);
public record UpdateAgentRequest(string ClientId, string DisplayName, string? ClientSecret, string Type, HashSet<string> Permissions, HashSet<Uri> RedirectUris, HashSet<Uri> PostLogoutRedirectUris);

/// <summary>
/// Client SDK for privileged Passport admin endpoints.
/// Consumed exclusively by Passport.AdminWebApp. All methods require the caller
/// to attach a Bearer token with the `passport_admin` role.
/// </summary>
public class PassportAdminClient
{
    private readonly HttpClient _http;

    public PassportAdminClient(HttpClient http) => _http = http;

    // ── Tenants ────────────────────────────────────────────────────────────────

    public async Task<IReadOnlyList<TenantAdminDto>> GetTenantsAsync(CancellationToken ct = default)
    {
        var result = await _http.GetFromJsonAsync<List<TenantAdminDto>>("/api/tenants", ct);
        return result ?? [];
    }

    public async Task<Guid> CreateTenantAsync(string name, string slug, string? customDomain = null, string? subdomainSlug = null, CancellationToken ct = default)
    {
        var r = await _http.PostAsJsonAsync("/api/tenants", new { name, slug, customDomain, subdomainSlug }, ct);
        r.EnsureSuccessStatusCode();
        var result = await r.Content.ReadFromJsonAsync<IdResponse>(cancellationToken: ct);
        return result!.Id;
    }

    public async Task UpdateTenantAsync(Guid id, string name, string slug, bool isActive, string? customDomain = null, string? subdomainSlug = null, CancellationToken ct = default)
    {
        var r = await _http.PutAsJsonAsync($"/api/tenants/{id}", new { id, name, slug, isActive, customDomain, subdomainSlug }, ct);
        r.EnsureSuccessStatusCode();
    }

    public async Task DeleteTenantAsync(Guid id, CancellationToken ct = default)
    {
        var r = await _http.DeleteAsync($"/api/tenants/{id}", ct);
        r.EnsureSuccessStatusCode();
    }

    // ── Users ──────────────────────────────────────────────────────────────────

    public async Task<IReadOnlyList<UserAdminDto>> GetGlobalUsersAsync(string? searchTerm = null, CancellationToken ct = default)
    {
        var url = "/api/users";
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            url += $"?searchTerm={Uri.EscapeDataString(searchTerm)}";
        }
        var result = await _http.GetFromJsonAsync<List<UserAdminDto>>(url, ct);
        return result ?? [];
    }

    public async Task<IReadOnlyList<UserAdminDto>> GetUsersAsync(Guid tenantId, string? searchTerm = null, CancellationToken ct = default)
    {
        var url = $"/api/tenants/{tenantId}/users";
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            url += $"?searchTerm={Uri.EscapeDataString(searchTerm)}";
        }
        var result = await _http.GetFromJsonAsync<List<UserAdminDto>>(url, ct);
        return result ?? [];
    }

    /// <summary>Creates active user with password (admin-set). prefer InviteUserAsync for onboarding.</summary>
    public async Task<Guid> CreateUserAsync(Guid tenantId, string username, string email, string password, string displayName, string roleName, CancellationToken ct = default)
    {
        var r = await _http.PostAsJsonAsync($"/api/tenants/{tenantId}/users", new { username, email, password, displayName, roleName }, ct);
        r.EnsureSuccessStatusCode();
        var result = await r.Content.ReadFromJsonAsync<IdResponse>(cancellationToken: ct);
        return result!.Id;
    }

    /// <summary>Invites a user by email — creates inactive account, sends invite email.</summary>
    public async Task<Guid> InviteUserAsync(Guid tenantId, string username, string email, string displayName, string roleName, CancellationToken ct = default)
    {
        var r = await _http.PostAsJsonAsync($"/api/tenants/{tenantId}/users/invite", new { username, email, displayName, roleName }, ct);
        r.EnsureSuccessStatusCode();
        var result = await r.Content.ReadFromJsonAsync<IdResponse>(cancellationToken: ct);
        return result!.Id;
    }

    public async Task UpdateUserAsync(Guid tenantId, Guid userId, string username, string email, string displayName, bool isActive, string roleName, CancellationToken ct = default)
    {
        var r = await _http.PutAsJsonAsync($"/api/tenants/{tenantId}/users/{userId}", new { id = userId, username, email, displayName, isActive, roleName }, ct);
        r.EnsureSuccessStatusCode();
    }

    public async Task DeleteUserAsync(Guid tenantId, Guid userId, CancellationToken ct = default)
    {
        var r = await _http.DeleteAsync($"/api/tenants/{tenantId}/users/{userId}", ct);
        r.EnsureSuccessStatusCode();
    }

    // ── Roles ──────────────────────────────────────────────────────────────────

    public async Task<IReadOnlyList<RoleDto>> GetRolesAsync(CancellationToken ct = default)
    {
        var result = await _http.GetFromJsonAsync<List<RoleDto>>("/api/roles", ct);
        return result ?? [];
    }

    public async Task<Guid> CreateRoleAsync(string name, CancellationToken ct = default)
    {
        var r = await _http.PostAsJsonAsync("/api/roles", new { name }, ct);
        r.EnsureSuccessStatusCode();
        var result = await r.Content.ReadFromJsonAsync<IdResponse>(cancellationToken: ct);
        return result!.Id;
    }

    public async Task UpdateRoleAsync(Guid id, string name, CancellationToken ct = default)
    {
        var r = await _http.PutAsJsonAsync($"/api/roles/{id}", new { id, name }, ct);
        r.EnsureSuccessStatusCode();
    }

    public async Task DeleteRoleAsync(Guid id, CancellationToken ct = default)
    {
        var r = await _http.DeleteAsync($"/api/roles/{id}", ct);
        r.EnsureSuccessStatusCode();
    }

    // ── Sessions / Force Logout ────────────────────────────────────────────────

    /// <summary>Force logout — revokes all tokens + all trusted devices for the user.</summary>
    public async Task ForceLogoutUserAsync(Guid userId, CancellationToken ct = default)
    {
        var r = await _http.DeleteAsync($"/api/usersessions/{userId}", ct);
        r.EnsureSuccessStatusCode();
    }

    // ── Devices ────────────────────────────────────────────────────────────────

    public async Task<IReadOnlyList<DeviceAdminDto>> GetUserDevicesAsync(Guid userId, CancellationToken ct = default)
    {
        var result = await _http.GetFromJsonAsync<List<DeviceAdminDto>>($"/api/usersessions/{userId}/devices", ct);
        return result ?? [];
    }

    public async Task RevokeUserDeviceAsync(Guid deviceId, CancellationToken ct = default)
    {
        var r = await _http.DeleteAsync($"/api/usersessions/devices/{deviceId}", ct);
        r.EnsureSuccessStatusCode();
    }

    // ── Agents ──────────────────────────────────────────────────────────────────

    public async Task<IReadOnlyList<AgentDto>> GetAgentsAsync(CancellationToken ct = default)
    {
        var result = await _http.GetFromJsonAsync<List<AgentDto>>("/api/agents", ct);
        return result ?? [];
    }

    public async Task CreateAgentAsync(CreateAgentRequest request, CancellationToken ct = default)
    {
        var r = await _http.PostAsJsonAsync("/api/agents", request, ct);
        r.EnsureSuccessStatusCode();
    }

    public async Task UpdateAgentAsync(string clientId, UpdateAgentRequest request, CancellationToken ct = default)
    {
        var r = await _http.PutAsJsonAsync($"/api/agents/{clientId}", request, ct);
        r.EnsureSuccessStatusCode();
    }

    public async Task DeleteAgentAsync(string clientId, CancellationToken ct = default)
    {
        var r = await _http.DeleteAsync($"/api/agents/{clientId}", ct);
        r.EnsureSuccessStatusCode();
    }
}

file record IdResponse(Guid Id);
