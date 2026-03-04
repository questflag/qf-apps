var builder = DistributedApplication.CreateBuilder(args);

string Compose(string key, string fallback)
    => builder.Configuration[$"ComposeEnv:{key}"]
       ?? Environment.GetEnvironmentVariable(key)
       ?? fallback;

var aspnetCoreEnvironment = Compose("ASPNETCORE_ENVIRONMENT", "Development");

// Configuration for ports
var infraWebAppHttpPort = int.Parse(Compose("INFRA_WEBAPP_HTTP_PORT", "8000"));
var infraWebAppHttpsPort = int.Parse(Compose("INFRA_WEBAPP_HTTPS_PORT", "7000"));

var passportServicesHttpPort = int.Parse(Compose("PASSPORT_SERVICES_HTTP_PORT", "8004"));
var passportServicesHttpsPort = int.Parse(Compose("PASSPORT_SERVICES_HTTPS_PORT", "7004"));
var commServicesHttpPort = int.Parse(Compose("COMM_SERVICES_HTTP_PORT", "8005"));
var commServicesHttpsPort = int.Parse(Compose("COMM_SERVICES_HTTPS_PORT", "7005"));
var passportWebAppHttpPort = int.Parse(Compose("PASSPORT_WEBAPP_HTTP_PORT", "8002"));
var passportWebAppHttpsPort = int.Parse(Compose("PASSPORT_WEBAPP_HTTPS_PORT", "7002"));

// 1. Passport Services (Identity Provider)
var passportServices = builder.AddProject<Projects.QuestFlag_Passport_Services>("services-passport", launchProfileName: null)
    .WithHttpEndpoint(port: passportServicesHttpPort, targetPort: passportServicesHttpPort, name: "http", isProxied: false)
    .WithHttpsEndpoint(port: passportServicesHttpsPort, targetPort: passportServicesHttpsPort, name: "https", isProxied: false)
    .WithEnvironment("ASPNETCORE_ENVIRONMENT", aspnetCoreEnvironment)
    .WithEnvironment("ASPNETCORE_PREVENTHOSTINGSTARTUP", "true")
    .WithEnvironment("ConnectionStrings__PassportConnection", Compose("PASSPORT_CONNECTION_STRING", "Host=localhost;Port=15432;Database=QuestFlag_Passport;Username=postgres;Password=P@ssw0rd!Qf2026!"))
    .WithEnvironment("ServiceUrls__InfraWebApp", $"https://localhost:{infraWebAppHttpsPort.ToString()}")
    .WithEnvironment("ServiceUrls__InfraWebAppHttp", $"http://localhost:{infraWebAppHttpPort.ToString()}")
    .WithEnvironment("ServiceUrls__PassportWebApp", $"https://localhost:{passportWebAppHttpsPort.ToString()}")
    .WithEnvironment("OpenIddictApplications__infra-webapp", $"https://localhost:{infraWebAppHttpsPort.ToString()}");


// 1.1 Passport Web App (SSO Portal)
var passportWebApp = builder.AddProject<Projects.QuestFlag_Passport_WebApp>("app-passport", launchProfileName: null)
    .WithHttpEndpoint(port: passportWebAppHttpPort, targetPort: passportWebAppHttpPort, name: "http", isProxied: false)
    .WithHttpsEndpoint(port: passportWebAppHttpsPort, targetPort: passportWebAppHttpsPort, name: "https", isProxied: false)
    .WithEnvironment("ASPNETCORE_ENVIRONMENT", aspnetCoreEnvironment)
    .WithEnvironment("ASPNETCORE_PREVENTHOSTINGSTARTUP", "true")
    .WithEnvironment("ServiceUrls__PassportServices", passportServices.GetEndpoint("https").Property(EndpointProperty.Url))
    .WithReference(passportServices);


// 4. Communication Services
var commServices = builder.AddProject<Projects.QuestFlag_Communication_Services>("services-comm", launchProfileName: null)
    .WithHttpEndpoint(port: commServicesHttpPort, targetPort: commServicesHttpPort, name: "http", isProxied: false)
    .WithHttpsEndpoint(port: commServicesHttpsPort, targetPort: commServicesHttpsPort, name: "https", isProxied: false)
    .WithEnvironment("ASPNETCORE_ENVIRONMENT", aspnetCoreEnvironment)
    .WithEnvironment("ASPNETCORE_PREVENTHOSTINGSTARTUP", "true")
    .WithEnvironment("ConnectionStrings__Postgres", Compose("COMM_POSTGRES_CONNECTION_STRING", "Host=localhost;Port=15432;Database=QuestFlag_Comm;Username=postgres;Password=P@ssw0rd!Qf2026!"))
    .WithEnvironment("ConnectionStrings__Mongo", Compose("COMM_MONGO_CONNECTION_STRING", "mongodb://localhost:27017"))
    .WithEnvironment("Kafka__BootstrapServers", Compose("KAFKA_BOOTSTRAP_SERVERS", "localhost:19092"))
    .WithEnvironment("Kafka__TopicName", Compose("KAFKA_TOPIC_NAME", "qf-communication-tasks"))
    .WithEnvironment("Kafka__GroupId", Compose("KAFKA_GROUP_ID", "qf-orchestrator-group"))
    .WithEnvironment("ServiceUrls__PassportServices", passportServices.GetEndpoint("https").Property(EndpointProperty.Url))
    .WithReference(passportServices);


// 2. Demo Web Application
var infraWebApp = builder.AddProject<Projects.QuestFlag_Demo_WebApp>("app-demo", launchProfileName: null)
    .WithHttpEndpoint(port: infraWebAppHttpPort, targetPort: infraWebAppHttpPort, name: "http", isProxied: false)
    .WithHttpsEndpoint(port: infraWebAppHttpsPort, targetPort: infraWebAppHttpsPort, name: "https", isProxied: false)
    .WithEnvironment("ASPNETCORE_ENVIRONMENT", aspnetCoreEnvironment)
    .WithEnvironment("ASPNETCORE_PREVENTHOSTINGSTARTUP", "true")
    .WithEnvironment("ServiceUrls__PassportServices", passportServices.GetEndpoint("https").Property(EndpointProperty.Url))
    .WithEnvironment("ServiceUrls__InfraServices", commServices.GetEndpoint("http").Property(EndpointProperty.Url))
    .WithReference(passportServices)
    .WithReference(commServices);

builder.Build().Run();
