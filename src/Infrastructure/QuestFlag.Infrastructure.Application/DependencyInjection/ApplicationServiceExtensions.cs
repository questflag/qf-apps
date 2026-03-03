using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
        // 2. Messaging (Kafka)
        services.Configure<KafkaSettings>(configuration.GetSection(KafkaSettings.SectionName));
        
        return services;
    }
}
