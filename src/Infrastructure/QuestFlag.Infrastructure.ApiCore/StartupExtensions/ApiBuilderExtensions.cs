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
    public static WebApplication UseQuestFlagApiPipeline(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();

        return app;
    }
}
