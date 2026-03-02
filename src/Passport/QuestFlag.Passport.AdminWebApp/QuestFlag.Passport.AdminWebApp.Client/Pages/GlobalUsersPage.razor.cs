using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using QuestFlag.Passport.AdminClient;

namespace QuestFlag.Passport.AdminWebApp.Client.Pages;

public partial class GlobalUsersPage
{
    private IReadOnlyList<UserAdminDto>? _users;
    private IReadOnlyList<TenantAdminDto>? _tenants;
    private string _searchQuery = "";
    
    private string SearchQuery
    {
        get => _searchQuery;
        set
        {
            if (_searchQuery != value)
            {
                _searchQuery = value;
                _ = LoadUsersAsync();
            }
        }
    }

    private bool _showInvite;
    private string _invTenantId = "";
    private string _invUsername = "", _invEmail = "", _invDisplayName = "", _invRole = "member";
    private bool _inviting;
    private bool _inviteSent;
    private string? _inviteError;

    private UserAdminDto? _editingUser;
    private string _editUsername = "", _editEmail = "", _editDisplayName = "", _editRole = "";
    private bool _editIsActive;
    private bool _updating;
    private string? _editError;

    private IEnumerable<UserAdminDto>? FilteredUsers => _users;

    protected override async Task OnInitializedAsync()
    {
        await LoadUsersAsync();
    }

    private async Task LoadUsersAsync()
    {
        _users = await AdminClient.GetGlobalUsersAsync(_searchQuery);
    }
    
    private async Task PrepareInvite()
    {
        if (_tenants == null)
        {
            _tenants = await AdminClient.GetTenantsAsync();
        }
        _showInvite = true;
        _inviteSent = false;
        _inviteError = null;
    }

    private async Task InviteUser()
    {
        if (!Guid.TryParse(_invTenantId, out var tenantId))
        {
            _inviteError = "Please select a valid tenant.";
            return;
        }

        _inviting = true;
        _inviteError = null;
        _inviteSent = false;
        try
        {
            await AdminClient.InviteUserAsync(tenantId, _invUsername, _invEmail, _invDisplayName, _invRole);
            _inviteSent = true;
            await LoadUsersAsync();
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
        _editRole = user.Role;
        _editError = null;
    }

    private async Task UpdateUser()
    {
        if (_editingUser == null) return;
        _updating = true;
        _editError = null;
        try
        {
            await AdminClient.UpdateUserAsync(_editingUser.TenantId, _editingUser.Id, _editUsername, _editEmail, _editDisplayName, _editIsActive, _editRole);
            await LoadUsersAsync();
            _editingUser = null;
        }
        catch (Exception ex) { _editError = ex.Message; }
        finally { _updating = false; }
    }

    private async Task DeleteUser(UserAdminDto user)
    {
        if (!await JSRuntime.InvokeAsync<bool>("confirm", $"Are you sure you want to delete {user.DisplayName}? This cannot be undone."))
            return;

        try
        {
            await AdminClient.DeleteUserAsync(user.TenantId, user.Id);
            await LoadUsersAsync();
        }
        catch (Exception) { /* Log error */ }
    }

    [Inject] private IJSRuntime JSRuntime { get; set; } = default!;
}
