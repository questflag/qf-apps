using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Configuration;
using Microsoft.JSInterop;
using QuestFlag.Passport.UserClient;

namespace QuestFlag.Demo.WebApp.Client.Pages;

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
    private string _authorizeUrl = "";
    private Dictionary<string, string> _formParams = new();

    protected override async Task OnInitializedAsync()
    {
        // 1. Try to resolve tenant from the Host header
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
        UpdateAuthorizeUrl();
    }

    private void OnInputChange() => UpdateAuthorizeUrl();

    private void UpdateAuthorizeUrl()
    {
        try
        {
            var passportServicesBaseUrl = Config["Passport:PassportServicesBaseUrl"] 
                ?? Config["ServiceUrls:PassportServices"] 
                ?? "https://localhost:7004";
            var infraWebAppBaseUrl = Config["Passport:InfraWebAppBaseUrl"] 
                ?? Config["ServiceUrls:InfraWebApp"] 
                ?? "https://localhost:7000";

            _formParams = new();
            var uri = new Uri(Nav.Uri);
            var query = uri.Query.TrimStart('?');
            if (!string.IsNullOrEmpty(query))
            {
                var pairs = query.Split('&');
                foreach (var pair in pairs)
                {
                    var parts = pair.Split('=', 2);
                    if (parts.Length > 0 && !string.IsNullOrEmpty(parts[0]))
                    {
                        var key = Uri.UnescapeDataString(parts[0]);
                        var value = parts.Length > 1 ? Uri.UnescapeDataString(parts[1]) : "";
                        _formParams[key] = value;
                    }
                }
            }

            if (!_formParams.ContainsKey("response_type")) _formParams["response_type"] = "code";
            if (!_formParams.ContainsKey("client_id")) _formParams["client_id"] = ClientId ?? "infra-webapp";
            if (!_formParams.ContainsKey("scope")) _formParams["scope"] = "openid profile roles offline_access";
            if (!_formParams.ContainsKey("redirect_uri")) _formParams["redirect_uri"] = ReturnUrl ?? $"{infraWebAppBaseUrl}/signin-oidc";

            var uriBuilder = new UriBuilder($"{passportServicesBaseUrl}/connect/authorize");
            var queryList = new List<string>();
            foreach (var kvp in _formParams)
            {
                queryList.Add($"{Uri.EscapeDataString(kvp.Key)}={Uri.EscapeDataString(kvp.Value)}");
            }
            uriBuilder.Query = string.Join("&", queryList);

            var tenantSlugExt = _tenantSlug;
            queryList.Add($"tenant={Uri.EscapeDataString(tenantSlugExt)}");
            var usernameExt = _username;
            queryList.Add($"login_hint={Uri.EscapeDataString(usernameExt)}");
            uriBuilder.Query = string.Join("&", queryList);
            
            _authorizeUrl = uriBuilder.ToString();
        }
        catch { /* ignore */ }
    }

}
