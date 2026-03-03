using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace QuestFlag.Passport.UserClient;

// ── Response models ────────────────────────────────────────────────────────────
public record TenantDto(Guid Id, string Name, string Slug);
public record ResolvedTenantDto(Guid Id, string Name, string Slug);
public record TokenResponse(
    [property: JsonPropertyName("access_token")]  string AccessToken,
    [property: JsonPropertyName("token_type")]    string TokenType,
    [property: JsonPropertyName("expires_in")]    int ExpiresIn,
    [property: JsonPropertyName("refresh_token")] string? RefreshToken,
    [property: JsonPropertyName("id_token")]      string? IdToken
);

public record UserProfileDto(Guid Id, string Email, string Name, bool EmailConfirmed, string? PhoneNumber, bool TwoFactorEnabled);
public record UserSummaryDto(Guid Id, string DisplayName, string Username);
public record DeviceDto(Guid Id, string DeviceName, string IpAddress, DateTime TrustedAtUtc, DateTime ExpiresAtUtc);
public record SetupPhoneRequest(string PhoneNumber);
public record VerifyPhoneRequest(string OtpCode);

/// <summary>
/// Client SDK for user-facing and anonymous Passport endpoints.
/// Consumed by Passport.WebApp (SSO portal) and Infrastructure.WebApp (profile pages).
/// Does NOT expose any admin-only APIs.
/// </summary>
public class PassportUserClient(HttpClient http)
{
    private readonly HttpClient _http = http;

    // ── Anonymous APIs ─────────────────────────────────────────────────────────

    /// <summary>Lists all active tenants (for the login page dropdown).</summary>
    public async Task<IReadOnlyList<TenantDto>> GetTenantsAsync(CancellationToken ct = default)
    {
        var result = await _http.GetFromJsonAsync<List<TenantDto>>("/api/tenants", ct);
        return result ?? [];
    }

