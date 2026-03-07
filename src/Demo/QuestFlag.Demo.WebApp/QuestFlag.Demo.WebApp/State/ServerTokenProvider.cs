using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http;
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
        if (context == null)
        {
            Console.WriteLine("[ServerTokenProvider] HttpContext is null.");
            return null;
        }

        // Preferred source: token saved in cookie auth ticket.
        var token = await context.GetTokenAsync(CookieAuthenticationDefaults.AuthenticationScheme, "access_token");

        // Fallbacks for cases where middleware stores token under different scheme/context.
        token ??= await context.GetTokenAsync(OpenIdConnectDefaults.AuthenticationScheme, "access_token");
        token ??= await context.GetTokenAsync("access_token");

        if (string.IsNullOrEmpty(token))
        {
            Console.WriteLine($"[ServerTokenProvider] No access token found. Authenticated={context.User?.Identity?.IsAuthenticated == true}");
        }

        return token;
    }

    public Task HandleUnauthorizedAsync()
    {
        // On server side, we don't handle redirects via the provider.
        return Task.CompletedTask;
    }
}
