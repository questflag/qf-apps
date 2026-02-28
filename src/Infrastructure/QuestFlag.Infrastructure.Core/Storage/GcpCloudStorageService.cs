using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Google.Cloud.Storage.V1;
using Microsoft.Extensions.Logging;
using QuestFlag.Infrastructure.Domain.Interfaces;

namespace QuestFlag.Infrastructure.Core.Storage;

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
            // Note: GCP buckets have global uniqueness constraints, 
            // the system needs to prefix the bucket name if necessary, handling here assumes bucketName is already globally unique
            // e.g. "qf-app-" + tenantSlug
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
        // Require UrlSigner mapped by Dependency Injection (using service account keys)
        var url = _urlSigner.Sign(bucketName, objectKey, expiration, HttpMethod.Get);
        return Task.FromResult(url);
    }
}
