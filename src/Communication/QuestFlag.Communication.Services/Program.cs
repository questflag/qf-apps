using QuestFlag.Communication.Services.DependencyInjection;
using QuestFlag.Infrastructure.ApiCore.StartupExtensions;

namespace QuestFlag.Communication.Services;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add service defaults & Aspire client integrations.
        builder.AddServiceDefaults();

        // Add services to the container.
        builder.Services.AddCommunicationServices(builder.Configuration);

        // Use common API services and authentication defaults
        builder.AddQuestFlagApi();

        var app = builder.Build();

        app.MapDefaultEndpoints();

        // Configure the standard QuestFlag API middleware pipeline
        app.UseQuestFlagApiPipeline(requireAuthorization: false);

        app.Run();
    }
}
