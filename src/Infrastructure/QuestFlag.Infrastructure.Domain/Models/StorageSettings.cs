namespace QuestFlag.Infrastructure.Domain.Models;

public class StorageSettings
{
    public const string SectionName = "Storage";

    public string Provider { get; set; } = string.Empty; // "Minio" or "Gcp"
    
    // Minio Settings
    public string MinioEndpoint { get; set; } = string.Empty;
    public string MinioAccessKey { get; set; } = string.Empty;
    public string MinioSecretKey { get; set; } = string.Empty;
    public bool MinioUseSSL { get; set; }

    // GCP Settings
    public string GcpProjectId { get; set; } = string.Empty;
    public string GcpCredentialsJson { get; set; } = string.Empty;
}
