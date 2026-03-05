using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using QuestFlag.Demo.WebApp.Client.State;
using Microsoft.AspNetCore.Components.Authorization;
using System.Threading.Tasks;

namespace QuestFlag.Demo.WebApp.Client.Pages;

public partial class Logout
{
    [Inject] private IJSRuntime JS { get; set; } = default!;
    [Inject] private NavigationManager Nav { get; set; } = default!;
    [Inject] private AuthenticationStateProvider AuthStateProvider { get; set; } = default!;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender) return;
        await PerformLogout();
    }

    private async Task PerformLogout()
    {
        try
        {
            // Clear all localStorage keys
            await JS.InvokeVoidAsync("localStorage.clear");

            // Clear all session storage too
            await JS.InvokeVoidAsync("sessionStorage.clear");

            // Clear all cookies via JavaScript
            await JS.InvokeVoidAsync("eval", @"
                document.cookie.split(';').forEach(function(c) {
                    document.cookie = c.trim().split('=')[0] + '=;expires=Thu, 01 Jan 1970 00:00:00 UTC;path=/;';
                });
            ");
        }
        catch { /* Ignore JS errors */ }

        // Notify auth state provider (in-memory singleton)
        if (AuthStateProvider is PersistentAuthenticationStateProvider provider)
        {
            await provider.SignOutAsync();
        }

        // Navigate to login — client-side nav, auth state is already cleared
        Nav.NavigateTo("/login", forceLoad: false);
    }
}
