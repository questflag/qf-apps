using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenIddict.Validation;
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
    /// Registers the common QuestFlag API surface including controllers and Swagger.
    /// Does NOT register authentication or authorization.
    /// </summary>
    public static WebApplicationBuilder AddQuestFlagApiBase(this WebApplicationBuilder builder,
        string corsPolicyName = "DefaultClients",
        string[]? corsConfigKeys = null)
    {
        // Controllers + Swagger + API explorer
        builder.Services.AddQuestFlagApiServices();

        // Optional CORS registration
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

    /// <summary>
    /// Registers OpenIddict validation-based authentication and authorization policies.
    /// </summary>
    public static WebApplicationBuilder AddQuestFlagAuthentication(this WebApplicationBuilder builder,
        bool useLocalServer = false,
        Action<AuthorizationOptions>? configureAuthorization = null)
    {
        builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme;
        });

        builder.Services.AddOpenIddict()
            .AddValidation(options =>
            {
                var authority = builder.Configuration["QuestFlag:Passport:Authority"] 
                    ?? builder.Configuration["ServiceUrls:PassportServices"]
                    ?? builder.Configuration["Oidc:Authority"];

                if (!string.IsNullOrEmpty(authority))
                {
                    options.SetIssuer(authority);
                    options.UseSystemNetHttp();
                }

                if (useLocalServer)
                {
                    options.UseLocalServer();
                }

                options.UseAspNetCore();
            });

        builder.Services.AddAuthorization(options =>
        {
            configureAuthorization?.Invoke(options);
        });

        return builder;
    }

    /// <summary>
    /// Registers everything: controllers, Swagger, CORS, Authentication and Authorization.
    /// </summary>
    public static WebApplicationBuilder AddQuestFlagApi(this WebApplicationBuilder builder,
        string corsPolicyName = "DefaultClients",
        string[]? corsConfigKeys = null,
        bool useLocalServer = false,
        Action<AuthorizationOptions>? configureAuthorization = null)
    {
        builder.AddQuestFlagApiBase(corsPolicyName, corsConfigKeys);
        builder.AddQuestFlagAuthentication(useLocalServer, configureAuthorization);

        return builder;
    }
}
