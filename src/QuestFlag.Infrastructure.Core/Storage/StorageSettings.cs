namespace QuestFlag.Infrastructure.Core.Storage;

public class StorageSettings
{
    public const string SectionName = "Storage";

    public string Provider { get; set; } = "Minio"; // "Minio" or "Gcp"
    
    // Minio Settings
    public string MinioEndpoint { get; set; } = "localhost:9000";
    public string MinioAccessKey { get; set; } = "minioadmin";
    public string MinioSecretKey { get; set; } = "minioadmin";
    public bool MinioUseSSL { get; set; } = false;

    // GCP Settings
    public string GcpProjectId { get; set; } = string.Empty;
    public string GcpCredentialsJson { get; set; } = string.Empty;
}
