using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;
using QuestFlag.Communication.Application.DependencyInjection;
using QuestFlag.Communication.Core.Messaging.Kafka;
using QuestFlag.Communication.Core.Persistence.MongoDB;
using QuestFlag.Communication.Core.Persistence.PostgreSQL;
using QuestFlag.Communication.Core.Providers;
using QuestFlag.Communication.Core.VectorDB;
using QuestFlag.Communication.Domain.Interfaces;

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
