using System.IO;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Minio;
using Google.Cloud.Storage.V1;
using Google.Apis.Auth.OAuth2;
using QuestFlag.Infrastructure.Domain.Models;
using QuestFlag.Infrastructure.Domain.Contracts;
using QuestFlag.Infrastructure.Core.Implementations.Storage;

namespace QuestFlag.Infrastructure.Core.DependencyInjection;

public static class CoreServiceExtensions
{
    public static IServiceCollection AddInfrastructureCore(this IServiceCollection services, IConfiguration configuration)
    {


        // 2. Storage Setup
        var storageSection = configuration.GetSection(StorageSettings.SectionName);
        services.Configure<StorageSettings>(storageSection);
        
        // Register Minio
        var minioSettings = storageSection.Get<StorageSettings>() ?? throw new InvalidOperationException("Storage settings are not configured.");
        services.AddMinio(opt => opt
            .WithEndpoint(minioSettings.MinioEndpoint)
            .WithCredentials(minioSettings.MinioAccessKey, minioSettings.MinioSecretKey)
            .WithSSL(minioSettings.MinioUseSSL)
            .Build());
            
        services.AddScoped<MinioStorageService>();

        // Register GCP
        if (!string.IsNullOrEmpty(minioSettings.GcpCredentialsJson))
        {
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(minioSettings.GcpCredentialsJson));
#pragma warning disable CS0618 // GoogleCredential.FromStream is obsolete
            var credential = GoogleCredential.FromStream(stream)
                .CreateScoped("https://www.googleapis.com/auth/devstorage.full_control");
#pragma warning restore CS0618
            services.AddSingleton(StorageClient.Create(credential));
            services.AddSingleton(UrlSigner.FromCredential(credential));
            services.AddScoped<GcpCloudStorageService>();
        }

        // Factory
        services.AddScoped<IStorageService>(StorageServiceFactory.Create);



        return services;
    }
}
