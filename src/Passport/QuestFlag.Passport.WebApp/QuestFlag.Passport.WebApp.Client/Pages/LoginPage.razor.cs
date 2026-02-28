using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Configuration;
using Microsoft.JSInterop;
using QuestFlag.Passport.UserClient;

namespace QuestFlag.Passport.WebApp.Client.Pages;

public partial class LoginPage
{
    [SupplyParameterFromQuery] public string? ReturnUrl { get; set; }
    [SupplyParameterFromQuery] public string? ClientId { get; set; }

    [Inject] private IConfiguration Config { get; set; } = default!;

    private IReadOnlyList<TenantDto>? _tenants;
    private ResolvedTenantDto? _resolvedTenant;
    private bool _loadingTenants = true;
    private string _tenantSlug = "";
    private string _username = "";
    private string _password = "";
    private string? _error;
    private bool _isSubmitting;

    protected override async Task OnInitializedAsync()
    {
        // 1. Try to resolve tenant from the Host header (server-side, via cascading parameter or JS interop)
        try
        {
            var host = await JS.InvokeAsync<string>("eval", "window.location.host");
            _resolvedTenant = await PassportClient.ResolveTenantByDomainAsync(host);
            if (_resolvedTenant != null)
                _tenantSlug = _resolvedTenant.Slug;
        }
        catch { /* ignore — fallback to dropdown */ }

        // 2. Load all tenants for dropdown (if no auto-resolved tenant)
        if (_resolvedTenant == null)
        {
            try
            {
                _tenants = await PassportClient.GetTenantsAsync();
                if (_tenants.Count == 1)
                {
                    _tenantSlug = _tenants[0].Slug;
                }
            }
            catch { _error = "Could not load organizations. The auth service may be offline."; }
        }

        _loadingTenants = false;
    }

    private async Task HandleLogin()
    {
        if (string.IsNullOrWhiteSpace(_tenantSlug)) { _error = "Please select an organization."; return; }

        _error = null;
        _isSubmitting = true;

        try
        {
            // Build the authorize URL — redirect to Passport.Services /connect/authorize
            // URLs are configured via Passport:PassportServicesBaseUrl and Passport:InfraWebAppBaseUrl in appsettings.json
            var passportServicesBaseUrl = Config["Passport:PassportServicesBaseUrl"]
                ?? throw new InvalidOperationException("Passport:PassportServicesBaseUrl is required in configuration.");
            var infraWebAppBaseUrl = Config["Passport:InfraWebAppBaseUrl"]
                ?? throw new InvalidOperationException("Passport:InfraWebAppBaseUrl is required in configuration.");

            var authorizeUrl = $"{passportServicesBaseUrl}/connect/authorize" +
                               $"?response_type=code" +
                               $"&client_id={Uri.EscapeDataString(ClientId ?? "passport-webapp")}" +
                               $"&scope=openid profile roles offline_access" +
                               $"&redirect_uri={Uri.EscapeDataString(ReturnUrl ?? $"{infraWebAppBaseUrl}/signin-oidc")}" +
                               $"&tenant={Uri.EscapeDataString(_tenantSlug)}" +
                               $"&login_hint={Uri.EscapeDataString(_username)}";

            // Post credentials to Passport.Services via form, then redirect
            // The actual credential validation happens in AuthController which reads the form post
            await JS.InvokeVoidAsync("history.replaceState", null, "", Nav.Uri);
            Nav.NavigateTo(authorizeUrl, forceLoad: true);
        }
        catch (Exception ex)
        {
            _error = "Sign-in failed. Please check your credentials.";
            Console.WriteLine(ex);
        }
        finally
        {
            _isSubmitting = false;
        }
    }
}
