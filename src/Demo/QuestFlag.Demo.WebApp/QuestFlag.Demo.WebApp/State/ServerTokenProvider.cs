using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using QuestFlag.Infrastructure.Client;
using QuestFlag.Infrastructure.Client.Contracts;
using System.Threading;
using System.Threading.Tasks;

namespace QuestFlag.Demo.WebApp.State;

public class ServerTokenProvider : IAccessTokenProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ServerTokenProvider(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<string?> GetAccessTokenAsync(CancellationToken cancellationToken = default)
    {
        var context = _httpContextAccessor.HttpContext;
        if (context == null) return null;

        return await context.GetTokenAsync("access_token");
    }

    public Task HandleUnauthorizedAsync()
    {
        // On server side, we don't handle redirects via the provider
        return Task.CompletedTask;
    }
}