    /// <summary>Authenticates a user and returns tokens. tenantSlug is appended to username per convention.</summary>
    public async Task<TokenResponse?> LoginAsync(string tenantSlug, string username, string password, CancellationToken ct = default)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "/connect/token");
        request.Content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["grant_type"] = "password",
            ["username"] = username + "@" + tenantSlug,
            ["password"] = password,
            ["scope"] = "openid offline_access roles"
        });
        var response = await _http.SendAsync(request, ct);
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync(ct);
            throw new Exception($"Login failed: {error}");
        }
        return await response.Content.ReadFromJsonAsync<TokenResponse>(cancellationToken: ct);
    }

    /// <summary>Exchanges a refresh token for a new token set.</summary>
    public async Task<TokenResponse?> RefreshTokenAsync(string refreshToken, CancellationToken ct = default)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "/connect/token");
        request.Content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["grant_type"] = "refresh_token",
            ["refresh_token"] = refreshToken,
            ["scope"] = "openid offline_access roles"
        });
        var response = await _http.SendAsync(request, ct);
        if (!response.IsSuccessStatusCode)
            throw new Exception("Token refresh failed.");
        return await response.Content.ReadFromJsonAsync<TokenResponse>(cancellationToken: ct);
    }

    /// <summary>Lists summary of users belonging to the given tenant (for filter dropdowns).</summary>
    public async Task<IReadOnlyList<UserSummaryDto>> GetUsersByTenantAsync(Guid tenantId, CancellationToken ct = default)
    {
        var response = await _http.GetAsync($"/api/passport/tenants/{tenantId}/users", ct);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<List<UserSummaryDto>>(cancellationToken: ct);
        return result ?? [];
    }

    /// <summary>Resolves a tenant from a Host header value (for tenant-aware login URL).</summary>
    public async Task<ResolvedTenantDto?> ResolveTenantByDomainAsync(string domain, CancellationToken ct = default)
    {
        var response = await _http.GetAsync($"/api/tenants/resolve?domain={Uri.EscapeDataString(domain)}", ct);
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<ResolvedTenantDto>(cancellationToken: ct);
    }

    /// <summary>Sends a password-reset link to the given email.</summary>
    public async Task ForgotPasswordAsync(string email, CancellationToken ct = default)
    {
        await _http.PostAsJsonAsync("/account/forgot-password", new { email }, ct);
    }

    /// <summary>Resets password using the token from the reset email.</summary>
    public async Task<bool> ResetPasswordAsync(Guid userId, string token, string newPassword, CancellationToken ct = default)
    {
        var r = await _http.PostAsJsonAsync("/account/reset-password", new { userId, token, newPassword }, ct);
        return r.IsSuccessStatusCode;
    }

    /// <summary>Verifies the email invite token and sets the initial password (new user onboarding).</summary>
    public async Task<bool> VerifyEmailAsync(Guid userId, string token, string password, CancellationToken ct = default)
    {
        var r = await _http.PostAsJsonAsync("/account/verify-email", new { userId, token, password }, ct);
        return r.IsSuccessStatusCode;
    }

    /// <summary>Sends the login-time 2FA OTP to the user's phone.</summary>
    public async Task SendLoginOtpAsync(Guid userId, CancellationToken ct = default)
    {
        await _http.PostAsJsonAsync("/account/two-factor/send-login-otp", new { userId }, ct);
    }

    /// <summary>Verifies the login-time 2FA OTP.</summary>
    public async Task<bool> VerifyLoginOtpAsync(Guid userId, string otp, CancellationToken ct = default)
    {
        var r = await _http.PostAsJsonAsync("/account/two-factor/verify-login-otp", new { userId, otp }, ct);
        return r.IsSuccessStatusCode;
    }

    // ── Authenticated user APIs (caller must set Authorization header) ─────────

    public async Task<UserProfileDto?> GetUserInfoAsync(CancellationToken ct = default)
    {
        return await _http.GetFromJsonAsync<UserProfileDto>("/account/profile", ct);
    }

    /// <summary>Starts phone 2FA enrollment by sending an OTP.</summary>
    public async Task<bool> SetupPhoneTwoFactorAsync(SetupPhoneRequest req, CancellationToken ct = default)
    {
        var r = await _http.PostAsJsonAsync("/account/two-factor/enable-phone", req, ct);
        return r.IsSuccessStatusCode;
    }

    /// <summary>Confirms the enrollment OTP and activates 2FA on the account.</summary>
    public async Task<bool> VerifyPhoneTwoFactorAsync(VerifyPhoneRequest req, CancellationToken ct = default)
    {
        var r = await _http.PostAsJsonAsync("/account/two-factor/verify-phone", req, ct);
        return r.IsSuccessStatusCode;
    }

    /// <summary>Disables 2FA on the account.</summary>
    public async Task<bool> DisableTwoFactorAsync(CancellationToken ct = default)
    {
        var r = await _http.PostAsync("/account/two-factor/disable", null, ct);
        return r.IsSuccessStatusCode;
    }

    /// <summary>Lists the current user's active trusted devices.</summary>
    public async Task<IReadOnlyList<DeviceDto>> GetMyDevicesAsync(CancellationToken ct = default)
    {
        var result = await _http.GetFromJsonAsync<List<DeviceDto>>("/api/devices", ct);
        return result ?? [];
    }

    /// <summary>Revokes trust for a specific device.</summary>
    public async Task<bool> RevokeDeviceAsync(Guid deviceId, CancellationToken ct = default)
    {
        var r = await _http.DeleteAsync($"/api/devices/{deviceId}", ct);
        return r.IsSuccessStatusCode;
    }

    /// <summary>Revokes all of the current user's trusted devices.</summary>
    public async Task RevokeAllMyDevicesAsync(CancellationToken ct = default)
    {
        await _http.DeleteAsync("/api/devices", ct);
    }
}
