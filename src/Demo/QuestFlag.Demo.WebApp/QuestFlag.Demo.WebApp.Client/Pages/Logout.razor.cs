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
        // Clear in-memory auth state and localStorage keys (user_info, access_token, code_verifier)
        if (AuthStateProvider is PersistentAuthenticationStateProvider provider)
        {
            await provider.SignOutAsync();
        }

        // Navigate to the server-side logout endpoint with forceLoad:true so the server request is made.
        // The server calls SignOutAsync which expires the HttpOnly auth cookie — JS cannot delete it.
        // The server endpoint then redirects to / (the demo app landing page).
        Nav.NavigateTo("/api/auth/logout", forceLoad: true);
    }
}
