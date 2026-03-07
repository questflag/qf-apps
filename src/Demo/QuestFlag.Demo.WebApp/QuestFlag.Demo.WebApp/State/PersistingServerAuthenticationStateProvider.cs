using System.Diagnostics;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;
using Microsoft.AspNetCore.Components.Web;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using QuestFlag.Demo.WebApp.Client.State;

namespace QuestFlag.Demo.WebApp.State;

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

        if (httpContextAccessor.HttpContext?.User is { Identity.IsAuthenticated: true } user)
        {
            SetAuthenticationState(Task.FromResult(new AuthenticationState(user)));
        }
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
            var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                ?? principal.FindFirst("sub")?.Value
                ?? principal.FindFirst("oid")?.Value;

            var name = principal.Identity.Name ?? principal.FindFirst("name")?.Value ?? userId;
            var email = principal.FindFirst(ClaimTypes.Email)?.Value;

            if (userId != null)
            {
                Console.WriteLine($"[ServerAuth] Persisting UserInfo for {name} ({userId})");
                state.PersistAsJson(nameof(UserInfo), new UserInfo
                {
                    UserId = userId,
                    Name = name,
                    Email = email,
                    Roles = principal.FindAll(ClaimTypes.Role)
                        .Concat(principal.FindAll("role"))
                        .Select(c => c.Value)
                        .Distinct()
                        .ToArray()
                });

                // Also persist the access token if available
                var context = httpContextAccessor.HttpContext;
                if (context != null)
                {
                    // Explicitly pull from the cookie scheme where OIDC stores it after SaveTokens = true
                    var token = await context.GetTokenAsync(CookieAuthenticationDefaults.AuthenticationScheme, "access_token");
                    if (!string.IsNullOrEmpty(token))
                    {
                        Console.WriteLine($"[ServerAuth] Persisting AccessToken (length: {token.Length})");
                        state.PersistAsJson("AccessToken", token);
                    }
                    else
                    {
                        Console.WriteLine("[ServerAuth] WARNING: AccessToken NOT FOUND in HttpContext tokens.");
                    }
                }
            }
            else
            {
                Console.WriteLine("[ServerAuth] WARNING: Authenticated user has no Subject/NameIdentifier claim.");
            }
        }
    }

    public void Dispose()
    {
        subscription.Dispose();
        AuthenticationStateChanged -= OnAuthenticationStateChanged;
    }
}
