using QuestFlag.Passport.WebApp.Client.Pages;
using QuestFlag.Passport.WebApp.Components;
using QuestFlag.Infrastructure.ApiCore.StartupExtensions;
using QuestFlag.Passport.UserClient;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddQuestFlagApiServices();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

builder.Services.AddHttpClient<PassportUserClient>(client =>
{
    var passportServicesUrl = builder.Configuration["Passport:PassportServicesBaseUrl"] 
        ?? builder.Configuration["ServiceUrls:PassportServices"]
        ?? "https://localhost:7004";
    client.BaseAddress = new Uri(passportServicesUrl);
});

var app = builder.Build();

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();
app.UseAntiforgery();
app.UseQuestFlagApiPipeline();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(QuestFlag.Passport.WebApp.Client._Imports).Assembly);

app.Run();
