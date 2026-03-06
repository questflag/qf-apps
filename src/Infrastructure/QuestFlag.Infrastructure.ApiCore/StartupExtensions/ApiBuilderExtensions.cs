using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenIddict.Validation.AspNetCore;

namespace QuestFlag.Infrastructure.ApiCore.StartupExtensions;

public static class ApiBuilderExtensions
{
    /// <summary>
    /// Registers common API services: controllers, API explorer, and Swagger generator.
    /// Call this in Program.cs instead of the three individual calls.
    /// </summary>
    public static IServiceCollection AddQuestFlagApiServices(this IServiceCollection services)
    {
        services.AddControllers();
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();

        return services;
    }

    /// <summary>
    /// Configures the standard QuestFlag API middleware pipeline:
    /// Swagger UI (dev only), HTTPS redirection, authentication, authorization, and controller mapping.
    /// </summary>
    public static WebApplication UseQuestFlagApiPipeline(this WebApplication app, bool requireAuthorization = false)
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger(options =>
            {
                options.RouteTemplate = "docs/{documentName}/swagger.json";
            });

            app.UseSwaggerUI(options =>
            {
                options.RoutePrefix = "docs";
                options.SwaggerEndpoint("v1/swagger.json", "V1");
            });
        }

        app.UseHttpsRedirection();
        app.UseAuthentication();
        app.UseAuthorization();

        var endpoints = app.MapControllers();
        if (requireAuthorization)
        {
            endpoints.RequireAuthorization();
        }

        return app;
    }

    /// <summary>
    /// Registers the common QuestFlag API surface including controllers, Swagger and the
    /// OpenIddict validation-based authentication defaults. Optionally registers a CORS
    /// policy by reading origin URLs from configuration keys and allows the caller to
    /// configure authorization policies.
    /// </summary>
    public static WebApplicationBuilder AddQuestFlagApi(this WebApplicationBuilder builder,
        string corsPolicyName = "DefaultClients",
        string[]? corsConfigKeys = null,
        Action<AuthorizationOptions>? configureAuthorization = null)
    {
        // Controllers + Swagger + API explorer
        builder.Services.AddQuestFlagApiServices();

        // Authentication defaults: use OpenIddict validation scheme so services don't need
        // to repeat these few lines in each Program.cs.
        // We also register the OpenIddict validation handler here to ensure it's used by all APIs.
        builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme;
        });

        builder.Services.AddOpenIddict()
            .AddValidation(options =>
            {
                // Note: The authority must match the Passport Services URL.
                // We attempt to read it from configuration.
                var authority = builder.Configuration["QuestFlag:Passport:Authority"] 
                    ?? builder.Configuration["ServiceUrls:PassportServices"];

                if (!string.IsNullOrEmpty(authority))
                {
                    options.SetIssuer(authority);
                    options.UseSystemNetHttp();
                }

                options.UseLocalServer();
                options.UseAspNetCore();
            });

        // Authorization - allow the caller to add policies specific to the service.
        builder.Services.AddAuthorization(options =>
        {
            configureAuthorization?.Invoke(options);
        });

        // Optional CORS registration - read origins from provided configuration keys
        // (e.g. "ServiceUrls:InfraWebApp"). If no keys provided, skip CORS registration.
        if (corsConfigKeys != null && corsConfigKeys.Length > 0)
        {
            var origins = new List<string>();
            foreach (var key in corsConfigKeys)
            {
                var val = builder.Configuration[key];
                if (!string.IsNullOrWhiteSpace(val)) origins.Add(val!);
            }

            if (origins.Count > 0)
            {
                builder.Services.AddCors(options =>
                {
                    options.AddPolicy(corsPolicyName, b => b.WithOrigins(origins.ToArray())
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials()
                        .WithExposedHeaders("Token-Expired"));
                });
            }
        }

        return builder;
    }
}
