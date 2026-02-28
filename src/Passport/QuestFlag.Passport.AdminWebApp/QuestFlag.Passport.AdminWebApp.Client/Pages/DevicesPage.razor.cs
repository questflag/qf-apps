using Microsoft.AspNetCore.Components;
using QuestFlag.Passport.AdminClient;

namespace QuestFlag.Passport.AdminWebApp.Client.Pages;

public partial class DevicesPage
{
    [Parameter] public Guid UserId { get; set; }
    private IReadOnlyList<DeviceAdminDto>? _devices;

    protected override async Task OnInitializedAsync()
        => _devices = await AdminClient.GetUserDevicesAsync(UserId);

    private async Task RevokeDevice(Guid deviceId)
    {
        await AdminClient.RevokeUserDeviceAsync(deviceId);
        _devices = await AdminClient.GetUserDevicesAsync(UserId);
    }

    private async Task RevokeAll()
    {
        foreach (var d in _devices ?? [])
            await AdminClient.RevokeUserDeviceAsync(d.Id);
        _devices = await AdminClient.GetUserDevicesAsync(UserId);
    }
}
