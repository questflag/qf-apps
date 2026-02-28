using Microsoft.AspNetCore.Components;
using QuestFlag.Passport.UserClient;

namespace QuestFlag.Passport.WebApp.Client.Pages;

public partial class VerifyEmailPage
{
    [SupplyParameterFromQuery] public Guid UserId { get; set; }
    [SupplyParameterFromQuery] public string Token { get; set; } = "";

    private string _password = "";
    private string _confirm = "";
    private bool _isSubmitting;
    private bool _success;
    private string? _error;

    private async Task HandleVerify()
    {
        if (_password != _confirm) { _error = "Passwords do not match."; return; }
        _isSubmitting = true;
        _error = null;
        try
        {
            var ok = await PassportClient.VerifyEmailAsync(UserId, Token, _password);
            if (ok) _success = true;
            else _error = "This invite link is invalid or has expired. Contact your administrator.";
        }
        catch { _error = "An error occurred. Please try again."; }
        finally { _isSubmitting = false; }
    }
}
