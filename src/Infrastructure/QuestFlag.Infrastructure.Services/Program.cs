using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenIddict.Validation.AspNetCore;
using QuestFlag.Infrastructure.ApiCore.StartupExtensions;
using QuestFlag.Infrastructure.Application.DependencyInjection;
using QuestFlag.Infrastructure.Core.Data;
using QuestFlag.Infrastructure.Core.DependencyInjection;
using Microsoft.EntityFrameworkCore;

namespace QuestFlag.Infrastructure.Services;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // 1. Layer DI
        builder.Services.AddInfrastructureApplication();
        builder.Services.AddInfrastructureCore(builder.Configuration);

        // HttpClient for Kafka consumer downstream calls
        builder.Services.AddHttpClient();

        // 2. Common API services (controllers, endpoint explorer, Swagger)
        builder.Services.AddQuestFlagApiServices();

        // 3. Authentication & Authorization (consuming Passport JWT via Introspection)
        builder.Services.AddAuthentication(OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme);

        var passportServicesUrl = builder.Configuration["ServiceUrls:PassportServices"]
            ?? throw new InvalidOperationException("ServiceUrls:PassportServices is required in configuration.");

        builder.Services.AddOpenIddict()
            .AddValidation(options =>
            {
                // Passport Server's address — configured via ServiceUrls:PassportServices
                options.SetIssuer(passportServicesUrl);
                options.AddAudiences("infra-api");

                // Validate against the Passport Introspection endpoint
                options.UseIntrospection()
                       .SetClientId("infra-api")
                       .SetClientSecret("infra-api-secret"); // Should come from config in production

                options.UseSystemNetHttp();
                options.UseAspNetCore();
            });

        builder.Services.AddAuthorization(options =>
        {
            options.AddPolicy("TenantAdmin", policy =>
                policy.RequireClaim("role", "tenant_admin"));
        });

        // 4. CORS — origins configured via ServiceUrls:* in appsettings.json
        var infraWebApp     = builder.Configuration["ServiceUrls:InfraWebApp"]     ?? throw new InvalidOperationException("ServiceUrls:InfraWebApp is required.");
        var infraWebAppHttp = builder.Configuration["ServiceUrls:InfraWebAppHttp"] ?? throw new InvalidOperationException("ServiceUrls:InfraWebAppHttp is required.");

        builder.Services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
            {
                policy.WithOrigins(infraWebApp, infraWebAppHttp, passportServicesUrl)
                      .AllowAnyHeader()
                      .AllowAnyMethod()
                      .AllowCredentials();
            });
        });

        var app = builder.Build();

        // 5. EF Migrate on startup
        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Database.Migrate();
        }

        // 6. CORS must come before auth middleware
        app.UseCors();

        // 7. Standard API pipeline (Swagger, HTTPS, Auth, Controllers)
        app.UseQuestFlagApiPipeline();

        app.Run();
    }
}
