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
    private readonly ImagesOptions _options;

    public LocalImageStorage(IOptions<ImagesOptions> options)
    {
        _options = options.Value;
        _root = Path.GetFullPath(_options.StoragePath);
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

    public Task<Stream?> OpenReadAsync(string relativePath, CancellationToken cancellationToken = default)
    {
        var fullPath = Path.GetFullPath(Path.Combine(_root, relativePath));
        if (!fullPath.StartsWith(_root, StringComparison.OrdinalIgnoreCase) || !File.Exists(fullPath))
        {
            return Task.FromResult<Stream?>(null);
        }

        Stream stream = File.OpenRead(fullPath);
        return Task.FromResult<Stream?>(stream);
    }

    public Task DeleteAsync(string relativePath, CancellationToken cancellationToken = default)
    {
        var fullPath = Path.GetFullPath(Path.Combine(_root, relativePath));
        if (fullPath.StartsWith(_root, StringComparison.OrdinalIgnoreCase) && File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }

        return Task.CompletedTask;
    }

    public string GetPublicUrl(string relativePath)
    {
        var prefix = _options.RequestPath.TrimEnd('/');
        var path = relativePath.Replace('\\', '/').TrimStart('/');
        return $"{prefix}/{path}";
    }
}
