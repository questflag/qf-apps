using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;

namespace QuestFlag.Demo.WebApp.Client.State;

internal class PersistentAuthenticationStateProvider : AuthenticationStateProvider
{
    private static readonly Task<AuthenticationState> defaultUnauthenticatedTask =
        Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity())));

    private Task<AuthenticationState> _authenticationStateTask = defaultUnauthenticatedTask;
    private readonly IJSRuntime _js;
    private readonly PersistentComponentState _state;
    private bool _initialized;

    public PersistentAuthenticationStateProvider(PersistentComponentState state, IJSRuntime js)
    {
        _state = state;
        _js = js;

        // Try server-persisted state first (prerender).
        if (_state.TryTakeFromJson<UserInfo>(nameof(UserInfo), out var userInfo) && userInfo is not null)
        {
            Console.WriteLine($"[AuthState] Recovered UserInfo from server: {userInfo.Name} ({userInfo.UserId})");
            _authenticationStateTask = Task.FromResult(CreateState(userInfo));
            _initialized = true;
        }
        else
        {
            Console.WriteLine("[AuthState] NO UserInfo found in PersistentComponentState.");
        }
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        if (!_initialized)
        {
            try
            {
                var userInfoJson = await _js.InvokeAsync<string?>("localStorage.getItem", "user_info");
                if (!string.IsNullOrEmpty(userInfoJson))
                {
                    var userInfo = JsonSerializer.Deserialize<UserInfo>(userInfoJson);
                    if (userInfo != null)
                    {
                        _authenticationStateTask = Task.FromResult(CreateState(userInfo));
                    }
                }
                else
                {
                    // Fallback: rebuild user info from JWT payload when user_info was not yet stored.
                    var accessToken = await _js.InvokeAsync<string?>("localStorage.getItem", "access_token");
                    if (!string.IsNullOrWhiteSpace(accessToken) && TryCreateUserInfoFromJwt(accessToken, out var tokenUserInfo))
                    {
                        Console.WriteLine($"[AuthState] Reconstructed UserInfo from access token for {tokenUserInfo.Name} ({tokenUserInfo.UserId}).");
                        _authenticationStateTask = Task.FromResult(CreateState(tokenUserInfo));
                        await _js.InvokeVoidAsync("localStorage.setItem", "user_info", JsonSerializer.Serialize(tokenUserInfo));
                    }
                }
            }
            catch
            {
                // JS not available during prerendering.
            }

            _initialized = true;
        }

        return await _authenticationStateTask;
    }

    public async Task SignInAsync(UserInfo userInfo, string accessToken)
    {
        try
        {
            await _js.InvokeVoidAsync("localStorage.setItem", "user_info", JsonSerializer.Serialize(userInfo));
            await _js.InvokeVoidAsync("localStorage.setItem", "access_token", accessToken);
        }
        catch
        {
            // Ignore storage errors.
        }

        _authenticationStateTask = Task.FromResult(CreateState(userInfo));
        _initialized = true;
        NotifyAuthenticationStateChanged(_authenticationStateTask);
    }

    public async Task SignOutAsync()
    {
        try
        {
            await _js.InvokeVoidAsync("localStorage.removeItem", "user_info");
            await _js.InvokeVoidAsync("localStorage.removeItem", "access_token");
            await _js.InvokeVoidAsync("localStorage.removeItem", "code_verifier");
        }
        catch
        {
            // Ignore storage errors.
        }

        _authenticationStateTask = defaultUnauthenticatedTask;
        _initialized = true;
        NotifyAuthenticationStateChanged(_authenticationStateTask);
    }

    private static AuthenticationState CreateState(UserInfo userInfo)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userInfo.UserId ?? string.Empty),
            new(ClaimTypes.Name, userInfo.Name ?? string.Empty)
        };

        if (!string.IsNullOrEmpty(userInfo.Email))
        {
            claims.Add(new Claim(ClaimTypes.Email, userInfo.Email));
        }

        foreach (var role in userInfo.Roles ?? Array.Empty<string>())
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity(claims, authenticationType: "Bearer")));
    }

    private static bool TryCreateUserInfoFromJwt(string token, out UserInfo userInfo)
    {
        userInfo = new UserInfo();

        try
        {
            var parts = token.Split('.');
            if (parts.Length < 2)
            {
                return false;
            }

            var payload = parts[1]
                .Replace('-', '+')
                .Replace('_', '/');

            payload = payload.PadRight(payload.Length + (4 - payload.Length % 4) % 4, '=');
            var payloadBytes = Convert.FromBase64String(payload);
            using var doc = JsonDocument.Parse(Encoding.UTF8.GetString(payloadBytes));
            var root = doc.RootElement;

            string? GetString(params string[] names)
            {
                foreach (var name in names)
                {
                    if (root.TryGetProperty(name, out var prop) && prop.ValueKind == JsonValueKind.String)
                    {
                        return prop.GetString();
                    }
                }
                return null;
            }

            var roles = new List<string>();
            void AddRoles(string claimName)
            {
                if (!root.TryGetProperty(claimName, out var prop)) return;

                if (prop.ValueKind == JsonValueKind.String)
                {
                    var role = prop.GetString();
                    if (!string.IsNullOrWhiteSpace(role)) roles.Add(role);
                }
                else if (prop.ValueKind == JsonValueKind.Array)
                {
                    foreach (var item in prop.EnumerateArray())
                    {
                        if (item.ValueKind == JsonValueKind.String)
                        {
                            var role = item.GetString();
                            if (!string.IsNullOrWhiteSpace(role)) roles.Add(role);
                        }
                    }
                }
            }

            AddRoles("role");
            AddRoles("http://schemas.microsoft.com/ws/2008/06/identity/claims/role");

            var userId = GetString("user_id", "sub");
            var name = GetString("name", "username", "preferred_username", "sub");
            var email = GetString("email");

            if (string.IsNullOrWhiteSpace(userId) && string.IsNullOrWhiteSpace(name))
            {
                return false;
            }

            userInfo = new UserInfo
            {
                UserId = userId,
                Name = name,
                Email = email,
                Roles = roles.Distinct(StringComparer.OrdinalIgnoreCase).ToArray()
            };

            return true;
        }
        catch
        {
            return false;
        }
    }
}
