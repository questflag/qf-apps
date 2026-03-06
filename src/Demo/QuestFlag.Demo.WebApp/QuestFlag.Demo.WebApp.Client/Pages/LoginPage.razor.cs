using Microsoft.AspNetCore.Components;

namespace QuestFlag.Demo.WebApp.Client.Pages;

public partial class LoginPage
{
    [Parameter]
    [SupplyParameterFromQuery]
    public string? ErrorMessage { get; set; }

    [Inject] private NavigationManager Nav { get; set; } = default!;

    protected override void OnAfterRender(bool firstRender)
    {
        if (firstRender && string.IsNullOrEmpty(ErrorMessage))
        {
            // Navigate to the server-side OIDC challenge endpoint.
            // The server handles PKCE via correlation cookies — no localStorage needed.
            Nav.NavigateTo("/api/auth/login", forceLoad: true);
        }
    }

    // Called by the "Retry Sign In" button on the error screen
    private void RedirectToLogin() => Nav.NavigateTo("/api/auth/login", forceLoad: true);
}
