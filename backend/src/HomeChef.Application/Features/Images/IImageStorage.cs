namespace HomeChef.Application.Features.Images;

/// <summary>
/// Persistence for optimized image binaries. The default implementation
/// writes to local disk; a later stage can swap in object storage
/// (Cloudflare R2 / S3 / Azure Blob) without touching callers.
/// </summary>
public interface IImageStorage
{
    /// <summary>Writes the encoded image bytes under the given relative path.</summary>
    Task SaveAsync(string relativePath, Stream content, CancellationToken cancellationToken = default);
}
