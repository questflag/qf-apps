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

// OpenIddict Server Configuration
builder.Services.AddOpenIddict()
    .AddServer(options =>
    {
        // Enable endpoints
        options.SetTokenEndpointUris("/connect/token");

        // Enable flows
        options.AllowPasswordFlow()
               .AllowRefreshTokenFlow();

        // Accept anonymous clients (we don't enforce client_id/client_secret for this internal app)
        options.AcceptAnonymousClients();

        // Encryption and Signing credentials (development only)
        options.AddDevelopmentEncryptionCertificate()
               .AddDevelopmentSigningCertificate();

        // Register scopes
        options.RegisterScopes(OpenIddictConstants.Scopes.Roles, OpenIddictConstants.Scopes.OfflineAccess);

        // Required to use AspNetCore integration
        options.UseAspNetCore()
               .EnableTokenEndpointPassthrough();
               
        // Configure JWT output
        options.DisableAccessTokenEncryption();
    })
    .AddValidation(options =>
    {
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
    options.AddPolicy("TenantAdmin", policy => policy.RequireClaim(OpenIddictConstants.Claims.Role, "tenant_admin"));
});

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazorClient",
        b => b.WithOrigins("https://localhost:7000", "http://localhost:5000") // UI hosts
              .AllowAnyMethod()
              .AllowAnyHeader()
              .WithExposedHeaders("Token-Expired"));
});

var app = builder.Build();

app.UseCors("AllowBlazorClient");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    // Initialize Db and seed roles/admin if configured
    await app.InitializeDatabaseAsync();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
