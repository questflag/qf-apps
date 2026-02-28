using Microsoft.AspNetCore.Components;
using QuestFlag.Passport.AdminClient;

namespace QuestFlag.Passport.AdminWebApp.Client.Pages;

public partial class UsersPage
{
    [Parameter] public Guid TenantId { get; set; }
    private IReadOnlyList<UserAdminDto>? _users;
    private bool _showInvite;
    private string _invUsername = "", _invEmail = "", _invDisplayName = "", _invRole = "member";
    private bool _inviting;
    private bool _inviteSent;
    private string? _inviteError;

    protected override async Task OnInitializedAsync()
        => _users = await AdminClient.GetUsersAsync(TenantId);

    private async Task InviteUser()
    {
        _inviting = true;
        _inviteError = null;
        _inviteSent = false;
        try
        {
            await AdminClient.InviteUserAsync(TenantId, _invUsername, _invEmail, _invDisplayName, _invRole);
            _inviteSent = true;
            _users = await AdminClient.GetUsersAsync(TenantId);
        }
        catch (Exception ex) { _inviteError = ex.Message; }
        finally { _inviting = false; }
    }

    private async Task ForceLogout(Guid userId)
    {
        await AdminClient.ForceLogoutUserAsync(userId);
    }
}
