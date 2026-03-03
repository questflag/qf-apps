namespace QuestFlag.Infrastructure.Domain.Contracts;

public interface IStorageService
{
    // E.g., minio or gcp
    string ProviderName { get; }

    Task UploadStreamAsync(string bucketName, string objectKey, Stream stream, string contentType, CancellationToken ct = default);
    Task DeleteObjectAsync(string bucketName, string objectKey, CancellationToken ct = default);
    
    // Generate signed download URL with expiration
    Task<string> GetSignedDownloadUrlAsync(string bucketName, string objectKey, TimeSpan expiration, CancellationToken ct = default);
}
