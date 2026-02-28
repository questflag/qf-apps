using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Minio;
using Google.Cloud.Storage.V1;
using Google.Apis.Auth.OAuth2;
using QuestFlag.Infrastructure.Core.Data;
using QuestFlag.Infrastructure.Core.Messaging;
using QuestFlag.Infrastructure.Core.Repositories;
using QuestFlag.Infrastructure.Core.Storage;
using QuestFlag.Infrastructure.Domain.Interfaces;

namespace QuestFlag.Infrastructure.Core.DependencyInjection;

public static class CoreServiceExtensions
{
    public static IServiceCollection AddInfrastructureCore(this IServiceCollection services, IConfiguration configuration)
    {
        // 1. Data Access
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("InfrastructureConnection")));

        services.AddScoped<IUploadRepository, UploadRepository>();

        // 2. Storage Setup
        services.Configure<StorageSettings>(configuration.GetSection(StorageSettings.SectionName));
        
        // Register Minio
        var minioSettings = configuration.GetSection(StorageSettings.SectionName).Get<StorageSettings>() ?? new StorageSettings();
        services.AddMinio(opt => opt
            .WithEndpoint(minioSettings.MinioEndpoint)
            .WithCredentials(minioSettings.MinioAccessKey, minioSettings.MinioSecretKey)
            .WithSSL(minioSettings.MinioUseSSL)
            .Build());
            
        services.AddScoped<MinioStorageService>();

        // Register GCP
        if (!string.IsNullOrEmpty(minioSettings.GcpCredentialsJson))
        {
            var credential = GoogleCredential.FromJson(minioSettings.GcpCredentialsJson);
            services.AddSingleton(StorageClient.Create(credential));
            services.AddSingleton(UrlSigner.FromCredential(credential));
            services.AddScoped<GcpCloudStorageService>();
        }

        // Factory
        services.AddScoped<IStorageService>(StorageServiceFactory.Create);

        // 3. Messaging (Kafka)
        services.Configure<KafkaSettings>(configuration.GetSection(KafkaSettings.SectionName));
        services.AddSingleton<IUploadEventPublisher, KafkaUploadEventPublisher>();
        
        // Background Service for Kafka consuming
        services.AddHostedService<UploadCompletedConsumer>();

        return services;
    }
}
