using Microsoft.AspNetCore.Components;
using QuestFlag.Passport.UserClient;

namespace QuestFlag.Passport.WebApp.Client.Pages;

public partial class ResetPasswordPage
{
    [SupplyParameterFromQuery] public Guid UserId { get; set; }
    [SupplyParameterFromQuery] public string Token { get; set; } = "";

    private string _password = "";
    private string _confirm = "";
    private bool _isSubmitting;
    private bool _success;
    private string? _error;

    private async Task HandleReset()
    {
        if (_password != _confirm) { _error = "Passwords do not match."; return; }
        _isSubmitting = true;
        _error = null;
        try
        {
            var ok = await PassportClient.ResetPasswordAsync(UserId, Token, _password);
            if (ok) _success = true;
            else _error = "Invalid or expired reset link. Please request a new one.";
        }
        catch { _error = "An error occurred. Please try again."; }
        finally { _isSubmitting = false; }
    }
}
