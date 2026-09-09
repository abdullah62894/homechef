using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;
using HomeChef.Application.Features.Images;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HomeChef.Infrastructure.Storage;

/// <summary>
/// Persists images to Neon S3-compatible object storage.
/// </summary>
public sealed class NeonS3ImageStorage : IImageStorage
{
    private readonly IAmazonS3 _s3Client;
    private readonly NeonStorageOptions _storageOptions;
    private readonly ImagesOptions _imagesOptions;
    private readonly ILogger<NeonS3ImageStorage> _logger;

    public NeonS3ImageStorage(
        IAmazonS3 s3Client,
        IOptions<NeonStorageOptions> storageOptions,
        IOptions<ImagesOptions> imagesOptions,
        ILogger<NeonS3ImageStorage> logger)
    {
        _s3Client = s3Client;
        _storageOptions = storageOptions.Value;
        _imagesOptions = imagesOptions.Value;
        _logger = logger;
    }

    public async Task SaveAsync(string relativePath, Stream content, CancellationToken cancellationToken = default)
    {
        var key = NormalizeKey(relativePath);

        var putRequest = new PutObjectRequest
        {
            BucketName = _storageOptions.BucketName,
            Key = key,
            InputStream = content,
            ContentType = "image/webp",
            DisablePayloadSigning = true
        };

        _logger.LogInformation("Uploading image to Neon S3 bucket {Bucket} with key {Key}", _storageOptions.BucketName, key);
        var response = await _s3Client.PutObjectAsync(putRequest, cancellationToken);
        if (response.HttpStatusCode != HttpStatusCode.OK && (int)response.HttpStatusCode >= 400)
        {
            throw new InvalidOperationException($"Failed to upload {key} to Neon S3: {response.HttpStatusCode}");
        }
    }

    public async Task<Stream?> OpenReadAsync(string relativePath, CancellationToken cancellationToken = default)
    {
        var key = NormalizeKey(relativePath);

        try
        {
            var getResponse = await _s3Client.GetObjectAsync(_storageOptions.BucketName, key, cancellationToken);
            return getResponse.ResponseStream;
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == HttpStatusCode.NotFound || ex.ErrorCode == "NoSuchKey")
        {
            return null;
        }
    }

    public async Task DeleteAsync(string relativePath, CancellationToken cancellationToken = default)
    {
        var key = NormalizeKey(relativePath);

        try
        {
            await _s3Client.DeleteObjectAsync(_storageOptions.BucketName, key, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to delete image {Key} from Neon S3", key);
        }
    }

    public string GetPublicUrl(string relativePath)
    {
        var normalizedKey = NormalizeKey(relativePath);

        if (!string.IsNullOrWhiteSpace(_storageOptions.PublicBaseUrl))
        {
            return $"{_storageOptions.PublicBaseUrl.TrimEnd('/')}/{normalizedKey}";
        }

        var prefix = _imagesOptions.RequestPath.TrimEnd('/');
        return $"{prefix}/{normalizedKey}";
    }

    private static string NormalizeKey(string relativePath)
    {
        return relativePath.Replace('\\', '/').TrimStart('/');
    }
}
