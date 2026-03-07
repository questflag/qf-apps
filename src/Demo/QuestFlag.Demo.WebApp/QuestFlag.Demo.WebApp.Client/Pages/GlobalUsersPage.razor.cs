using System.Net;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using QuestFlag.Passport.AdminClient;

namespace QuestFlag.Demo.WebApp.Client.Pages;

public partial class GlobalUsersPage
{
    private IReadOnlyList<UserAdminDto>? _users;
    private IReadOnlyList<TenantAdminDto>? _tenants;
    private string _searchQuery = "";
    private string? _loadError;

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
        if (!OperatingSystem.IsBrowser())
        {
            // Skip protected API calls during server prerender.
            return;
        }

        await Task.WhenAll(LoadUsersAsync(), LoadMetadataAsync());
    }

    private async Task LoadMetadataAsync()
    {
        try
        {
            _availableRoles = await AdminClient.GetRolesAsync();
            _availableAgents = await AdminClient.GetAgentsAsync();
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"[GlobalUsersPage] Failed to load metadata. Status: {ex.StatusCode}; Message: {ex.Message}");
            if (ex.StatusCode == HttpStatusCode.Forbidden)
            {
                _loadError = "Access denied for user administration. Your session is active, but your account does not have permission for this action.";
            }
        }
    }

    private async Task LoadUsersAsync()
    {
        try
        {
            _users = await AdminClient.GetGlobalUsersAsync(_searchQuery);
            _loadError = null;
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"[GlobalUsersPage] Failed to load users. Status: {ex.StatusCode}; Message: {ex.Message}");
            _loadError = ex.StatusCode == HttpStatusCode.Forbidden
                ? "Access denied for global user directory. Your session is active, but your account does not have permission for this action."
                : "Unable to load users right now.";
        }
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
        _invRoles = new List<string> { "member" };
        _invAgentClientIds = new List<string>();
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
            await AdminClient.InviteUserAsync(tenantId, _invUsername, _invEmail, _invDisplayName, _invRoles, _invAgentClientIds);
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
            await AdminClient.UpdateUserAsync(_editingUser.TenantId, _editingUser.Id, _editUsername, _editEmail, _editDisplayName, _editIsActive, _editRoles, _editAgentClientIds);
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
        catch (Exception ex)
        {
            Console.WriteLine($"[GlobalUsersPage] Failed to delete user {user.Id}: {ex.Message}");
        }
    }

    [Inject] private IJSRuntime JSRuntime { get; set; } = default!;
}
