using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using QuestFlag.Infrastructure.Client;
using QuestFlag.Passport.Client;
using System;
using System.Net.Http;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

// Register PassportApiClient pointing to Passport API Host (local dev port 7002)
builder.Services.AddHttpClient<PassportApiClient>(client =>
{
    client.BaseAddress = new Uri("https://localhost:7002");
});

// Register UploadApiService pointing to Infrastructure API Host (local dev port 7001)
builder.Services.AddHttpClient<UploadApiService>(client =>
{
    client.BaseAddress = new Uri("https://localhost:7001");
});

await builder.Build().RunAsync();
