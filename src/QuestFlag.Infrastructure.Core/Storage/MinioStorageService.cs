using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Minio;
using Minio.DataModel.Args;
using QuestFlag.Infrastructure.Domain.Interfaces;

namespace QuestFlag.Infrastructure.Core.Storage;

public class MinioStorageService : IStorageService
{
    private readonly IMinioClient _minioClient;
    private readonly ILogger<MinioStorageService> _logger;

    public string ProviderName => "Minio";

    public MinioStorageService(IMinioClient minioClient, ILogger<MinioStorageService> logger)
    {
        _minioClient = minioClient;
        _logger = logger;
    }

    public async Task UploadStreamAsync(string bucketName, string objectKey, Stream stream, string contentType, CancellationToken ct = default)
    {
        try
        {
            // Ensure bucket exists
            bool found = await _minioClient.BucketExistsAsync(new BucketExistsArgs().WithBucket(bucketName), ct);
            if (!found)
            {
                await _minioClient.MakeBucketAsync(new MakeBucketArgs().WithBucket(bucketName), ct);
            }

            // Upload object
            var putObjectArgs = new PutObjectArgs()
                .WithBucket(bucketName)
                .WithObject(objectKey)
                .WithStreamData(stream)
                .WithObjectSize(stream.Length)
                .WithContentType(contentType);

            await _minioClient.PutObjectAsync(putObjectArgs, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading to Minio. Bucket: {Bucket}, Key: {Key}", bucketName, objectKey);
            throw;
        }
    }

    public async Task DeleteObjectAsync(string bucketName, string objectKey, CancellationToken ct = default)
    {
        try
        {
            var removeObjectArgs = new RemoveObjectArgs()
                .WithBucket(bucketName)
                .WithObject(objectKey);

            await _minioClient.RemoveObjectAsync(removeObjectArgs, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting from Minio. Bucket: {Bucket}, Key: {Key}", bucketName, objectKey);
            throw;
        }
    }

    public async Task<string> GetSignedDownloadUrlAsync(string bucketName, string objectKey, TimeSpan expiration, CancellationToken ct = default)
    {
        var args = new PresignedGetObjectArgs()
            .WithBucket(bucketName)
            .WithObject(objectKey)
            .WithExpiry((int)expiration.TotalSeconds);

        return await _minioClient.PresignedGetObjectAsync(args);
    }
}
