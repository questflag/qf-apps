using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Components.Authorization;
using QuestFlag.Passport.AdminWebApp.Client.State;
using QuestFlag.Infrastructure.Client;
using QuestFlag.Infrastructure.Client.Contracts;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddAuthorizationCore();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<AuthenticationStateProvider, PersistentAuthenticationStateProvider>();
builder.Services.AddScoped<IAccessTokenProvider, TokenProvider>();

builder.Services.AddTransient<AuthenticatedHttpHandler>();
var passportServicesUrl = builder.Configuration["ServiceUrls:PassportServices"]
    ?? throw new InvalidOperationException("ServiceUrls:PassportServices is required.");

builder.Services.AddHttpClient<QuestFlag.Passport.AdminClient.PassportAdminClient>(client =>
{
    client.BaseAddress = new Uri(passportServicesUrl);
})
.AddHttpMessageHandler<AuthenticatedHttpHandler>();

await builder.Build().RunAsync();
