using Microsoft.AspNetCore.Components;
using QuestFlag.Passport.AdminClient;

namespace QuestFlag.Passport.AdminWebApp.Client.Pages;

public partial class TenantsPage
{
    private IReadOnlyList<TenantAdminDto>? _tenants;
    private bool _showCreate;
    private string _newName = "", _newSlug = "", _newDomain = "", _newSubSlug = "";
    private bool _creating;
    private string? _createError;

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
}
