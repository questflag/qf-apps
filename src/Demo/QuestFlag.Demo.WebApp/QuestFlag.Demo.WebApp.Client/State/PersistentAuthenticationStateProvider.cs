using System.Security.Claims;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using System.Text.Json;
using System.Linq;

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

        // Try server-persisted state first (prerender)
        if (_state.TryTakeFromJson<UserInfo>(nameof(UserInfo), out var userInfo) && userInfo is not null)
        {
            _authenticationStateTask = Task.FromResult(CreateState(userInfo));
            _initialized = true;
        }
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        // On first client-side call (after WASM init), read from localStorage
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
            }
            catch { /* JS not available during prerendering */ }
            _initialized = true;
        }

        return await _authenticationStateTask;
    }

    /// <summary>
    /// Called after successful SSO token exchange. Stores user info and notifies Blazor.
    /// Does NOT require a page reload.
    /// </summary>
    public async Task SignInAsync(UserInfo userInfo, string accessToken)
    {
        try
        {
            await _js.InvokeVoidAsync("localStorage.setItem", "user_info", JsonSerializer.Serialize(userInfo));
            await _js.InvokeVoidAsync("localStorage.setItem", "access_token", accessToken);
        }
        catch { /* Ignore */ }

        _authenticationStateTask = Task.FromResult(CreateState(userInfo));
        _initialized = true;
        NotifyAuthenticationStateChanged(_authenticationStateTask);
    }

    /// <summary>
    /// Called on logout. Clears all auth state from memory and localStorage.
    /// Does NOT require a page reload.
    /// </summary>
    public async Task SignOutAsync()
    {
        try
        {
            // Clear all known auth keys
            await _js.InvokeVoidAsync("localStorage.removeItem", "user_info");
            await _js.InvokeVoidAsync("localStorage.removeItem", "access_token");
            await _js.InvokeVoidAsync("localStorage.removeItem", "code_verifier");
        }
        catch { /* Ignore */ }

        _authenticationStateTask = defaultUnauthenticatedTask;
        _initialized = true;
        NotifyAuthenticationStateChanged(_authenticationStateTask);
    }

    private AuthenticationState CreateState(UserInfo userInfo)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userInfo.UserId ?? ""),
            new Claim(ClaimTypes.Name, userInfo.Name ?? ""),
        };
        if (!string.IsNullOrEmpty(userInfo.Email))
            claims.Add(new Claim(ClaimTypes.Email, userInfo.Email));

        if (userInfo.Roles != null)
        {
            foreach (var role in userInfo.Roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }
        }

        return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity(claims, authenticationType: "Bearer")));
    }
}
