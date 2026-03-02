using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using QuestFlag.Passport.AdminClient;

namespace QuestFlag.Passport.AdminWebApp.Client.Pages;

public partial class UsersPage
{
    [Parameter] public Guid TenantId { get; set; }
    private IReadOnlyList<UserAdminDto>? _users;
    private string _searchQuery = "";
    private bool _showInvite;
    private string _invUsername = "", _invEmail = "", _invDisplayName = "", _invRole = "member";
    private bool _inviting;
    private bool _inviteSent;
    private string? _inviteError;

    private UserAdminDto? _editingUser;
    private string _editUsername = "", _editEmail = "", _editDisplayName = "", _editRole = "";
    private bool _editIsActive;
    private bool _updating;
    private string? _editError;

    private IEnumerable<UserAdminDto>? FilteredUsers => 
        string.IsNullOrWhiteSpace(_searchQuery) 
            ? _users 
            : _users?.Where(u => u.DisplayName.Contains(_searchQuery, StringComparison.OrdinalIgnoreCase) || u.Username.Contains(_searchQuery, StringComparison.OrdinalIgnoreCase) || u.Email.Contains(_searchQuery, StringComparison.OrdinalIgnoreCase));

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
        if (!await JSRuntime.InvokeAsync<bool>("confirm", "Force logout will revoke all active sessions and trusted devices. Continue?"))
            return;
        await AdminClient.ForceLogoutUserAsync(userId);
    }

    private void StartEdit(UserAdminDto user)
    {
        _editingUser = user;
        _editUsername = user.Username;
        _editEmail = user.Email;
        _editDisplayName = user.DisplayName;
        _editIsActive = user.IsActive;
        _editRole = "member"; // Defaulting as we don't have GetUserRole endpoint yet, but UpdateUser needs it.
        _editError = null;
    }

    private async Task UpdateUser()
    {
        if (_editingUser == null) return;
        _updating = true;
        _editError = null;
        try
        {
            await AdminClient.UpdateUserAsync(TenantId, _editingUser.Id, _editUsername, _editEmail, _editDisplayName, _editIsActive, _editRole);
            _users = await AdminClient.GetUsersAsync(TenantId);
            _editingUser = null;
        }
        catch (Exception ex) { _editError = ex.Message; }
        finally { _updating = false; }
    }

    private async Task DeleteUser(Guid userId)
    {
        if (!await JSRuntime.InvokeAsync<bool>("confirm", "Are you sure you want to delete this user? This cannot be undone."))
            return;

        try
        {
            await AdminClient.DeleteUserAsync(TenantId, userId);
            _users = await AdminClient.GetUsersAsync(TenantId);
        }
        catch (Exception ex) { /* Log error */ }
    }

    [Inject] private IJSRuntime JSRuntime { get; set; } = default!;
}
