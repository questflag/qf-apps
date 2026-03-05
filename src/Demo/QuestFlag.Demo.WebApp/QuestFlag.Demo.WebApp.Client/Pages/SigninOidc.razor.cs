using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.JSInterop;
using QuestFlag.Demo.WebApp.Client.State;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace QuestFlag.Demo.WebApp.Client.Pages;

public partial class SigninOidc
{
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender) return;

        var uri = Nav.ToAbsoluteUri(Nav.Uri);
        var query = QueryHelpers.ParseQuery(uri.Query);

        if (!query.TryGetValue("code", out var codeValues) || string.IsNullOrEmpty(codeValues))
        {
            Nav.NavigateTo("/login?ErrorMessage=No+authorization+code+received");
            return;
        }

        var code = codeValues.ToString();
        try
        {
            var authority = Configuration["Oidc:Authority"];
            var clientId = Configuration["Oidc:ClientId"];
            var redirectUri = Nav.ToAbsoluteUri("signin-oidc").ToString();

            if (string.IsNullOrEmpty(authority) || string.IsNullOrEmpty(clientId))
            {
                Nav.NavigateTo("/login?ErrorMessage=OIDC+configuration+is+missing");
                return;
            }

            // Get PKCE verifier stored before SSO redirect
            var codeVerifier = await JS.InvokeAsync<string?>("localStorage.getItem", "code_verifier");
            var tokenResponse = await ExchangeCodeForToken(authority, clientId, code, redirectUri, codeVerifier);

            if (tokenResponse == null || string.IsNullOrEmpty(tokenResponse.AccessToken))
            {
                Nav.NavigateTo("/login?ErrorMessage=Token+exchange+failed");
                return;
            }

            // Remove PKCE verifier — no longer needed
            await JS.InvokeVoidAsync("localStorage.removeItem", "code_verifier");

            // Parse user info from token
            var userInfo = ParseUserInfoFromToken(tokenResponse.AccessToken);
            if (userInfo == null)
            {
                Nav.NavigateTo("/login?ErrorMessage=Could+not+parse+token");
                return;
            }

            // Sign in via the auth provider (singleton) — stores to localStorage
            // and calls NotifyAuthenticationStateChanged so MainLayout re-renders without a page reload
            var authProvider = (PersistentAuthenticationStateProvider)AuthStateProvider;
            await authProvider.SignInAsync(userInfo, tokenResponse.AccessToken);

            // Client-side nav (no forceLoad) — the singleton auth provider already has the user,
            // so MainLayout's AuthorizeView will now render AuthenticatedLayout
            Nav.NavigateTo("/", forceLoad: false);
        }
        catch (Exception ex)
        {
            Nav.NavigateTo($"/login?ErrorMessage={Uri.EscapeDataString(ex.Message)}");
        }
    }

    private UserInfo? ParseUserInfoFromToken(string token)
    {
        try
        {
            var parts = token.Split('.');
            if (parts.Length < 2) return null;

            var payload = parts[1];
            var jsonBytes = ParseBase64WithoutPadding(payload);
            var claims = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(jsonBytes);
            if (claims == null) return null;

            var info = new UserInfo
            {
                UserId = claims.TryGetValue("sub", out var sub) ? sub.GetString() : null,
                Name = claims.TryGetValue("name", out var name) ? name.GetString() : null,
                Email = claims.TryGetValue("email", out var email) ? email.GetString() : null
            };

            // Handle both 'role' (singular, OpenIddict default) and 'roles' (plural)
            var roleKey = claims.ContainsKey("role") ? "role"
                        : claims.ContainsKey("roles") ? "roles"
                        : null;

            if (roleKey != null && claims.TryGetValue(roleKey, out var rolesEl))
            {
                if (rolesEl.ValueKind == JsonValueKind.Array)
                {
                    info.Roles = rolesEl.EnumerateArray()
                        .Select(x => x.GetString() ?? "")
                        .Where(x => !string.IsNullOrEmpty(x))
                        .ToArray();
                }
                else if (rolesEl.ValueKind == JsonValueKind.String)
                {
                    var r = rolesEl.GetString() ?? "";
                    info.Roles = r.Length > 0 ? new[] { r } : Array.Empty<string>();
                }
            }

            return info;
        }
        catch
        {
            return null;
        }
    }

    private byte[] ParseBase64WithoutPadding(string base64)
    {
        base64 = base64.Replace('-', '+').Replace('_', '/');
        switch (base64.Length % 4)
        {
            case 2: base64 += "=="; break;
            case 3: base64 += "="; break;
        }
        return Convert.FromBase64String(base64);
    }

    private async Task<TokenResponse?> ExchangeCodeForToken(string authority, string clientId, string code, string redirectUri, string? codeVerifier)
    {
        var tokenEndpoint = $"{authority}/connect/token";

        var parameters = new List<KeyValuePair<string, string>>
        {
            new("grant_type", "authorization_code"),
            new("client_id", clientId),
            new("code", code),
            new("redirect_uri", redirectUri),
        };

        if (!string.IsNullOrEmpty(codeVerifier))
            parameters.Add(new("code_verifier", codeVerifier));

        var content = new FormUrlEncodedContent(parameters);

        using var client = new HttpClient();
        var response = await client.PostAsync(tokenEndpoint, content);
        if (!response.IsSuccessStatusCode) return null;

        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<TokenResponse>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    }

    public class TokenResponse
    {
        public string? AccessToken { get; set; }
        public string? TokenType { get; set; }
        public int ExpiresIn { get; set; }
        public string? RefreshToken { get; set; }
        public string? IdToken { get; set; }
    }
}
