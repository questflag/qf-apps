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

var builder = WebAssemblyHostBuilder.CreateDefault(args);

var passportServicesUrl = builder.Configuration["ServiceUrls:PassportServices"]
    ?? "https://localhost:7001";

var infraServicesUrl = builder.Configuration["ServiceUrls:InfraServices"]
    ?? "https://localhost:7002";

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

builder.Services.AddHttpClient<IUploadApiService, UploadApiService>(client =>
{
    client.BaseAddress = new Uri(infraServicesUrl);
}).AddHttpMessageHandler<AuthenticatedHttpHandler>();

builder.Services.AddAuthorizationCore();
builder.Services.AddCascadingAuthenticationState();

await builder.Build().RunAsync();
