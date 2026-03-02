using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using QuestFlag.Passport.AdminClient;

namespace QuestFlag.Passport.AdminWebApp.Client.Pages;

public partial class UsersPage
{
    [Parameter] public Guid TenantId { get; set; }
    private IReadOnlyList<UserAdminDto>? _users;
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
    private string _invUsername = "", _invEmail = "", _invDisplayName = "";
    private List<string> _invRoles = new();
    private List<string> _invAgentClientIds = new();
    
    private IReadOnlyList<RoleDto>? _availableRoles;
    private IReadOnlyList<AgentDto>? _availableAgents;
    private bool _inviting;
    private bool _inviteSent;
    private string? _inviteError;

    private UserAdminDto? _editingUser;
    private string _editUsername = "", _editEmail = "", _editDisplayName = "";
    private List<string> _editRoles = new();
    private List<string> _editAgentClientIds = new();
    private bool _editIsActive;
    private bool _updating;
    private string? _editError;

    private IEnumerable<UserAdminDto>? FilteredUsers => _users;

    protected override async Task OnInitializedAsync()
    {
        await Task.WhenAll(LoadUsersAsync(), LoadMetadataAsync());
    }

    private async Task LoadMetadataAsync()
    {
        _availableRoles = await AdminClient.GetRolesAsync();
        _availableAgents = await AdminClient.GetAgentsAsync();
    }

    private async Task LoadUsersAsync()
        => _users = await AdminClient.GetUsersAsync(TenantId, _searchQuery);

    private async Task PrepareInvite()
    {
        _showInvite = true;
        _inviteSent = false;
        _inviteError = null;
        _invRoles = new List<string> { "member" };
        _invAgentClientIds = new List<string>();
    }

    private async Task InviteUser()
    {
        _inviting = true;
        _inviteError = null;
        _inviteSent = false;
        try
        {
            await AdminClient.InviteUserAsync(TenantId, _invUsername, _invEmail, _invDisplayName, _invRoles, _invAgentClientIds);
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
        _editRoles = user.Roles?.ToList() ?? new List<string>();
        _editAgentClientIds = user.AssignedAgentIds?.ToList() ?? new List<string>();
        _editError = null;
    }

    private async Task UpdateUser()
    {
        if (_editingUser == null) return;
        _updating = true;
        _editError = null;
        try
        {
            await AdminClient.UpdateUserAsync(TenantId, _editingUser.Id, _editUsername, _editEmail, _editDisplayName, _editIsActive, _editRoles, _editAgentClientIds);
            await LoadUsersAsync();
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
        catch (Exception) { /* Log error */ }
    }

    [Inject] private IJSRuntime JSRuntime { get; set; } = default!;
}
