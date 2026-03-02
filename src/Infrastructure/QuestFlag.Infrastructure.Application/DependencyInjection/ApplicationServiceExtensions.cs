using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using QuestFlag.Infrastructure.Application.Data;
using QuestFlag.Infrastructure.Application.Messaging;
using QuestFlag.Infrastructure.Application.Repositories;
using QuestFlag.Infrastructure.Domain.Interfaces;
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

        services.AddScoped<IUploadRepository, UploadRepository>();

        // 2. Messaging (Kafka)
        services.Configure<KafkaSettings>(configuration.GetSection(KafkaSettings.SectionName));
        services.AddSingleton<IUploadEventPublisher, KafkaUploadEventPublisher>();
        
        // Background Service for Kafka consuming
        services.AddHostedService<UploadCompletedConsumer>();
        
        return services;
    }
}
