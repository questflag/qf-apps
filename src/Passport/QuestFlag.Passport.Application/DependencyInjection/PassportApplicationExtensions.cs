using System.Reflection;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace QuestFlag.Passport.Application.DependencyInjection;

public static class PassportApplicationExtensions
{
    public static IServiceCollection AddPassportApplication(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();
        
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assembly));
        services.AddValidatorsFromAssembly(assembly);
        
        return services;
    }
}
