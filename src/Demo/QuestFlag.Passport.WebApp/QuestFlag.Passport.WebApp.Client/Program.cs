using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using QuestFlag.Passport.UserClient;
using System;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddHttpClient<PassportUserClient>(client =>
{
    var passportServicesUrl = builder.Configuration["Passport:PassportServicesBaseUrl"] 
        ?? "https://localhost:7004";
    client.BaseAddress = new Uri(passportServicesUrl);
});

await builder.Build().RunAsync();
