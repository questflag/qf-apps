using System;
using QuestFlag.Passport.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using QuestFlag.Passport.Core.Data;
using QuestFlag.Passport.Core.Implementations.Repositories;
using QuestFlag.Infrastructure.Core.Implementations.Messaging;
using QuestFlag.Passport.Domain.Contracts;
using QuestFlag.Passport.Domain.Models;
using QuestFlag.Infrastructure.Domain.Models;

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
        var identitySettings = configuration.GetSection(IdentitySettings.SectionName).Get<IdentitySettings>()
                               ?? new IdentitySettings();
        services.Configure<IdentitySettings>(configuration.GetSection(IdentitySettings.SectionName));

        services.AddIdentity<ApplicationUser, IdentityRole<Guid>>(options =>
        {
            options.Password.RequireDigit           = identitySettings.RequireDigit;
            options.Password.RequiredLength         = identitySettings.RequiredLength;
            options.Password.RequireNonAlphanumeric = identitySettings.RequireNonAlphanumeric;
            options.Password.RequireUppercase       = identitySettings.RequireUppercase;
            options.Password.RequireLowercase       = identitySettings.RequireLowercase;
            options.User.RequireUniqueEmail         = identitySettings.RequireUniqueEmail;
            options.SignIn.RequireConfirmedEmail     = identitySettings.RequireConfirmedEmail;
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

        services.AddSingleton<QuestFlag.Infrastructure.Domain.Contracts.IEmailSender, QuestFlag.Infrastructure.Core.Implementations.Messaging.SmtpEmailSender>();
        services.AddSingleton<QuestFlag.Infrastructure.Domain.Contracts.ISmsSender, QuestFlag.Infrastructure.Core.Implementations.Messaging.StubSmsSender>();    // swap for TwilioSmsSender in production

        return services;
    }
}
