using HomeChef.Application.Common.Errors;
using HomeChef.Application.Common.Exceptions;
using HomeChef.Application.Features.Images.Contracts;
using Microsoft.Extensions.Options;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;

namespace HomeChef.Application.Features.Images;

public sealed class ImageService : IImageService
{
    private static readonly IImageFormat[] AllowedFormats =
    [
        JpegFormat.Instance,
        PngFormat.Instance,
        WebpFormat.Instance,
    ];

    private readonly IImageStorage _storage;
    private readonly ImagesOptions _options;

    public ImageService(IImageStorage storage, IOptions<ImagesOptions> options)
    {
        _storage = storage;
        _options = options.Value;
    }

    public async Task<ImageUploadResult> UploadAsync(
        Stream content,
        long length,
        CancellationToken cancellationToken = default)
    {
        if (length <= 0)
        {
            throw new BusinessException(ErrorCodes.ImageInvalid, "The uploaded file is empty.");
        }

        if (length > _options.MaxFileSizeBytes)
        {
            throw new BusinessException(
                ErrorCodes.ImageTooLarge,
                $"The uploaded image exceeds the maximum size of {_options.MaxFileSizeBytes / (1024 * 1024)} MB.");
        }

        using var image = LoadImage(content);

        var encoder = new WebpEncoder { Quality = _options.Quality };

        using var optimized = image.Clone(x => x.AutoOrient().Resize(new ResizeOptions
        {
            Mode = ResizeMode.Max,
            Size = new Size(_options.MaxDimension, _options.MaxDimension),
        }));

        using var thumbnail = image.Clone(x => x.AutoOrient().Resize(new ResizeOptions
        {
            Mode = ResizeMode.Max,
            Size = new Size(_options.ThumbnailDimension, _options.ThumbnailDimension),
        }));

        var now = DateTime.UtcNow;
        var basePath = $"{now:yyyy/MM}/{Guid.NewGuid():N}";
        var imagePath = $"{basePath}.webp";
        var thumbnailPath = $"{basePath}_thumb.webp";

        await using (var imageStream = new MemoryStream())
        {
            await optimized.SaveAsync(imageStream, encoder, cancellationToken);
            imageStream.Position = 0;
            await _storage.SaveAsync(imagePath, imageStream, cancellationToken);
        }

        await using (var thumbnailStream = new MemoryStream())
        {
            await thumbnail.SaveAsync(thumbnailStream, encoder, cancellationToken);
            thumbnailStream.Position = 0;
            await _storage.SaveAsync(thumbnailPath, thumbnailStream, cancellationToken);
        }

        var prefix = _options.RequestPath.TrimEnd('/');
        return new ImageUploadResult
        {
            Url = $"{prefix}/{imagePath}",
            ThumbnailUrl = $"{prefix}/{thumbnailPath}",
        };
    }

    private static Image LoadImage(Stream content)
    {
        try
        {
            var image = Image.Load(content);

            if (!AllowedFormats.Contains(image.Metadata.DecodedImageFormat))
            {
                image.Dispose();
                throw new BusinessException(
                    ErrorCodes.ImageInvalidType,
                    "Only JPEG, PNG and WebP images are accepted.");
            }

            return image;
        }
        catch (BusinessException)
        {
            throw;
        }
        catch (SixLabors.ImageSharp.UnknownImageFormatException)
        {
            throw new BusinessException(
                ErrorCodes.ImageInvalidType,
                "Only JPEG, PNG and WebP images are accepted.");
        }
    }
}
