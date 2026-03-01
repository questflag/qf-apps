using QuestFlag.Infrastructure.WebApp.Client.Pages;
using QuestFlag.Infrastructure.WebApp.Components;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using QuestFlag.Infrastructure.WebApp.State;
using QuestFlag.Infrastructure.Client;
using QuestFlag.Passport.Client;
using QuestFlag.Infrastructure.ApiCore.StartupExtensions;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddQuestFlagApiServices();
builder.Services.AddScoped<AuthenticationStateProvider, PersistingServerAuthenticationStateProvider>();
builder.Services.AddHttpContextAccessor();

var passportServicesUrl = builder.Configuration["ServiceUrls:PassportServices"]
    ?? throw new InvalidOperationException("ServiceUrls:PassportServices is required in configuration.");

var infraServicesUrl = builder.Configuration["ServiceUrls:InfraServices"]
    ?? throw new InvalidOperationException("ServiceUrls:InfraServices is required in configuration.");

// Register Client APIs for server-side rendering/pre-rendering
builder.Services.AddHttpClient<PassportApiClient>(client =>
{
    client.BaseAddress = new Uri(passportServicesUrl);
});

builder.Services.AddHttpClient<UploadApiService>(client =>
{
    client.BaseAddress = new Uri(infraServicesUrl);
});

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
})
.AddCookie(CookieAuthenticationDefaults.AuthenticationScheme)
.AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, options =>
{
    options.Authority = passportServicesUrl; // Passport.Services — configured via ServiceUrls:PassportServices
    options.ClientId = "infra-webapp";
    options.ResponseType = OpenIdConnectResponseType.Code;
    options.SaveTokens = true;
    options.RequireHttpsMetadata = false; // Valid for local dev only
    options.GetClaimsFromUserInfoEndpoint = true;

    options.Scope.Clear();
    options.Scope.Add("openid");
    options.Scope.Add("profile");
    options.Scope.Add("email");
    options.Scope.Add("offline_access");

    options.TokenValidationParameters.NameClaimType = "name";
    options.TokenValidationParameters.RoleClaimType = "role";
});

builder.Services.AddAuthorization();

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
app.UseAuthentication();
app.UseAuthorization();
app.UseQuestFlagApiPipeline();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(QuestFlag.Infrastructure.WebApp.Client._Imports).Assembly);

app.MapPost("/account/logout", async (HttpContext context) =>
{
    await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    await context.SignOutAsync(OpenIdConnectDefaults.AuthenticationScheme);
}).RequireAuthorization();

app.Run();
