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

    /// <summary>Opens a read stream for the stored image, or returns null if not found.</summary>
    Task<Stream?> OpenReadAsync(string relativePath, CancellationToken cancellationToken = default);

    /// <summary>Deletes the stored image if it exists.</summary>
    Task DeleteAsync(string relativePath, CancellationToken cancellationToken = default);

    /// <summary>Returns the public URL to access the image.</summary>
    string GetPublicUrl(string relativePath);
}
