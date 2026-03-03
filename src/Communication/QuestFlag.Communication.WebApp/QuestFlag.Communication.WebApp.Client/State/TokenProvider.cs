using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Authorization;
using QuestFlag.Infrastructure.Client;
using QuestFlag.Infrastructure.Client.Contracts;
using Microsoft.AspNetCore.Components;

namespace QuestFlag.Communication.WebApp.Client.State;

public class TokenProvider : IAccessTokenProvider
{
    private readonly NavigationManager _navigation;
    private readonly PersistentComponentState _state;
    private string? _accessToken;

    public TokenProvider(NavigationManager navigation, PersistentComponentState state)
    {
        _navigation = navigation;
        _state = state;

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
        _navigation.NavigateTo("/Logout", forceLoad: true);
        return Task.CompletedTask;
    }
}
