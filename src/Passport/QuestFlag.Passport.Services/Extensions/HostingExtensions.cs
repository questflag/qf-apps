using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using QuestFlag.Passport.Application.DependencyInjection;
using QuestFlag.Passport.Core.Data;
using QuestFlag.Passport.Core.DependencyInjection;
using QuestFlag.Passport.Domain.Entities;
using QuestFlag.Passport.Domain.Enums;
using OpenIddict.Abstractions;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;

namespace QuestFlag.Passport.Services.Extensions;

public static class HostingExtensions
{
    public static async Task InitializeDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<PassportDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

        try
        {
            await context.Database.MigrateAsync();

            // Seed Roles
            string[] roles = [PassportRole.PassportAdmin, PassportRole.TenantAdmin, PassportRole.User];
            foreach (var roleName in roles)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole<Guid>(roleName));
                }
            }

            // Seed initial Tenant & Admin user if none exist
            if (!await context.Tenants.AnyAsync())
            {
                var tenant = new Tenant { Name = "Default System", Slug = "default", IsActive = true };
                context.Tenants.Add(tenant);
                await context.SaveChangesAsync();

                var adminUser = new ApplicationUser
                {
                    UserName = "admin",
                    Email = "admin@questflag.local",
                    DisplayName = "System Administrator",
                    TenantId = tenant.Id,
                    IsActive = true
                };

                var result = await userManager.CreateAsync(adminUser, "Admin123!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, PassportRole.PassportAdmin);
                    await userManager.AddToRoleAsync(adminUser, PassportRole.TenantAdmin);
                }
            }

            logger.LogInformation("Database initialized and default data seeded.");

            await SeedOpenIddictApplicationsAsync(scope.ServiceProvider);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while initializing the database.");
            throw;
        }
    }

    private static async Task SeedOpenIddictApplicationsAsync(IServiceProvider services)
    {
        var manager = services.GetRequiredService<IOpenIddictApplicationManager>();
        var configuration = services.GetRequiredService<IConfiguration>();
        var logger = services.GetRequiredService<ILogger<Program>>();

        var apps = configuration.GetSection("OpenIddictApplications").Get<Dictionary<string, string>>();
        if (apps == null) return;

        foreach (var app in apps)
        {
            if (await manager.FindByClientIdAsync(app.Key) == null)
            {
                logger.LogInformation("Seeding OpenIddict application: {ClientId}", app.Key);

                var descriptor = new OpenIddictApplicationDescriptor
                {
                    ClientId = app.Key,
                    DisplayName = app.Key == "infra-webapp" ? "Infrastructure Web App" :
                                 app.Key == "passport-webapp" ? "Passport Web App" :
                                 "Passport Admin Web App",
                    RedirectUris = { new Uri($"{app.Value}/signin-oidc") },
                    PostLogoutRedirectUris = { new Uri($"{app.Value}/signout-callback-oidc") },
                    Permissions =
                    {
                        OpenIddictConstants.Permissions.Endpoints.Authorization,
                        OpenIddictConstants.Permissions.Endpoints.EndSession,
                        OpenIddictConstants.Permissions.Endpoints.Token,
                        OpenIddictConstants.Permissions.GrantTypes.AuthorizationCode,
                        OpenIddictConstants.Permissions.GrantTypes.RefreshToken,
                        OpenIddictConstants.Permissions.ResponseTypes.Code,
                        OpenIddictConstants.Permissions.Scopes.Email,
                        OpenIddictConstants.Permissions.Scopes.Profile,
                        OpenIddictConstants.Permissions.Scopes.Roles,
                        OpenIddictConstants.Permissions.Prefixes.Scope + OpenIddictConstants.Scopes.OpenId,
                        OpenIddictConstants.Permissions.Prefixes.Scope + OpenIddictConstants.Scopes.OfflineAccess
                    },
                    Requirements =
                    {
                        OpenIddictConstants.Requirements.Features.ProofKeyForCodeExchange
                    }
                };

                await manager.CreateAsync(descriptor);
            }
        }
    }
}
