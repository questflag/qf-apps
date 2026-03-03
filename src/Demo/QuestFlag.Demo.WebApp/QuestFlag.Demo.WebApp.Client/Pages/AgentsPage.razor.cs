using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using QuestFlag.Passport.AdminClient;

namespace QuestFlag.Demo.WebApp.Client.Pages;

public partial class AgentsPage
{
    private IReadOnlyList<AgentDto>? _agents;
    private string _searchQuery = "";
    private bool _showCreate;
    private bool _creating, _updating;
    private string? _createError, _editError;

    private string _newClientId = "", _newDisplayName = "", _newSecret = "", _newType = "public";
    
    private AgentDto? _editingAgent;
    private string _editDisplayName = "", _editType = "", _editRedirectUris = "";

    private IEnumerable<AgentDto> FilteredAgents => 
        string.IsNullOrWhiteSpace(_searchQuery) 
            ? (_agents ?? Array.Empty<AgentDto>()) 
            : (_agents?.Where(a => a.ClientId.Contains(_searchQuery, StringComparison.OrdinalIgnoreCase) || a.DisplayName.Contains(_searchQuery, StringComparison.OrdinalIgnoreCase)) ?? Array.Empty<AgentDto>());

    protected override async Task OnInitializedAsync() => await LoadAgentsAsync();

    private async Task LoadAgentsAsync() => _agents = await AdminClient.GetAgentsAsync();

    private async Task CreateAgent()
    {
        _creating = true; _createError = null;
        try {
            await AdminClient.CreateAgentAsync(new CreateAgentRequest(
                _newClientId, _newDisplayName, string.IsNullOrWhiteSpace(_newSecret) ? null : _newSecret, _newType,
                new HashSet<string>(), new HashSet<Uri>(), new HashSet<Uri>()));
            _showCreate = false;
            await LoadAgentsAsync();
        } catch (Exception ex) { _createError = ex.Message; }
        finally { _creating = false; }
    }

    private void StartEdit(AgentDto a)
    {
        _editingAgent = a;
        _editDisplayName = a.DisplayName;
        _editType = a.Type;
        _editRedirectUris = string.Join(", ", a.RedirectUris);
        _editError = null;
    }

    private async Task UpdateAgent()
    {
        if (_editingAgent == null) return;
        _updating = true; _editError = null;
        try {
            var uris = _editRedirectUris.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(u => new Uri(u.Trim())).ToHashSet();
            await AdminClient.UpdateAgentAsync(_editingAgent.ClientId, new UpdateAgentRequest(
                _editingAgent.ClientId, _editDisplayName, null, _editType,
                _editingAgent.Permissions, uris, _editingAgent.PostLogoutRedirectUris));
            _editingAgent = null;
            await LoadAgentsAsync();
        } catch (Exception ex) { _editError = ex.Message; }
        finally { _updating = false; }
    }

    private async Task DeleteAgent(string clientId)
    {
        if (!await JSRuntime.InvokeAsync<bool>("confirm", $"Delete agent {clientId}?")) return;
        await AdminClient.DeleteAgentAsync(clientId);
        await LoadAgentsAsync();
    }
}
