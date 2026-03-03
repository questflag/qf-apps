using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using QuestFlag.Communication.Client.Contracts;
using QuestFlag.Communication.Client.Implementations;
using QuestFlag.Passport.UserClient;
using QuestFlag.Infrastructure.Client;
using QuestFlag.Infrastructure.Client.Contracts;
using System.Net.Http;
using Microsoft.AspNetCore.Components.Authorization;
using QuestFlag.Communication.WebApp.Client.State;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

var passportServicesUrl = builder.Configuration["ServiceUrls:PassportServices"]
    ?? "https://localhost:7001";

var infraServicesUrl = builder.Configuration["ServiceUrls:InfraServices"]
    ?? "https://localhost:7002";

builder.Services.AddHttpClient<PassportUserClient>(client =>
{
    client.BaseAddress = new Uri(passportServicesUrl);
}).AddHttpMessageHandler<AuthenticatedHttpHandler>();

builder.Services.AddScoped<IAccessTokenProvider, QuestFlag.Communication.WebApp.Client.State.TokenProvider>();
builder.Services.AddTransient<AuthenticatedHttpHandler>();

builder.Services.AddHttpClient<IUploadApiService, UploadApiService>(client =>
{
    client.BaseAddress = new Uri(infraServicesUrl);
}).AddHttpMessageHandler<AuthenticatedHttpHandler>();

builder.Services.AddAuthorizationCore();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddSingleton<AuthenticationStateProvider, PersistentAuthenticationStateProvider>();

await builder.Build().RunAsync();
