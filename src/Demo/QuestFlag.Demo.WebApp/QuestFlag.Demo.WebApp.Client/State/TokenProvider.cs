using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Authorization;
using QuestFlag.Infrastructure.Client;
using QuestFlag.Infrastructure.Client.Contracts;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace QuestFlag.Demo.WebApp.Client.State;

public class TokenProvider : IAccessTokenProvider
{
    private readonly NavigationManager _navigation;
    private readonly PersistentComponentState _state;
    private readonly IJSRuntime _js;
    private string? _accessToken;
    private bool _initialized;

    public TokenProvider(NavigationManager navigation, PersistentComponentState state, IJSRuntime js)
    {
        _navigation = navigation;
        _state = state;
        _js = js;

        // Try to read token persisted by the server host during prerendering
        if (_state.TryTakeFromJson<string>("AccessToken", out var token))
        {
            _accessToken = token;
        }
    }

    public async Task<string?> GetAccessTokenAsync(CancellationToken cancellationToken = default)
    {
        if (_initialized) return _accessToken;

        try
        {
            if (string.IsNullOrEmpty(_accessToken))
            {
                // Try load from storage
                _accessToken = await _js.InvokeAsync<string?>("localStorage.getItem", cancellationToken, "access_token");
            }
            else
            {
                // Save what we got from server
                await _js.InvokeVoidAsync("localStorage.setItem", cancellationToken, "access_token", _accessToken);
            }
            _initialized = true;
        }
        catch { /* Prerendering or JS error */ }

        return _accessToken;
    }

    public async Task ClearTokenAsync()
    {
        _accessToken = null;
        try
        {
            await _js.InvokeVoidAsync("localStorage.removeItem", "access_token");
        }
        catch { /* Ignore */ }
    }

    public Task HandleUnauthorizedAsync()
    {
        // Token is expired or invalid. Redirect to logout.
        _navigation.NavigateTo("/Logout", forceLoad: true);
        return Task.CompletedTask;
    }
}
