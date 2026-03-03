using Microsoft.AspNetCore.Components;

namespace QuestFlag.Passport.WebApp.Client.Pages;

public partial class LogoutPage
{
    [SupplyParameterFromQuery] public string? AppUrl { get; set; }
    [SupplyParameterFromQuery] public string? AppName { get; set; }
}
