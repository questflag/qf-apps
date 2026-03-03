using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using QuestFlag.Infrastructure.Application.Data;
using QuestFlag.Communication.Shared.Messaging;
using System.Reflection;

namespace QuestFlag.Infrastructure.Application.DependencyInjection;

public static class ApplicationServiceExtensions
{
    public static IServiceCollection AddInfrastructureApplication(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

        // 1. Data Access
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("InfrastructureConnection")));

        // 2. Messaging (Kafka)
        services.Configure<KafkaSettings>(configuration.GetSection(KafkaSettings.SectionName));
        
        return services;
    }
}
