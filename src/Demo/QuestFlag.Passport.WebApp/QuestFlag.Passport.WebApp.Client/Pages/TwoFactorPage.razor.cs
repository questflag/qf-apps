using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using QuestFlag.Passport.UserClient;

namespace QuestFlag.Passport.WebApp.Client.Pages;

public partial class TwoFactorPage
{
    [SupplyParameterFromQuery] public Guid UserId { get; set; }
    [SupplyParameterFromQuery] public string ReturnUrl { get; set; } = "/";

    private string _otp = "";
    private bool _rememberDevice;
    private bool _isSubmitting;
    private string? _error;

    private async Task HandleVerify()
    {
        _isSubmitting = true;
        _error = null;
        try
        {
            var valid = await PassportClient.VerifyLoginOtpAsync(UserId, _otp);
            if (!valid)
            {
                _error = "Invalid or expired code. Please try again.";
                return;
            }

            if (_rememberDevice)
            {
                // Signal to the server-side OIDC handler to set a trust-device cookie
                await JS.InvokeVoidAsync("sessionStorage.setItem", "trustDevice", "1");
            }

            Nav.NavigateTo(ReturnUrl, forceLoad: true);
        }
        catch { _error = "An error occurred. Please try again."; }
        finally { _isSubmitting = false; }
    }

    private async Task ResendOtp()
    {
        await PassportClient.SendLoginOtpAsync(UserId);
    }
}
