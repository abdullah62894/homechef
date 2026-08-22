namespace HomeChef.Application.Features.Images.Contracts;

/// <summary>URLs of a stored, optimized image pair.</summary>
public sealed record ImageUploadResult
{
    public required string Url { get; init; }

    public required string ThumbnailUrl { get; init; }
}
