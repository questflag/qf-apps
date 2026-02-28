using System.Diagnostics;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;
using Microsoft.AspNetCore.Components.Web;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using QuestFlag.Infrastructure.WebApp.Client.State;

namespace QuestFlag.Infrastructure.WebApp.State;

internal sealed class PersistingServerAuthenticationStateProvider : ServerAuthenticationStateProvider, IDisposable
{
    private readonly PersistentComponentState state;
    private readonly PersistingComponentStateSubscription subscription;
    private readonly IHttpContextAccessor httpContextAccessor;
    private Task<AuthenticationState>? authenticationStateTask;

    public PersistingServerAuthenticationStateProvider(
        PersistentComponentState persistentComponentState,
        IHttpContextAccessor httpContextAccessor)
    {
        state = persistentComponentState;
        this.httpContextAccessor = httpContextAccessor;
        AuthenticationStateChanged += OnAuthenticationStateChanged;
        subscription = state.RegisterOnPersisting(OnPersistingAsync, RenderMode.InteractiveWebAssembly);
    }

    private void OnAuthenticationStateChanged(Task<AuthenticationState> task)
    {
        authenticationStateTask = task;
    }

    private async Task OnPersistingAsync()
    {
        if (authenticationStateTask is null)
        {
            throw new UnreachableException($"Authentication state not set in {nameof(OnPersistingAsync)}().");
        }

        var authenticationState = await authenticationStateTask;
        var principal = authenticationState.User;

        if (principal.Identity?.IsAuthenticated == true)
        {
            var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? principal.FindFirst("sub")?.Value;
            var name = principal.Identity.Name ?? principal.FindFirst("name")?.Value ?? userId;
            var email = principal.FindFirst(ClaimTypes.Email)?.Value;

            if (userId != null)
            {
                state.PersistAsJson(nameof(UserInfo), new UserInfo
                {
                    UserId = userId,
                    Name = name,
                    Email = email
                });

                // Also persist the access token if available
                var context = httpContextAccessor.HttpContext;
                if (context != null)
                {
                    var token = await context.GetTokenAsync("access_token");
                    if (!string.IsNullOrEmpty(token))
                    {
                        state.PersistAsJson("AccessToken", token);
                    }
                }
            }
        }
    }

    public void Dispose()
    {
        subscription.Dispose();
        AuthenticationStateChanged -= OnAuthenticationStateChanged;
    }
}
