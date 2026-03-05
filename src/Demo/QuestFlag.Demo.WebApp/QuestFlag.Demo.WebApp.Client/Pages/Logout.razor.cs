using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Threading.Tasks;

namespace QuestFlag.Demo.WebApp.Client.Pages;

public partial class Logout
{
    protected override async Task OnInitializedAsync()
    {
        await TokenProvider.ClearTokenAsync();
        try
        {
            await JS.InvokeVoidAsync("localStorage.removeItem", "user_info");
        }
        catch { /* Ignore */ }
        
        // Redirect to home since we are in pure token mode
        Nav.NavigateTo("/", forceLoad: true);
    }
}
