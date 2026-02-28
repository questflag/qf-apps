using System;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenIddict.Validation.AspNetCore;
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

        // 1. Add layers DI
        builder.Services.AddInfrastructureApplication();
        builder.Services.AddInfrastructureCore(builder.Configuration);

        // Add HttpClient for the Kafka consumer downstream call
        builder.Services.AddHttpClient();

        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();

        // 2. Swagger Configuration
        builder.Services.AddSwaggerGen();

        // 3. Authentication & Authorization (Consuming Passport JWT via Introspection)
        builder.Services.AddAuthentication(OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme);

        builder.Services.AddOpenIddict()
            .AddValidation(options =>
            {
                // The Passport Server's address
                options.SetIssuer("https://localhost:7002");
                options.AddAudiences("infra-api");

                // We want to validate against the Passport Introspection endpoint
                options.UseIntrospection()
                       .SetClientId("infra-api")
                       .SetClientSecret("infra-api-secret"); // Should come from config in production

                // Register the System.Net.Http integration.
                options.UseSystemNetHttp();

                // Register the ASP.NET Core host.
                options.UseAspNetCore();
            });

        builder.Services.AddAuthorization(options =>
        {
            // TenantAdmin Policy
            options.AddPolicy("TenantAdmin", policy =>
                policy.RequireClaim("role", "tenant_admin"));
        });

        // 4. CORS
        builder.Services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
            {
                policy.WithOrigins("https://localhost:7000", "http://localhost:5000", "https://localhost:7002") // Allow Blazor WebApp
                      .AllowAnyHeader()
                      .AllowAnyMethod()
                      .AllowCredentials(); // needed for some JS clients if they pack cookies
            });
        });

        var app = builder.Build();

        // 5. EF Migrate on startup
        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Database.Migrate();
        }

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();
        app.UseCors();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();

        app.Run();
    }
}
