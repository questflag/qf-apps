using QuestFlag.Passport.UserClient;

namespace QuestFlag.Demo.WebApp.Client.Pages;

public partial class ProfilePage
{
    private UserProfileDto? _userInfo;

    protected override async Task OnInitializedAsync()
    {
        _userInfo = await PassportClient.GetUserInfoAsync();
    }
}
