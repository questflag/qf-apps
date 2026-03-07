using System.Threading;
using System.Threading.Tasks;
using QuestFlag.Infrastructure.Client.Contracts;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace QuestFlag.Demo.WebApp.Client.State;

public class TokenProvider : IAccessTokenProvider
{
    private const string UnauthorizedRetryKey = "auth_retry_401";

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

        // Try to read token persisted by the server host during prerendering.
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
                Console.WriteLine($"[TokenProvider] Saving server-provided access token to localStorage (length: {_accessToken.Length})");
                await _js.InvokeVoidAsync("localStorage.setItem", cancellationToken, "access_token", _accessToken);
            }

            if (!string.IsNullOrEmpty(_accessToken))
            {
                await _js.InvokeVoidAsync("sessionStorage.removeItem", cancellationToken, UnauthorizedRetryKey);
            }

            _initialized = true;
        }
        catch (Exception ex)
        {
            // Prerendering or JS error.
            Console.WriteLine($"[TokenProvider] JS interop error or prerendering: {ex.Message}");
        }

        return _accessToken;
    }

    public async Task ClearTokenAsync()
    {
        _accessToken = null;
        _initialized = false;

        try
        {
            await _js.InvokeVoidAsync("localStorage.removeItem", "access_token");
        }
        catch
        {
            // Ignore storage errors.
        }
    }

    public async Task HandleUnauthorizedAsync()
    {
        Console.WriteLine($"[TokenProvider] Unauthorized response received for {_navigation.Uri}");

        try
        {
            var retryFlag = await _js.InvokeAsync<string?>("sessionStorage.getItem", UnauthorizedRetryKey);
            if (retryFlag != "1")
            {
                Console.WriteLine("[TokenProvider] First 401 in this browser session. Clearing token and hard-reloading once.");
                await _js.InvokeVoidAsync("sessionStorage.setItem", UnauthorizedRetryKey, "1");
                await ClearTokenAsync();

                var uri = new Uri(_navigation.Uri);
                var returnPath = string.IsNullOrEmpty(uri.PathAndQuery) ? "/" : uri.PathAndQuery;
                _navigation.NavigateTo(returnPath, forceLoad: true);
                return;
            }

            await _js.InvokeVoidAsync("sessionStorage.removeItem", UnauthorizedRetryKey);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[TokenProvider] Unable to evaluate 401 retry flow: {ex.Message}");
        }

        Console.WriteLine("[TokenProvider] Repeated 401 detected. Navigating to logout.");
        _navigation.NavigateTo("/logout", forceLoad: true);
    }
}
