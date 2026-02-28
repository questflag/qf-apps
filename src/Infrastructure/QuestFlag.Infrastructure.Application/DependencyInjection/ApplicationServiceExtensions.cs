using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace QuestFlag.Infrastructure.Application.DependencyInjection;

public static class ApplicationServiceExtensions
{
    public static IServiceCollection AddInfrastructureApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
        
        return services;
    }
}
