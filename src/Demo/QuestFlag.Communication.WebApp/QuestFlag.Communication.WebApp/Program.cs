using QuestFlag.Communication.WebApp.Components;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using QuestFlag.Communication.WebApp.State;
using QuestFlag.Infrastructure.Client;
using QuestFlag.Infrastructure.Client.Contracts;
using QuestFlag.Passport.Client;
using QuestFlag.Passport.UserClient;
using global::QuestFlag.Infrastructure.ApiCore.StartupExtensions;
using QuestFlag.Communication.Client.Implementations;
using QuestFlag.Communication.Client.Contracts;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

builder.Services.AddQuestFlagApiServices();
builder.Services.AddScoped<AuthenticationStateProvider, PersistingServerAuthenticationStateProvider>();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddHttpContextAccessor();

var passportServicesUrl = builder.Configuration["ServiceUrls:PassportServices"]
    ?? "https://localhost:7001";

var infraServicesUrl = builder.Configuration["ServiceUrls:InfraServices"]
    ?? "https://localhost:7002";

builder.Services.AddScoped<IAccessTokenProvider, ServerTokenProvider>();
builder.Services.AddTransient<AuthenticatedHttpHandler>();

// Register Client APIs for server-side rendering/pre-rendering
builder.Services.AddHttpClient<PassportApiClient>(client =>
{
    client.BaseAddress = new Uri(passportServicesUrl);
});

builder.Services.AddHttpClient<IUploadApiService, UploadApiService>(client =>
{
    client.BaseAddress = new Uri(infraServicesUrl);
}).AddHttpMessageHandler<AuthenticatedHttpHandler>();

builder.Services.AddHttpClient<PassportUserClient>(client =>
{
    client.BaseAddress = new Uri(passportServicesUrl);
}).AddHttpMessageHandler<AuthenticatedHttpHandler>();

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
})
.AddCookie(CookieAuthenticationDefaults.AuthenticationScheme)
.AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, options =>
{
    options.Authority = passportServicesUrl;
    options.ClientId = "infra-webapp";
    options.ResponseType = OpenIdConnectResponseType.Code;
    options.SaveTokens = true;
    options.RequireHttpsMetadata = false;
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
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseAntiforgery();
app.UseQuestFlagApiPipeline();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(QuestFlag.Communication.WebApp.Client._Imports).Assembly);

app.MapGet("/login", async (HttpContext context) =>
{
    await context.ChallengeAsync(OpenIdConnectDefaults.AuthenticationScheme, new AuthenticationProperties
    {
        RedirectUri = "/"
    });
});

app.MapPost("/account/logout", async (HttpContext context) =>
{
    await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    await context.SignOutAsync(OpenIdConnectDefaults.AuthenticationScheme);
}).RequireAuthorization();

app.Run();
