using Google.Cloud.Storage.V1;
using Microsoft.Extensions.Logging;
using QuestFlag.Infrastructure.Domain.Contracts;

namespace QuestFlag.Infrastructure.Core.Implementations.Storage;

public class GcpCloudStorageService : IStorageService
{
    private readonly StorageClient _storageClient;
    private readonly UrlSigner _urlSigner;
    private readonly ILogger<GcpCloudStorageService> _logger;

    public string ProviderName => "Gcp";

    public GcpCloudStorageService(StorageClient storageClient, UrlSigner urlSigner, ILogger<GcpCloudStorageService> logger)
    {
        _storageClient = storageClient;
        _urlSigner = urlSigner;
        _logger = logger;
    }

    public async Task UploadStreamAsync(string bucketName, string objectKey, Stream stream, string contentType, CancellationToken ct = default)
    {
        try
        {
            await _storageClient.UploadObjectAsync(bucketName, objectKey, contentType, stream, cancellationToken: ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading to GCP. Bucket: {Bucket}, Key: {Key}", bucketName, objectKey);
            throw;
        }
    }

    public async Task DeleteObjectAsync(string bucketName, string objectKey, CancellationToken ct = default)
    {
        try
        {
            await _storageClient.DeleteObjectAsync(bucketName, objectKey, cancellationToken: ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting from GCP. Bucket: {Bucket}, Key: {Key}", bucketName, objectKey);
            throw;
        }
    }

    public Task<string> GetSignedDownloadUrlAsync(string bucketName, string objectKey, TimeSpan expiration, CancellationToken ct = default)
    {
        var url = _urlSigner.Sign(bucketName, objectKey, expiration, HttpMethod.Get);
        return Task.FromResult(url);
    }
}
