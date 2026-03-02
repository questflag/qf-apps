using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Authorization;
using QuestFlag.Infrastructure.Client;
using Microsoft.AspNetCore.Components;

namespace QuestFlag.Passport.AdminWebApp.Client.State;

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
