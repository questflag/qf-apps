using Microsoft.AspNetCore.Components;
using QuestFlag.Passport.UserClient;

namespace QuestFlag.Infrastructure.WebApp.Client.Pages;

public partial class TwoFactorSetupPage
{
    private UserProfileDto? _userInfo;
    private string _phoneNumber = "";
    private string _otp = "";
    private bool _isVerifying = false;
    private bool _isLoading = false;
    private string? _errorMessage;
    private bool _isSetupComplete = false;

    protected override async Task OnInitializedAsync()
    {
        await LoadUserInfo();
    }

    private async Task LoadUserInfo()
    {
        _userInfo = await PassportClient.GetUserInfoAsync();
    }

    private async Task StartSetup()
    {
        if (string.IsNullOrWhiteSpace(_phoneNumber)) return;
        _isLoading = true;
        _errorMessage = null;
        try
        {
            await PassportClient.SetupPhoneTwoFactorAsync(new SetupPhoneRequest(_phoneNumber));
            _isVerifying = true;
        }
        catch (Exception ex)
        {
            _errorMessage = "Failed to send code: " + ex.Message;
        }
        finally
        {
            _isLoading = false;
        }
    }

    private async Task VerifyOtp()
    {
        if (string.IsNullOrWhiteSpace(_otp)) return;
        _isLoading = true;
        _errorMessage = null;
        try
        {
            await PassportClient.VerifyPhoneTwoFactorAsync(new VerifyPhoneRequest(_otp));
            await LoadUserInfo();
            _isVerifying = false;
            _isSetupComplete = true;
            _otp = "";
        }
        catch (Exception ex)
        {
            _errorMessage = "Invalid code: " + ex.Message;
        }
        finally
        {
            _isLoading = false;
        }
    }

    private void CancelSetup()
    {
        _isVerifying = false;
        _otp = "";
        _errorMessage = null;
    }

    private async Task Disable2FA()
    {
        _isLoading = true;
        _errorMessage = null;
        try
        {
            await PassportClient.DisableTwoFactorAsync();
            await LoadUserInfo();
            _isSetupComplete = false;
        }
        catch (Exception ex)
        {
            _errorMessage = "Failed to disable 2FA: " + ex.Message;
        }
        finally
        {
            _isLoading = false;
        }
    }
}
