var builder = DistributedApplication.CreateBuilder(args);

string Compose(string key, string fallback)
    => builder.Configuration[$"ComposeEnv:{key}"]
       ?? Environment.GetEnvironmentVariable(key)
       ?? fallback;

var aspnetCoreEnvironment = Compose("ASPNETCORE_ENVIRONMENT", "Development");

// Configuration for ports
var infraWebAppHttpPort = int.Parse(Compose("INFRA_WEBAPP_HTTP_PORT", "8000"));
var infraWebAppHttpsPort = int.Parse(Compose("INFRA_WEBAPP_HTTPS_PORT", "7000"));
var infraServicesHttpPort = int.Parse(Compose("INFRASTRUCTURE_SERVICES_HTTP_PORT", "8001"));
var infraServicesHttpsPort = int.Parse(Compose("INFRASTRUCTURE_SERVICES_HTTPS_PORT", "7001"));
var passportWebAppHttpPort = int.Parse(Compose("PASSPORT_WEBAPP_HTTP_PORT", "8002"));
var passportWebAppHttpsPort = int.Parse(Compose("PASSPORT_WEBAPP_HTTPS_PORT", "7002"));
var passportAdminWebAppHttpPort = int.Parse(Compose("PASSPORT_ADMINWEBAPP_HTTP_PORT", "8003"));
var passportAdminWebAppHttpsPort = int.Parse(Compose("PASSPORT_ADMINWEBAPP_HTTPS_PORT", "7003"));
var passportServicesHttpPort = int.Parse(Compose("PASSPORT_SERVICES_HTTP_PORT", "8004"));
var passportServicesHttpsPort = int.Parse(Compose("PASSPORT_SERVICES_HTTPS_PORT", "7004"));

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
    .WithEnvironment("ServiceUrls__PassportAdminWebApp", $"https://localhost:{passportAdminWebAppHttpsPort.ToString()}")
    .WithEnvironment("OpenIddictApplications__infra-webapp", $"https://localhost:{infraWebAppHttpsPort.ToString()}")
    .WithEnvironment("OpenIddictApplications__passport-webapp", $"https://localhost:{passportWebAppHttpsPort.ToString()}")
    .WithEnvironment("OpenIddictApplications__passport-admin", $"https://localhost:{passportAdminWebAppHttpsPort.ToString()}");

// 2. Infrastructure Services (Backend API)
var infraServices = builder.AddProject<Projects.QuestFlag_Infrastructure_Services>("services-infra", launchProfileName: null)
    .WithHttpEndpoint(port: infraServicesHttpPort, targetPort: infraServicesHttpPort, name: "http", isProxied: false)
    .WithHttpsEndpoint(port: infraServicesHttpsPort, targetPort: infraServicesHttpsPort, name: "https", isProxied: false)
    .WithEnvironment("ASPNETCORE_ENVIRONMENT", aspnetCoreEnvironment)
    .WithEnvironment("ASPNETCORE_PREVENTHOSTINGSTARTUP", "true")
    .WithEnvironment("Storage__MinioAccessKey", Compose("STORAGE_MINIO_ACCESS_KEY", "admin"))
    .WithEnvironment("Storage__MinioSecretKey", Compose("STORAGE_MINIO_SECRET_KEY", "m1n10!$$tr0ngP@ss!"))
    .WithEnvironment("Storage__MinioEndpoint", Compose("STORAGE_MINIO_ENDPOINT", "localhost:9000"))
    .WithEnvironment("Kafka__BootstrapServers", Compose("KAFKA_BOOTSTRAP_SERVERS", "localhost:19092"))
    .WithEnvironment("ConnectionStrings__InfrastructureConnection", Compose("INFRASTRUCTURE_CONNECTION_STRING", "Host=localhost;Port=15432;Database=questflag_infrastructure;Username=postgres;Password=P@ssw0rd!Qf2026!"))
    .WithEnvironment("ServiceUrls__PassportServices", passportServices.GetEndpoint("https").Property(EndpointProperty.Url))
    .WithEnvironment("ServiceUrls__InfraServices", $"https://localhost:{infraServicesHttpsPort.ToString()}")
    .WithEnvironment("ServiceUrls__InfraWebApp", $"https://localhost:{infraWebAppHttpsPort.ToString()}")
    .WithEnvironment("ServiceUrls__InfraWebAppHttp", $"http://localhost:{infraWebAppHttpPort.ToString()}")
    .WithReference(passportServices);

// 3. Web Applications
var infraWebApp = builder.AddProject<Projects.QuestFlag_Infrastructure_WebApp>("app-infra", launchProfileName: null)
    .WithHttpEndpoint(port: infraWebAppHttpPort, targetPort: infraWebAppHttpPort, name: "http", isProxied: false)
    .WithHttpsEndpoint(port: infraWebAppHttpsPort, targetPort: infraWebAppHttpsPort, name: "https", isProxied: false)
    .WithEnvironment("ASPNETCORE_ENVIRONMENT", aspnetCoreEnvironment)
    .WithEnvironment("ASPNETCORE_PREVENTHOSTINGSTARTUP", "true")
    .WithEnvironment("ServiceUrls__PassportServices", passportServices.GetEndpoint("https").Property(EndpointProperty.Url))
    .WithEnvironment("ServiceUrls__InfraServices", infraServices.GetEndpoint("https").Property(EndpointProperty.Url))
    .WithReference(passportServices);

var passportWebApp = builder.AddProject<Projects.QuestFlag_Passport_WebApp>("app-passport", launchProfileName: null)
    .WithHttpEndpoint(port: passportWebAppHttpPort, targetPort: passportWebAppHttpPort, name: "http", isProxied: false)
    .WithHttpsEndpoint(port: passportWebAppHttpsPort, targetPort: passportWebAppHttpsPort, name: "https", isProxied: false)
    .WithEnvironment("ASPNETCORE_ENVIRONMENT", aspnetCoreEnvironment)
    .WithEnvironment("ASPNETCORE_PREVENTHOSTINGSTARTUP", "true")
    .WithReference(passportServices);

var passportAdminWebApp = builder.AddProject<Projects.QuestFlag_Passport_AdminWebApp>("app-passport-admin", launchProfileName: null)
    .WithHttpEndpoint(port: passportAdminWebAppHttpPort, targetPort: passportAdminWebAppHttpPort, name: "http", isProxied: false)
    .WithHttpsEndpoint(port: passportAdminWebAppHttpsPort, targetPort: passportAdminWebAppHttpsPort, name: "https", isProxied: false)
    .WithEnvironment("ASPNETCORE_ENVIRONMENT", aspnetCoreEnvironment)
    .WithEnvironment("ASPNETCORE_PREVENTHOSTINGSTARTUP", "true")
    .WithReference(passportServices);

builder.Build().Run();
