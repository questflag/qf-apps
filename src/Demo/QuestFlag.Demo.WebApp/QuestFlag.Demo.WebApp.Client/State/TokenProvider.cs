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
                if (!string.IsNullOrEmpty(_accessToken))
                {
                    Console.WriteLine($"[TokenProvider] Access token recovered from localStorage (length: {_accessToken.Length})");
                }
                else
                {
                    Console.WriteLine("[TokenProvider] NO access token found in localStorage.");
                }
            }
            else
            {
                // Save what we got from server
                Console.WriteLine($"[TokenProvider] Saving server-provided access token to localStorage (length: {_accessToken.Length})");
                await _js.InvokeVoidAsync("localStorage.setItem", cancellationToken, "access_token", _accessToken);
            }
            _initialized = true;
        }
        catch (Exception ex)
        { 
            /* Prerendering or JS error */ 
            Console.WriteLine($"[TokenProvider] JS interop error or prerendering: {ex.Message}");
        }

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
        _navigation.NavigateTo("/Logout");
        return Task.CompletedTask;
    }
}
