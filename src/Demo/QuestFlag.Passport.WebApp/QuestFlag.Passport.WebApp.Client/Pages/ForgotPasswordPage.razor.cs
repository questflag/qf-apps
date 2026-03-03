using Microsoft.AspNetCore.Components;
using QuestFlag.Passport.UserClient;

namespace QuestFlag.Passport.WebApp.Client.Pages;

public partial class ForgotPasswordPage
{
    private string _email = "";
    private bool _isSubmitting;
    private bool _sent;
    private string? _error;

    private async Task HandleSubmit()
    {
        _isSubmitting = true;
        _error = null;
        try
        {
            await PassportClient.ForgotPasswordAsync(_email);
            _sent = true;
        }
        catch { _error = "An error occurred. Please try again."; }
        finally { _isSubmitting = false; }
    }
}
