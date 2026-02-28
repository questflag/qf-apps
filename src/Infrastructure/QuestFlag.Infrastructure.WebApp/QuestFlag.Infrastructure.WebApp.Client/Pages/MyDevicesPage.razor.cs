using Microsoft.AspNetCore.Components;
using QuestFlag.Passport.UserClient;

namespace QuestFlag.Infrastructure.WebApp.Client.Pages;

public partial class MyDevicesPage
{
    private IReadOnlyList<DeviceDto>? _devices;
    private bool _isLoading = true;

    protected override async Task OnInitializedAsync()
    {
        await LoadDevices();
    }

    private async Task LoadDevices()
    {
        _isLoading = true;
        try
        {
            var rawDevices = await PassportClient.GetMyDevicesAsync();
            _devices = rawDevices?.ToList();
        }
        finally
        {
            _isLoading = false;
        }
    }

    private async Task RevokeDevice(Guid deviceId)
    {
        await PassportClient.RevokeDeviceAsync(deviceId);
        await LoadDevices();
    }

    private async Task RevokeAll()
    {
        await PassportClient.RevokeAllMyDevicesAsync();
        await LoadDevices();
    }
}
