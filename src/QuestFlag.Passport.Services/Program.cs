using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using OpenIddict.Abstractions;
using QuestFlag.Passport.Application.DependencyInjection;
using QuestFlag.Passport.Core.DependencyInjection;
using QuestFlag.Passport.Services.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add layers
builder.Services.AddPassportApplication();
builder.Services.AddPassportCore(builder.Configuration);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// OpenIddict Server Configuration — Full SSO Provider
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

        // Configure JWT output (disable encryption for external validation compat)
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
        // Validate tokens issued by this server + support remote introspection
        options.UseLocalServer();
        options.UseAspNetCore();
    });

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

// Configure CORS for all web app origins
builder.Services.AddCors(options =>
{
    options.AddPolicy("PassportClients",
        b => b.WithOrigins(
                "https://localhost:7000",  // Infrastructure.WebApp
                "http://localhost:5000",
                "https://localhost:7003",  // Passport.WebApp (SSO portal)
                "https://localhost:7004"   // Passport.AdminWebApp
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
    app.UseSwagger();
    app.UseSwaggerUI();
    await app.InitializeDatabaseAsync();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

