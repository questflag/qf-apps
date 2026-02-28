using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using QuestFlag.Infrastructure.Client;
using QuestFlag.Passport.Client;
using System.Net.Http;
using Microsoft.AspNetCore.Components.Authorization;
using QuestFlag.Infrastructure.WebApp.Client.State;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

var passportServicesUrl = builder.Configuration["ServiceUrls:PassportServices"]
    ?? throw new InvalidOperationException("ServiceUrls:PassportServices is required in configuration.");

var infraServicesUrl = builder.Configuration["ServiceUrls:InfraServices"]
    ?? throw new InvalidOperationException("ServiceUrls:InfraServices is required in configuration.");

// Register PassportApiClient pointing to Passport API Host — configured via ServiceUrls:PassportServices
builder.Services.AddHttpClient<PassportApiClient>(client =>
{
    client.BaseAddress = new Uri(passportServicesUrl);
});

builder.Services.AddScoped<IAccessTokenProvider, TokenProvider>();
builder.Services.AddTransient<AuthenticatedHttpHandler>();

// Register UploadApiService pointing to Infrastructure API Host — configured via ServiceUrls:InfraServices
builder.Services.AddHttpClient<UploadApiService>(client =>
{
    client.BaseAddress = new Uri(infraServicesUrl);
}).AddHttpMessageHandler<AuthenticatedHttpHandler>();

builder.Services.AddAuthorizationCore();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddSingleton<AuthenticationStateProvider, PersistentAuthenticationStateProvider>();

await builder.Build().RunAsync();
