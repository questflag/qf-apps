using System;
using QuestFlag.Passport.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using QuestFlag.Passport.Core.Data;
using QuestFlag.Passport.Core.Implementations.Repositories;
using QuestFlag.Passport.Core.Implementations.Messaging;
using QuestFlag.Passport.Domain.Contracts;
using QuestFlag.Passport.Domain.Models;

namespace QuestFlag.Passport.Core.DependencyInjection;

public static class PassportCoreExtensions
{
    public static IServiceCollection AddPassportCore(this IServiceCollection services, IConfiguration configuration)
    {
        // 1. EF Core DbContext
        services.AddDbContext<PassportDbContext>(options =>
        {
            options.UseNpgsql(configuration.GetConnectionString("PassportConnection"));
        });

        // 2. Repositories
        services.AddScoped<ITenantRepository, TenantRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<ITrustedDeviceRepository, TrustedDeviceRepository>();

        // 3. ASP.NET Core Identity
        services.AddIdentity<ApplicationUser, IdentityRole<Guid>>(options =>
        {
            options.Password.RequireDigit = false;
            options.Password.RequiredLength = 6;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequireUppercase = false;
            options.Password.RequireLowercase = false;
            options.User.RequireUniqueEmail = false;

            // Required for email-based confirmation tokens
            options.SignIn.RequireConfirmedEmail = false; // enforce in app logic, not middleware
        })
        .AddEntityFrameworkStores<PassportDbContext>()
        .AddDefaultTokenProviders();

        // 4. OpenIddict Core (Stores)
        services.AddOpenIddict()
            .AddCore(options =>
            {
                options.UseEntityFrameworkCore()
                       .UseDbContext<PassportDbContext>();
            });

        // 5. Messaging services
        services.Configure<EmailSettings>(configuration.GetSection(EmailSettings.SectionName));
        services.Configure<SmsSettings>(configuration.GetSection(SmsSettings.SectionName));
        services.Configure<TrustedDeviceSettings>(configuration.GetSection(TrustedDeviceSettings.SectionName));
        services.Configure<PassportDbSettings>(configuration.GetSection(PassportDbSettings.SectionName));

        services.AddSingleton<IEmailSender, SmtpEmailSender>();
        services.AddSingleton<ISmsSender, StubSmsSender>();    // swap for TwilioSmsSender in production

        return services;
    }
}
