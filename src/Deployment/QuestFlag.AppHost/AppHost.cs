var builder = DistributedApplication.CreateBuilder(args);

string Compose(string key, string fallback)
    => builder.Configuration[$"ComposeEnv:{key}"]
       ?? Environment.GetEnvironmentVariable(key)
       ?? fallback;

var aspnetCoreEnvironment = Compose("ASPNETCORE_ENVIRONMENT", "Development");

var infraWebAppHttpPort = Compose("INFRA_WEBAPP_HTTP_PORT", "8000");
var infraWebAppHttpsPort = Compose("INFRA_WEBAPP_HTTPS_PORT", "7000");
var infrastructureServicesHttpPort = Compose("INFRASTRUCTURE_SERVICES_HTTP_PORT", "8001");
var infrastructureServicesHttpsPort = Compose("INFRASTRUCTURE_SERVICES_HTTPS_PORT", "7001");
var passportWebAppHttpPort = Compose("PASSPORT_WEBAPP_HTTP_PORT", "8002");
var passportWebAppHttpsPort = Compose("PASSPORT_WEBAPP_HTTPS_PORT", "7002");
var passportAdminWebAppHttpPort = Compose("PASSPORT_ADMINWEBAPP_HTTP_PORT", "8003");
var passportAdminWebAppHttpsPort = Compose("PASSPORT_ADMINWEBAPP_HTTPS_PORT", "7003");
var passportServicesHttpPort = Compose("PASSPORT_SERVICES_HTTP_PORT", "8004");
var passportServicesHttpsPort = Compose("PASSPORT_SERVICES_HTTPS_PORT", "7004");

var urlPassportServices = Compose("URL_PASSPORT_SERVICES", $"https://localhost:{passportServicesHttpsPort}");
var urlInfraServices = Compose("URL_INFRA_SERVICES", $"https://localhost:{infrastructureServicesHttpsPort}");
var urlInfraWebApp = Compose("URL_INFRA_WEBAPP", $"https://localhost:{infraWebAppHttpsPort}");
var urlPassportWebApp = Compose("URL_PASSPORT_WEBAPP", $"https://localhost:{passportWebAppHttpsPort}");
var urlPassportAdminWebApp = Compose("URL_PASSPORT_ADMIN_WEBAPP", $"https://localhost:{passportAdminWebAppHttpsPort}");

var infraServices = builder.AddProject<Projects.QuestFlag_Infrastructure_Services>("service-qf-infra")
    .WithEnvironment("ASPNETCORE_ENVIRONMENT", aspnetCoreEnvironment)
    .WithEnvironment("ASPNETCORE_HTTP_PORTS", infrastructureServicesHttpPort)
    .WithEnvironment("ASPNETCORE_HTTPS_PORTS", infrastructureServicesHttpsPort)
    .WithEnvironment("Storage__MinioAccessKey", Compose("STORAGE_MINIO_ACCESS_KEY", "admin"))
    .WithEnvironment("Storage__MinioSecretKey", Compose("STORAGE_MINIO_SECRET_KEY", "m1n10!$$tr0ngP@ss!"))
    .WithEnvironment("Storage__MinioEndpoint", Compose("STORAGE_MINIO_ENDPOINT", "localhost:9000"))
    .WithEnvironment("Kafka__BootstrapServers", Compose("KAFKA_BOOTSTRAP_SERVERS", "localhost:9092"))
    .WithEnvironment("ConnectionStrings__InfrastructureConnection", Compose("INFRASTRUCTURE_CONNECTION_STRING", "Host=localhost;Port=5432;Database=questflag_infrastructure;Username=postgres;Password=P@ssw0rd!Qf2026!"))
    .WithEnvironment("ServiceUrls__PassportServices", urlPassportServices)
    .WithEnvironment("ServiceUrls__InfraServices", urlInfraServices)
    .WithEnvironment("ServiceUrls__InfraWebApp", urlInfraWebApp)
    .WithEnvironment("ServiceUrls__InfraWebAppHttp", $"http://localhost:{infraWebAppHttpPort}");

var infraWebApp = builder.AddProject<Projects.QuestFlag_Infrastructure_WebApp>("app-qf-infra")
    .WithEnvironment("ASPNETCORE_ENVIRONMENT", aspnetCoreEnvironment)
    .WithEnvironment("ASPNETCORE_HTTP_PORTS", infraWebAppHttpPort)
    .WithEnvironment("ASPNETCORE_HTTPS_PORTS", infraWebAppHttpsPort)
    .WithEnvironment("ServiceUrls__PassportServices", urlPassportServices);

var passportWebApp = builder.AddProject<Projects.QuestFlag_Passport_WebApp>("app-qf-passport")
    .WithEnvironment("ASPNETCORE_ENVIRONMENT", aspnetCoreEnvironment)
    .WithEnvironment("ASPNETCORE_HTTP_PORTS", passportWebAppHttpPort)
    .WithEnvironment("ASPNETCORE_HTTPS_PORTS", passportWebAppHttpsPort);

var passportAdminWebApp = builder.AddProject<Projects.QuestFlag_Passport_AdminWebApp>("app-qf-passport-admin")
    .WithEnvironment("ASPNETCORE_ENVIRONMENT", aspnetCoreEnvironment)
    .WithEnvironment("ASPNETCORE_HTTP_PORTS", passportAdminWebAppHttpPort)
    .WithEnvironment("ASPNETCORE_HTTPS_PORTS", passportAdminWebAppHttpsPort);

var passportServices = builder.AddProject<Projects.QuestFlag_Passport_Services>("questflag-passport-services")
    .WithEnvironment("ASPNETCORE_ENVIRONMENT", aspnetCoreEnvironment)
    .WithEnvironment("ASPNETCORE_HTTP_PORTS", passportServicesHttpPort)
    .WithEnvironment("ASPNETCORE_HTTPS_PORTS", passportServicesHttpsPort)
    .WithEnvironment("ConnectionStrings__PassportConnection", Compose("PASSPORT_CONNECTION_STRING", "Host=localhost;Port=5432;Database=QuestFlag_Passport;Username=postgres;Password=P@ssw0rd!Qf2026!"))
    .WithEnvironment("ServiceUrls__InfraWebApp", urlInfraWebApp)
    .WithEnvironment("ServiceUrls__InfraWebAppHttp", $"http://localhost:{infraWebAppHttpPort}")
    .WithEnvironment("ServiceUrls__PassportWebApp", urlPassportWebApp)
    .WithEnvironment("ServiceUrls__PassportAdminWebApp", urlPassportAdminWebApp)
    .WithEnvironment("OpenIddictApplications__infra-webapp", urlInfraWebApp)
    .WithEnvironment("OpenIddictApplications__passport-webapp", urlPassportWebApp)
    .WithEnvironment("OpenIddictApplications__passport-admin", urlPassportAdminWebApp);

infraServices.WithReference(passportServices);
infraWebApp.WithReference(passportServices);
passportWebApp.WithReference(passportServices);
passportAdminWebApp.WithReference(passportServices);

builder.Build().Run();
