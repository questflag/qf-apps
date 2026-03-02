using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

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
}
