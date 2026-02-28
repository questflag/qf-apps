using Microsoft.JSInterop;
using QuestFlag.Infrastructure.Client;
using QuestFlag.Passport.Client;
using System.Text.Json;

namespace QuestFlag.Infrastructure.WebApp.Client.Pages;

public partial class UploadsListPage : IDisposable
{
    private bool _loading = true;
    private PagedResult<UploadRecordDto>? _results;
    private PeriodicTimer? _timer;

    private List<TenantDto>? _tenants;
    private List<UserSummaryDto>? _usersForFilter;

    // Auth State
    private string? _token;
    private bool IsTenantAdmin => HasRole("tenant_admin");

    // Filter State
    private string FilterTenantSlug = "";
    private string FilterUserId = "";
    private string FilterCategory = "";
    private string FilterStatus = "";

    // Sort State
    private string _sortBy = "CreatedAtUtc";
    private string _sortDir = "desc";

    // Pager
    private int _page = 1;
    private int _pageSize = 50;

    protected override async Task OnInitializedAsync()
    {
        _token = await JS.InvokeAsync<string>("localStorage.getItem", "jwt_token");
        if (string.IsNullOrEmpty(_token))
        {
            Nav.NavigateTo("/login");
            return;
        }

        UploadApi.SetBearerToken(_token);

        // Load initial lookup data
        try
        {
            _tenants = (await PassportApi.GetTenantsAsync()).ToList();
        }
        catch { }

        await LoadDataAsync();

        // Start polling if any item is not completed
        StartPolling();
    }

    private void StartPolling()
    {
        _timer = new PeriodicTimer(TimeSpan.FromSeconds(5));
        _ = PollAsync();
    }

    private async Task PollAsync()
    {
        try
        {
            while (await _timer!.WaitForNextTickAsync())
            {
                if (_results?.Items.Any(r => r.Status == 0 || r.Status == 1) == true)
                {
                    await LoadDataAsync(background: true);
                }
            }
        }
        catch { /* ignored if task canceled */ }
    }

    private async Task LoadDataAsync(bool background = false)
    {
        if (!background) _loading = true;

        if (!background) StateHasChanged();

        try
        {
            _results = await UploadApi.GetUploadsAsync(
                tenantSlug: FilterTenantSlug,
                userIdFilter: FilterUserId,
                category: FilterCategory,
                status: FilterStatus,
                sortBy: _sortBy,
                sortDir: _sortDir,
                page: _page,
                pageSize: _pageSize
            );
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
        finally
        {
            _loading = false;
            StateHasChanged();
        }
    }

    private async Task OnTenantFilterChanged()
    {
        FilterUserId = "";
        _usersForFilter = null;

        var token = _token;
        if (!string.IsNullOrEmpty(FilterTenantSlug) && _tenants != null)
        {
            var t = _tenants.FirstOrDefault(x => x.Slug == FilterTenantSlug);
            if (t != null)
            {
                try
                {
                    _usersForFilter = (await PassportApi.GetUsersByTenantAsync(t.Id)).ToList();
                }
                catch { } // Might fail if they aren't admin and request another tenant's users
            }
        }
    }

    private async Task ApplyFilters()
    {
        _page = 1;
        await LoadDataAsync();
    }

    private async Task ResetFilters()
    {
        FilterTenantSlug = "";
        FilterUserId = "";
        FilterCategory = "";
        FilterStatus = "";
        _page = 1;
        await LoadDataAsync();
    }

    private async Task Sort(string column)
    {
        if (_sortBy == column)
        {
            _sortDir = _sortDir == "asc" ? "desc" : "asc";
        }
        else
        {
            _sortBy = column;
            _sortDir = "asc";
        }
        await LoadDataAsync();
    }

    private async Task DeleteRecordAsync(Guid id)
    {
        if (await JS.InvokeAsync<bool>("confirm", "Are you sure you want to delete this record entirely?"))
        {
            try
            {
                await UploadApi.DeleteUploadAsync(id);
                await LoadDataAsync();
            }
            catch (Exception ex)
            {
                await JS.InvokeVoidAsync("alert", $"Failed to delete: {ex.Message}");
            }
        }
    }

    private async Task DownloadLinkAsync(Guid id)
    {
        try
        {
            var url = await UploadApi.GetSignedDownloadUrlAsync(id);
            await JS.InvokeVoidAsync("open", url, "_blank");
        }
        catch (Exception ex)
        {
            await JS.InvokeVoidAsync("alert", $"Failed to get download URL: {ex.Message}");
        }
    }

    private async Task PrevPage() { if (_page > 1) { _page--; await LoadDataAsync(); } }
    private async Task NextPage() { if (_page * _pageSize < _results?.TotalCount) { _page++; await LoadDataAsync(); } }

    private Microsoft.AspNetCore.Components.RenderFragment SortIndicator(string column) => builder =>
    {
        if (_sortBy == column)
        {
            builder.OpenElement(0, "span");
            builder.AddAttribute(1, "class", "text-blue-500 ml-1");
            builder.AddContent(2, _sortDir == "asc" ? "▲" : "▼");
            builder.CloseElement();
        }
    };

    private bool HasRole(string roleName)
    {
        if (string.IsNullOrEmpty(_token)) return false;
        try
        {
            // Simple JWT decode without external libraries
            var parts = _token.Split('.');
            if (parts.Length < 2) return false;

            var payload = parts[1];
            payload = payload.Replace('-', '+').Replace('_', '/');
            switch (payload.Length % 4)
            {
                case 2: payload += "=="; break;
                case 3: payload += "="; break;
            }

            var jsonBytes = Convert.FromBase64String(payload);
            var json = System.Text.Encoding.UTF8.GetString(jsonBytes);
            using var doc = JsonDocument.Parse(json);

            if (doc.RootElement.TryGetProperty("role", out var roleEl))
            {
                if (roleEl.ValueKind == JsonValueKind.Array)
                {
                    foreach (var element in roleEl.EnumerateArray())
                        if (element.GetString() == roleName) return true;
                }
                else
                {
                    return roleEl.GetString() == roleName;
                }
            }
            return false;
        }
        catch
        {
            return false;
        }
    }

    public void Dispose()
    {
        _timer?.Dispose();
    }
}
