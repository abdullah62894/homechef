using HomeChef.Application.Features.Images.Contracts;

namespace HomeChef.Application.Features.Images;

public interface IImageService
{
    /// <summary>
    /// Validates, decodes, optimizes (auto-orient, resize, WebP re-encode)
    /// and stores an uploaded image together with a thumbnail.
    /// </summary>
    Task<ImageUploadResult> UploadAsync(Stream content, long length, CancellationToken cancellationToken = default);
}
