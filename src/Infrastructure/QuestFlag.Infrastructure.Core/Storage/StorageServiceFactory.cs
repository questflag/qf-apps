using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using QuestFlag.Infrastructure.Domain.Interfaces;

namespace QuestFlag.Infrastructure.Core.Storage;

public class StorageServiceFactory
{
    public static IStorageService Create(IServiceProvider serviceProvider)
    {
        var settings = serviceProvider.GetRequiredService<IOptions<StorageSettings>>().Value;

        if (settings.Provider.Equals("Minio", StringComparison.OrdinalIgnoreCase))
        {
            return serviceProvider.GetRequiredService<MinioStorageService>();
        }
        else if (settings.Provider.Equals("Gcp", StringComparison.OrdinalIgnoreCase))
        {
            return serviceProvider.GetRequiredService<GcpCloudStorageService>();
        }

        throw new InvalidOperationException($"Unsupported storage provider: {settings.Provider}");
    }
}
