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

        if (_state.TryTakeFromJson<UserInfo>(nameof(UserInfo), out var userInfo) && userInfo is not null)
        {
            _authenticationStateTask = Task.FromResult(CreateState(userInfo));
        }
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        if (!_initialized && _authenticationStateTask == defaultUnauthenticatedTask)
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
            catch { /* Ignore JS errors during prerendering */ }
            _initialized = true;
        }

        // If we just got userInfo from server, persist it
        if (!_initialized && _authenticationStateTask != defaultUnauthenticatedTask)
        {
            try
            {
                var info = await GetUserInfo();
                if (info != null)
                {
                    await _js.InvokeVoidAsync("localStorage.setItem", "user_info", JsonSerializer.Serialize(info));
                }
                _initialized = true;
            }
            catch { /* Ignore */ }
        }

        return await _authenticationStateTask;
    }

    private async Task<UserInfo?> GetUserInfo()
    {
        var state = await _authenticationStateTask;
        if (!state.User.Identity?.IsAuthenticated == true) return null;

        return new UserInfo
        {
            UserId = state.User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
            Name = state.User.Identity.Name,
            Email = state.User.FindFirst(ClaimTypes.Email)?.Value,
            Roles = state.User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToArray()
        };
    }

    private AuthenticationState CreateState(UserInfo userInfo)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userInfo.UserId ?? ""),
            new Claim(ClaimTypes.Name, userInfo.Name ?? ""),
        };
        if (!string.IsNullOrEmpty(userInfo.Email)) claims.Add(new Claim(ClaimTypes.Email, userInfo.Email));

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
