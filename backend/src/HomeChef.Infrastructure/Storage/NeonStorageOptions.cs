namespace HomeChef.Infrastructure.Storage;

/// <summary>
/// Settings for Neon S3-compatible object storage.
/// </summary>
public sealed class NeonStorageOptions
{
    public const string SectionName = "NeonStorage";

    /// <summary>S3 endpoint URL (e.g. https://br-delicate-salad-a5lwkpmi.storage.c-1.us-east-2.aws.neon.tech).</summary>
    public string? EndpointUrl { get; set; }

    /// <summary>S3 Access Key ID.</summary>
    public string? AccessKeyId { get; set; }

    /// <summary>S3 Secret Access Key.</summary>
    public string? SecretAccessKey { get; set; }

    /// <summary>AWS region (default us-east-2).</summary>
    public string Region { get; set; } = "us-east-2";

    /// <summary>Bucket name (default homechef).</summary>
    public string BucketName { get; set; } = "homechef";

    /// <summary>Optional custom public base URL for direct CDN or public reads.</summary>
    public string? PublicBaseUrl { get; set; }
}
