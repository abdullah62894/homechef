using HomeChef.Application.Features.Images;
using Microsoft.Extensions.Options;

namespace HomeChef.Infrastructure.Storage;

/// <summary>
/// Writes images to the local filesystem under the configured storage path.
/// Intended to be replaced by object storage (R2/S3/Azure Blob) in the
/// production-deployment stage — the container filesystem is ephemeral.
/// </summary>
public sealed class LocalImageStorage : IImageStorage
{
    private readonly string _root;

    public LocalImageStorage(IOptions<ImagesOptions> options)
    {
        _root = Path.GetFullPath(options.Value.StoragePath);
    }

    public async Task SaveAsync(string relativePath, Stream content, CancellationToken cancellationToken = default)
    {
        var fullPath = Path.GetFullPath(Path.Combine(_root, relativePath));

        if (!fullPath.StartsWith(_root, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Image path escapes the storage root.");
        }

        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);

        await using var target = File.Create(fullPath);
        await content.CopyToAsync(target, cancellationToken);
    }
}
