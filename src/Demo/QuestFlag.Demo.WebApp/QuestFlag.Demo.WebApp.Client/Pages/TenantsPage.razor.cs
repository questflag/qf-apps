using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using QuestFlag.Passport.AdminClient;

namespace QuestFlag.Demo.WebApp.Client.Pages;

public partial class TenantsPage
{
    private IReadOnlyList<TenantAdminDto>? _tenants;
    private string _searchQuery = "";
    private bool _showCreate;
    private string _newName = "", _newSlug = "", _newDomain = "", _newSubSlug = "";
    private bool _creating;
    private string? _createError;

    private TenantAdminDto? _editingTenant;
    private string _editName = "", _editSlug = "", _editDomain = "", _editSubSlug = "";
    private bool _editIsActive;
    private bool _updating;
    private string? _editError;

    private IEnumerable<TenantAdminDto>? FilteredTenants => 
        string.IsNullOrWhiteSpace(_searchQuery) 
            ? _tenants 
            : _tenants?.Where(t => t.Name.Contains(_searchQuery, StringComparison.OrdinalIgnoreCase) || t.Slug.Contains(_searchQuery, StringComparison.OrdinalIgnoreCase));

    protected override async Task OnInitializedAsync()
        => _tenants = await AdminClient.GetTenantsAsync();

    private async Task CreateTenant()
    {
        _creating = true;
        _createError = null;
        try
        {
            await AdminClient.CreateTenantAsync(_newName, _newSlug,
                string.IsNullOrWhiteSpace(_newDomain) ? null : _newDomain,
                string.IsNullOrWhiteSpace(_newSubSlug) ? null : _newSubSlug);
            _tenants = await AdminClient.GetTenantsAsync();
            _showCreate = false;
            _newName = _newSlug = _newDomain = _newSubSlug = "";
        }
        catch (Exception ex) { _createError = ex.Message; }
        finally { _creating = false; }
    }

    private void StartEdit(TenantAdminDto tenant)
    {
        _editingTenant = tenant;
        _editName = tenant.Name;
        _editSlug = tenant.Slug;
        _editDomain = tenant.CustomDomain ?? "";
        _editSubSlug = tenant.SubdomainSlug ?? "";
        _editIsActive = tenant.IsActive;
        _editError = null;
    }

    private async Task UpdateTenant()
    {
        if (_editingTenant == null) return;
        _updating = true;
        _editError = null;
        try
        {
            await AdminClient.UpdateTenantAsync(_editingTenant.Id, _editName, _editSlug, _editIsActive,
                string.IsNullOrWhiteSpace(_editDomain) ? null : _editDomain,
                string.IsNullOrWhiteSpace(_editSubSlug) ? null : _editSubSlug);
            _tenants = await AdminClient.GetTenantsAsync();
            _editingTenant = null;
        }
        catch (Exception ex) { _editError = ex.Message; }
        finally { _updating = false; }
    }

    private async Task DeleteTenant(Guid id)
    {
        if (!await JSRuntime.InvokeAsync<bool>("confirm", $"Are you sure you want to delete this tenant? This action cannot be undone."))
            return;

        try
        {
            await AdminClient.DeleteTenantAsync(id);
            _tenants = await AdminClient.GetTenantsAsync();
        }
        catch (Exception) { /* Log error or show toast */ }
    }

    [Inject] private IJSRuntime JSRuntime { get; set; } = default!;
}
