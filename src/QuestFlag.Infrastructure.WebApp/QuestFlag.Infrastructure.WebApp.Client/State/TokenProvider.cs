using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Authorization;
using QuestFlag.Infrastructure.Client;
using Microsoft.AspNetCore.Components;

namespace QuestFlag.Infrastructure.WebApp.Client.State;

/// <summary>
/// Provides access tokens to the AuthenticatedHttpHandler.
/// Since we are using cookie-based auth in this updated architecture, 
/// the browser automatically sends the HttpOnly cookie with API requests
/// IF the API is on the same domain.
/// 
/// However, if calling an external API that requires a Bearer token, we would extract 
/// it here. For our architecture, since `Infrastructure.Services` runs on a different port (7001),
/// we either need to use CORS with credentials (cookies) OR have the Blazor host pass the access_token 
/// to the WASM client.
/// 
/// In our new architecture, the WASM client receives the token via the exact same `PersistentComponentState`
/// mechanism used in PersistentAuthenticationStateProvider.
/// </summary>
public class TokenProvider : IAccessTokenProvider
{
    private readonly NavigationManager _navigation;
    private readonly PersistentComponentState _state;
    private string? _accessToken;

    public TokenProvider(NavigationManager navigation, PersistentComponentState state)
    {
        _navigation = navigation;
        _state = state;

        // Try to read token persisted by the server host during prerendering
        if (_state.TryTakeFromJson<string>("AccessToken", out var token))
        {
            _accessToken = token;
        }
    }

    public Task<string?> GetAccessTokenAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_accessToken);
    }

    public Task HandleUnauthorizedAsync()
    {
        // Token is expired or invalid. Redirect to login.
        _navigation.NavigateTo("/login", forceLoad: true);
        return Task.CompletedTask;
    }
}
