using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using QuestFlag.Communication.Client.Contracts;
using QuestFlag.Communication.Client.Implementations;
using QuestFlag.Passport.Client;
using QuestFlag.Passport.UserClient;
using QuestFlag.Infrastructure.Client;
using QuestFlag.Infrastructure.Client.Contracts;
using System.Net.Http;
using Microsoft.AspNetCore.Components.Authorization;
using QuestFlag.Demo.WebApp.Client.State;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

var passportServicesUrl = builder.Configuration["ServiceUrls:PassportServices"]
    ?? throw new InvalidOperationException("ServiceUrls:PassportServices is required in configuration.");

var communicationServicesUrl = builder.Configuration["ServiceUrls:InfraServices"]
    ?? throw new InvalidOperationException("ServiceUrls:InfraServices is required in configuration.");

// Register PassportApiClient pointing to Passport API Host — configured via ServiceUrls:PassportServices
builder.Services.AddHttpClient<PassportApiClient>(client =>
{
    client.BaseAddress = new Uri(passportServicesUrl);
});

builder.Services.AddHttpClient<PassportUserClient>(client =>
{
    client.BaseAddress = new Uri(passportServicesUrl);
}).AddHttpMessageHandler<AuthenticatedHttpHandler>();

builder.Services.AddScoped<IAccessTokenProvider, TokenProvider>();
builder.Services.AddTransient<AuthenticatedHttpHandler>();

// Register IUploadApiService pointing to Communication API Host — configured via ServiceUrls:InfraServices
builder.Services.AddHttpClient<IUploadApiService, UploadApiService>(client =>
{
    client.BaseAddress = new Uri(communicationServicesUrl);
}).AddHttpMessageHandler<AuthenticatedHttpHandler>();

builder.Services.AddAuthorizationCore();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddSingleton<AuthenticationStateProvider, PersistentAuthenticationStateProvider>();

await builder.Build().RunAsync();
