using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using QuestFlag.Passport.AdminClient;

namespace QuestFlag.Passport.AdminWebApp.Client.Pages;

public partial class RolesPage
{
    private IReadOnlyList<RoleDto>? _roles;
    private string _searchQuery = "";
    private bool _showCreate;
    private string _newName = "";
    private bool _creating;
    private string? _createError;

    private RoleDto? _editingRole;
    private string _editName = "";
    private bool _updating;
    private string? _editError;

    private IEnumerable<RoleDto>? FilteredRoles => 
        string.IsNullOrWhiteSpace(_searchQuery) 
            ? _roles 
            : _roles?.Where(r => r.Name.Contains(_searchQuery, StringComparison.OrdinalIgnoreCase));

    protected override async Task OnInitializedAsync()
        => _roles = await AdminClient.GetRolesAsync();

    private async Task CreateRole()
    {
        _creating = true;
        _createError = null;
        try
        {
            await AdminClient.CreateRoleAsync(_newName);
            _roles = await AdminClient.GetRolesAsync();
            _showCreate = false;
            _newName = "";
        }
        catch (Exception ex) { _createError = ex.Message; }
        finally { _creating = false; }
    }

    private void StartEdit(RoleDto role)
    {
        _editingRole = role;
        _editName = role.Name;
        _editError = null;
    }

    private async Task UpdateRole()
    {
        if (_editingRole == null) return;
        _updating = true;
        _editError = null;
        try
        {
            await AdminClient.UpdateRoleAsync(_editingRole.Id, _editName);
            _roles = await AdminClient.GetRolesAsync();
            _editingRole = null;
        }
        catch (Exception ex) { _editError = ex.Message; }
        finally { _updating = false; }
    }

    private async Task DeleteRole(Guid id)
    {
        if (!await JSRuntime.InvokeAsync<bool>("confirm", "Are you sure you want to delete this role?"))
            return;

        try
        {
            await AdminClient.DeleteRoleAsync(id);
            _roles = await AdminClient.GetRolesAsync();
        }
        catch (Exception) { /* Log error */ }
    }

    [Inject] private IJSRuntime JSRuntime { get; set; } = default!;
}
