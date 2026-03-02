using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace QuestFlag.Communication.Application.DependencyInjection;

public static class CommunicationApplicationExtensions
{
    public static IServiceCollection AddCommunicationApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
        return services;
    }
}
