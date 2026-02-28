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
            string[] roles = { PassportRole.TenantAdmin, PassportRole.User };
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
                    await userManager.AddToRoleAsync(adminUser, PassportRole.TenantAdmin);
                }
            }

            logger.LogInformation("Database initialized and default data seeded.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while initializing the database.");
            throw;
        }
    }
}
