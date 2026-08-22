namespace HomeChef.Application.Features.Images;

/// <summary>
/// Image upload pipeline settings (bound from the "Images" configuration section).
/// </summary>
public sealed class ImagesOptions
{
    public const string SectionName = "Images";

    /// <summary>Filesystem directory where optimized images are written.</summary>
    public string StoragePath { get; set; } = "uploads";

    /// <summary>Public URL prefix under which stored images are served.</summary>
    public string RequestPath { get; set; } = "/uploads";

    /// <summary>Maximum accepted upload size in bytes.</summary>
    public long MaxFileSizeBytes { get; set; } = 5 * 1024 * 1024;

    /// <summary>Longest image edge after optimization.</summary>
    public int MaxDimension { get; set; } = 1600;

    /// <summary>Longest thumbnail edge.</summary>
    public int ThumbnailDimension { get; set; } = 400;

    /// <summary>WebP encoding quality (0-100).</summary>
    public int Quality { get; set; } = 80;
}
