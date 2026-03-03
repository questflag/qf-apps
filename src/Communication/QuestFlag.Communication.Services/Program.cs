using QuestFlag.Communication.Services.DependencyInjection;
using QuestFlag.Infrastructure.ApiCore.StartupExtensions;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddCommunicationServices(builder.Configuration);

// Use common API services (controllers, endpoint explorer, Swagger)
builder.Services.AddQuestFlagApiServices();

var app = builder.Build();

app.MapDefaultEndpoints();

// Configure the standard QuestFlag API middleware pipeline
app.UseQuestFlagApiPipeline(requireAuthorization: false);

app.Run();
