using Microsoft.Extensions.DependencyInjection;

namespace QuestFlag.Communication.Client.DependencyInjection;

public static class CommunicationClientExtensions
{
    public static IServiceCollection AddCommunicationClient(this IServiceCollection services, string baseUrl)
    {
        services.AddHttpClient<ICommunicationClient, HttpCommunicationClient>(client =>
        {
            client.BaseAddress = new Uri(baseUrl);
        });
        return services;
    }
}
