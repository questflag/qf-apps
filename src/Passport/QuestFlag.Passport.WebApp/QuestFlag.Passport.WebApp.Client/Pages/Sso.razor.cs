using Microsoft.AspNetCore.Components;
using System.Collections.Generic;
using System;

namespace QuestFlag.Passport.WebApp.Client.Pages;

public partial class Sso
{
    [SupplyParameterFromQuery(Name = "client_id")] public string? ClientId { get; set; }
    [SupplyParameterFromQuery(Name = "redirect_uri")] public string? RedirectUri { get; set; }
    [SupplyParameterFromQuery(Name = "response_type")] public string? ResponseType { get; set; }
    [SupplyParameterFromQuery(Name = "scope")] public string? Scope { get; set; }
    [SupplyParameterFromQuery(Name = "response_mode")] public string? ResponseMode { get; set; }
    [SupplyParameterFromQuery(Name = "nonce")] public string? Nonce { get; set; }
    [SupplyParameterFromQuery(Name = "state")] public string? State { get; set; }
    [SupplyParameterFromQuery(Name = "code_challenge")] public string? CodeChallenge { get; set; }
    [SupplyParameterFromQuery(Name = "code_challenge_method")] public string? CodeChallengeMethod { get; set; }
    [SupplyParameterFromQuery(Name = "returnUrl")] public string? ReturnUrl { get; set; }
    [SupplyParameterFromQuery(Name = "ErrorMessage")] public string? ErrorMessage { get; set; }

    private string SubmitUrl
    {
        get
        {
            var baseUrl = $"{Configuration["ServiceUrls:PassportServices"]}/connect/authorize";
            var uri = NavigationManager.ToAbsoluteUri(NavigationManager.Uri);
            return string.IsNullOrEmpty(uri.Query) ? baseUrl : $"{baseUrl}{uri.Query}";
        }
    }

    private Dictionary<string, string> ExtraParameters = new();

    protected override void OnInitialized()
    {
        var uri = NavigationManager.ToAbsoluteUri(NavigationManager.Uri);
        var query = Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(uri.Query);

        var handledKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "client_id", "redirect_uri", "response_type", "scope", "response_mode",
            "nonce", "state", "code_challenge", "code_challenge_method", "returnUrl", "ErrorMessage"
        };

        foreach (var param in query)
        {
            if (handledKeys.Contains(param.Key)) continue;
            ExtraParameters[param.Key] = param.Value.ToString();
        }
    }
}
