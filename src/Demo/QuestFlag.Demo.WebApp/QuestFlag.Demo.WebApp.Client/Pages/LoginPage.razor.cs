using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using QuestFlag.Demo.WebApp.Client.Helpers;

namespace QuestFlag.Demo.WebApp.Client.Pages;

public partial class LoginPage
{
    [Parameter]
    [SupplyParameterFromQuery]
    public string? ErrorMessage { get; set; }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender && string.IsNullOrEmpty(ErrorMessage))
        {
            await RedirectToSso();
        }
    }

    private async Task RedirectToSso()
    {
        var clientId = Configuration["Oidc:ClientId"] ?? "infra-webapp";
        var redirectUri = Nav.ToAbsoluteUri("signin-oidc").ToString();
        
        var ssoBaseUrl = Configuration["ServiceUrls:PassportWebApp"] ?? "https://localhost:7002";
        var ssoUrl = $"{ssoBaseUrl.TrimEnd('/')}/sso";

        // Clear any stale auth state before starting a fresh login
        try
        {
            await JS.InvokeVoidAsync("localStorage.removeItem", "user_info");
            await JS.InvokeVoidAsync("localStorage.removeItem", "access_token");
            await JS.InvokeVoidAsync("localStorage.removeItem", "code_verifier");
        }
        catch { /* Ignore */ }

        // PKCE
        var codeVerifier = PkceHelper.GenerateCodeVerifier();
        var codeChallenge = PkceHelper.GenerateCodeChallenge(codeVerifier);
        await JS.InvokeVoidAsync("localStorage.setItem", "code_verifier", codeVerifier);
        
        var queryParams = new Dictionary<string, string?>
        {
            ["client_id"] = clientId,
            ["redirect_uri"] = redirectUri,
            ["response_type"] = "code",
            ["scope"] = "openid profile email roles offline_access",
            ["response_mode"] = "query",
            ["state"] = Guid.NewGuid().ToString("N"),
            ["nonce"] = Guid.NewGuid().ToString("N"),
            ["code_challenge"] = codeChallenge,
            ["code_challenge_method"] = "S256"
        };

        var uri = Microsoft.AspNetCore.WebUtilities.QueryHelpers.AddQueryString(ssoUrl, queryParams);
        Nav.NavigateTo(uri, forceLoad: true);
    }
}
