using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenIddict.Abstractions;
using QuestFlag.Passport.Core.Data;
using QuestFlag.Passport.Domain.Entities;
using QuestFlag.Passport.Services.Settings;

namespace QuestFlag.Passport.Services.Extensions;

public static class HostingExtensions
{
    public static async Task InitializeDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var context       = scope.ServiceProvider.GetRequiredService<PassportDbContext>();
        var logger        = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

        try
        {
            await context.Database.MigrateAsync();
            await SeedAsync(scope.ServiceProvider, logger);
            logger.LogInformation("Database initialized and seed data applied.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while initializing the database.");
            throw;
        }
    }

    // -------------------------------------------------------------------------
    // Single entry-point for all seed data — everything is config-driven.
    // -------------------------------------------------------------------------
    private static async Task SeedAsync(IServiceProvider services, ILogger logger)
    {
        var configuration = services.GetRequiredService<IConfiguration>();
        var seed = configuration.GetSection(SeedDataSettings.SectionName).Get<SeedDataSettings>()
                   ?? new SeedDataSettings();

        await SeedRolesAsync(services, seed, logger);
        var tenant = await SeedTenantAsync(services, seed, logger);
        await SeedAdminUserAsync(services, seed, tenant, logger);
        await SeedOidcAppsAsync(services, seed, logger);
        await SeedAgentsAsync(services, seed, logger);
    }

    // -------------------------------------------------------------------------
    // 1. Roles
    // -------------------------------------------------------------------------
    private static async Task SeedRolesAsync(IServiceProvider services, SeedDataSettings seed, ILogger logger)
    {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole<Guid>>>();

        foreach (var roleName in seed.Roles)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                logger.LogInformation("Seeding role: {Role}", roleName);
                await roleManager.CreateAsync(new IdentityRole<Guid>(roleName));
            }
        }
    }

    // -------------------------------------------------------------------------
    // 2. Default tenant (only when no tenants exist)
    // -------------------------------------------------------------------------
    private static async Task<Tenant?> SeedTenantAsync(IServiceProvider services, SeedDataSettings seed, ILogger logger)
    {
        var context = services.GetRequiredService<PassportDbContext>();

        if (await context.Tenants.AnyAsync())
            return null;

        var cfg = seed.Tenant;
        var tenant = new Tenant
        {
            Name     = cfg.Name,
            Slug     = cfg.Slug,
            IsActive = cfg.IsActive
        };

        context.Tenants.Add(tenant);
        await context.SaveChangesAsync();
        logger.LogInformation("Seeded default tenant: {TenantName} / {Slug}", tenant.Name, tenant.Slug);
        return tenant;
    }

    // -------------------------------------------------------------------------
    // 3. Admin user — create if missing, then ensure all configured roles.
    // -------------------------------------------------------------------------
    private static async Task SeedAdminUserAsync(
        IServiceProvider services,
        SeedDataSettings seed,
        Tenant? newTenant,
        ILogger logger)
    {
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var context     = services.GetRequiredService<PassportDbContext>();
        var cfg         = seed.AdminUser;

        if (string.IsNullOrWhiteSpace(cfg.UserName))
            return;

        // Create user alongside a freshly-seeded tenant
        if (newTenant != null && await userManager.FindByNameAsync(cfg.UserName) == null)
        {
            var user = new ApplicationUser
            {
                UserName    = cfg.UserName,
                Email       = cfg.Email,
                DisplayName = cfg.DisplayName,
                TenantId    = newTenant.Id,
                IsActive    = true
            };

            var result = await userManager.CreateAsync(user, cfg.Password);
            if (!result.Succeeded)
            {
                logger.LogWarning("Could not create admin user {UserName}: {Errors}",
                    cfg.UserName, string.Join(", ", result.Errors.Select(e => e.Description)));
                return;
            }

            logger.LogInformation("Seeded admin user: {UserName}", cfg.UserName);
        }

        // Ensure all configured roles are assigned (idempotent)
        var adminUser = await userManager.FindByNameAsync(cfg.UserName);
        if (adminUser == null) return;

        foreach (var role in cfg.Roles)
        {
            if (!await userManager.IsInRoleAsync(adminUser, role))
            {
                await userManager.AddToRoleAsync(adminUser, role);
                logger.LogInformation("Assigned role {Role} to {UserName}", role, cfg.UserName);
            }
        }
    }

    // -------------------------------------------------------------------------
    // 4. OIDC client apps (authorization-code flow)
    // -------------------------------------------------------------------------
    private static async Task SeedOidcAppsAsync(IServiceProvider services, SeedDataSettings seed, ILogger logger)
    {
        var manager = services.GetRequiredService<IOpenIddictApplicationManager>();

        foreach (var app in seed.OidcApps)
        {
            if (await manager.FindByClientIdAsync(app.ClientId) != null)
                continue;

            logger.LogInformation("Seeding OIDC app: {ClientId}", app.ClientId);

            var permissions = app.Permissions.Count > 0
                ? app.Permissions
                : DefaultCodeFlowPermissions();

            var descriptor = new OpenIddictApplicationDescriptor
            {
                ClientId    = app.ClientId,
                DisplayName = app.DisplayName,
                RedirectUris            = { new Uri($"{app.BaseUrl}/signin-oidc") },
                PostLogoutRedirectUris  = { new Uri($"{app.BaseUrl}/signout-callback-oidc") },
            };

            foreach (var p in permissions) descriptor.Permissions.Add(p);

            descriptor.Requirements.Add(OpenIddictConstants.Requirements.Features.ProofKeyForCodeExchange);

            await manager.CreateAsync(descriptor);
        }
    }

    // -------------------------------------------------------------------------
    // 5. Agent / service-account apps (client-credentials)
    // -------------------------------------------------------------------------
    private static async Task SeedAgentsAsync(IServiceProvider services, SeedDataSettings seed, ILogger logger)
    {
        var manager = services.GetRequiredService<IOpenIddictApplicationManager>();

        foreach (var agent in seed.Agents)
        {
            if (await manager.FindByClientIdAsync(agent.ClientId) != null)
                continue;

            logger.LogInformation("Seeding agent: {ClientId}", agent.ClientId);

            var descriptor = new OpenIddictApplicationDescriptor
            {
                ClientId     = agent.ClientId,
                DisplayName  = agent.DisplayName,
                ClientSecret = agent.ClientSecret,
                ClientType   = agent.Type
            };

            foreach (var p in agent.Permissions)
                descriptor.Permissions.Add(p);

            foreach (var u in agent.RedirectUris)
                descriptor.RedirectUris.Add(new Uri(u));

            foreach (var u in agent.PostLogoutRedirectUris)
                descriptor.PostLogoutRedirectUris.Add(new Uri(u));

            await manager.CreateAsync(descriptor);
        }
    }

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------
    private static List<string> DefaultCodeFlowPermissions() =>
    [
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
    ];
}
