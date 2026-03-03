using QuestFlag.Communication.Application.Implementations.Repositories;
using QuestFlag.Communication.Domain.Events;
using QuestFlag.Communication.Shared.Messaging;
using QuestFlag.Communication.Shared.DTOs;

namespace QuestFlag.Communication.Services.DependencyInjection;

public static class CommunicationServicesExtensions
{
    public static IServiceCollection AddCommunicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        // 1. PostgreSQL
        services.AddDbContext<CommunicationDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("Postgres")));

        // 2. MongoDB
        services.AddSingleton<IMongoClient>(new MongoClient(configuration.GetConnectionString("Mongo")));
        services.AddScoped(sp => sp.GetRequiredService<IMongoClient>().GetDatabase(configuration["Mongo:DatabaseName"] ?? "QuestFlag_Comm"));

        // 3. Kafka
        services.AddSingleton<KafkaProducer>();
        services.AddHostedService<KafkaConsumer>();
        
        // --- Upload Support ---
        services.Configure<KafkaSettings>(configuration.GetSection("Kafka"));
        services.AddScoped<IUploadRepository, UploadRepository>();
        services.AddSingleton<IUploadEventPublisher, KafkaUploadEventPublisher>();
        services.AddHostedService<UploadCompletedConsumer>();
        // ----------------------

        // 4. Repositories & Services
        services.AddScoped<ICommunicationLogRepository, CommunicationLogRepository>();
        services.AddScoped<IConversationThreadRepository, ConversationThreadRepository>();
        services.AddScoped<IProviderResolver, ProviderResolver>();
        services.AddSingleton<QdrantConversationVectorStore>();

        // 5. Application Layer
        services.AddCommunicationApplication();

        return services;
    }
}
