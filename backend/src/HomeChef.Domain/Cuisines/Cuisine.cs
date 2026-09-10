namespace HomeChef.Domain.Cuisines;

/// <summary>
/// A cuisine type that can be assigned to food items (e.g. "Pakistani", "Chinese").
/// Managed by admins; cover images can be used as fallback for dishes without photos.
/// </summary>
public class Cuisine
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Slug { get; set; } = string.Empty;

    public string? Description { get; set; }

    /// <summary>Full-size cover image URL for this cuisine.</summary>
    public string? ImageUrl { get; set; }

    /// <summary>Thumbnail variant of <see cref="ImageUrl"/>.</summary>
    public string? ImageThumbnailUrl { get; set; }

    public int DisplayOrder { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAtUtc { get; set; }
}
