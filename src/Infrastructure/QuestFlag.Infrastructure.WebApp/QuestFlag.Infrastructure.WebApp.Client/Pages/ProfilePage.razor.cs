using QuestFlag.Passport.UserClient;

namespace QuestFlag.Infrastructure.WebApp.Client.Pages;

public partial class ProfilePage
{
    private UserProfileDto? _userInfo;

    protected override async Task OnInitializedAsync()
    {
        _userInfo = await PassportClient.GetUserInfoAsync();
    }
}
