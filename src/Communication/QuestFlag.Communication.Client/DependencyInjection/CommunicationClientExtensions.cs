using Microsoft.Extensions.DependencyInjection;
using QuestFlag.Communication.Client.Contracts;
using QuestFlag.Communication.Client.Implementations;

namespace QuestFlag.Communication.Client.DependencyInjection;

public static class CommunicationClientExtensions
{
    public static IServiceCollection AddCommunicationClient(this IServiceCollection services, string baseUrl)
    {
        services.AddHttpClient<ICommunicationClient, HttpCommunicationClient>(client =>
        {
            client.BaseAddress = new Uri(baseUrl);
        });

        services.AddHttpClient<IUploadApiService, UploadApiService>(client =>
        {
            client.BaseAddress = new Uri(baseUrl);
        });

        return services;
    }
}
