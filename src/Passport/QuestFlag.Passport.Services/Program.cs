using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenIddict.Abstractions;
using QuestFlag.Infrastructure.ApiCore.StartupExtensions;
using QuestFlag.Passport.Application.DependencyInjection;
using QuestFlag.Passport.Core.DependencyInjection;
using QuestFlag.Passport.Services.Extensions;

var builder = WebApplication.CreateBuilder(args);

// 1. Layer DI
builder.Services.AddPassportApplication();
builder.Services.AddPassportCore(builder.Configuration);

// 2. Common API services (controllers, endpoint explorer, Swagger)
builder.Services.AddQuestFlagApiServices();

// 3. OpenIddict Server — Full SSO Provider
builder.Services.AddOpenIddict()
    .AddServer(options =>
    {
        // Enable all required endpoints
        options.SetAuthorizationEndpointUris("/connect/authorize")
               .SetTokenEndpointUris("/connect/token")
               .SetUserInfoEndpointUris("/connect/userinfo")
               .SetEndSessionEndpointUris("/connect/logout")
               .SetIntrospectionEndpointUris("/connect/introspect");

        // Allow Authorization Code + PKCE (primary SSO flow)
        options.AllowAuthorizationCodeFlow()
               .RequireProofKeyForCodeExchange();

        // Keep refresh token flow
        options.AllowRefreshTokenFlow();

        // Register scopes
        options.RegisterScopes(
            OpenIddictConstants.Scopes.OpenId,
            OpenIddictConstants.Scopes.Profile,
            OpenIddictConstants.Scopes.Email,
            OpenIddictConstants.Scopes.Phone,
            OpenIddictConstants.Scopes.Roles,
            OpenIddictConstants.Scopes.OfflineAccess);

        // Encryption and Signing credentials (swap for real certs in production)
        options.AddDevelopmentEncryptionCertificate()
               .AddDevelopmentSigningCertificate();

        // Disable access token encryption for external validation compatibility
        options.DisableAccessTokenEncryption();

        // ASP.NET Core integration + passthrough so controllers handle the endpoints
        options.UseAspNetCore()
               .EnableAuthorizationEndpointPassthrough()
               .EnableTokenEndpointPassthrough()
               .EnableUserInfoEndpointPassthrough()
               .EnableEndSessionEndpointPassthrough();
    })
    .AddValidation(options =>
    {
        options.UseLocalServer();
        options.UseAspNetCore();
    });

// 4. Authentication & Authorization
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = OpenIddict.Validation.AspNetCore.OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = OpenIddict.Validation.AspNetCore.OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme;
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("PassportAdmin", policy =>
        policy.RequireClaim(OpenIddictConstants.Claims.Role, "passport_admin"));
});

// 5. CORS for all web app origins — configured via ServiceUrls:* in appsettings.json
var infraWebApp          = builder.Configuration["ServiceUrls:InfraWebApp"]         ?? throw new InvalidOperationException("ServiceUrls:InfraWebApp is required.");
var infraWebAppHttp      = builder.Configuration["ServiceUrls:InfraWebAppHttp"]     ?? throw new InvalidOperationException("ServiceUrls:InfraWebAppHttp is required.");
var passportWebApp       = builder.Configuration["ServiceUrls:PassportWebApp"]      ?? throw new InvalidOperationException("ServiceUrls:PassportWebApp is required.");
var passportAdminWebApp  = builder.Configuration["ServiceUrls:PassportAdminWebApp"] ?? throw new InvalidOperationException("ServiceUrls:PassportAdminWebApp is required.");

builder.Services.AddCors(options =>
{
    options.AddPolicy("PassportClients",
        b => b.WithOrigins(
                infraWebApp,          // Infrastructure.WebApp
                infraWebAppHttp,
                passportWebApp,       // Passport.WebApp (SSO portal)
                passportAdminWebApp   // Passport.AdminWebApp
              )
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials()
              .WithExposedHeaders("Token-Expired"));
});

var app = builder.Build();

app.UseCors("PassportClients");

if (app.Environment.IsDevelopment())
{
    await app.InitializeDatabaseAsync();
}

// 6. Standard API pipeline (Swagger, HTTPS, Auth, Controllers)
app.UseQuestFlagApiPipeline();

app.Run();
