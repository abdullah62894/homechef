namespace HomeChef.Application.Features.Chefs.Contracts;

public sealed class ChefListItemDto
{
    public Guid Id { get; set; }

    public string DisplayName { get; set; } = string.Empty;

    public string Bio { get; set; } = string.Empty;

    public string City { get; set; } = string.Empty;

    public string? Area { get; set; }

    public string? Address { get; set; }

    public double? Latitude { get; set; }

    public double? Longitude { get; set; }

    public double? DistanceKm { get; set; }

    public string[] Cuisines { get; set; } = [];

    public string? PhotoUrl { get; set; }

    public string? PhotoThumbnailUrl { get; set; }

    /// <summary>Average review rating (null when unrated).</summary>
    public double? RatingAverage { get; set; }

    public int RatingCount { get; set; }

    /// <summary>Cheapest available dish price, when the chef has a menu.</summary>
    public decimal? StartingPrice { get; set; }
}

public sealed class ChefProfileDto
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public string DisplayName { get; set; } = string.Empty;

    public string Bio { get; set; } = string.Empty;

    public string City { get; set; } = string.Empty;

    public string? Area { get; set; }

    public string? Address { get; set; }

    public double? Latitude { get; set; }

    public double? Longitude { get; set; }

    public double? DistanceKm { get; set; }

    public string[] Cuisines { get; set; } = [];

    public string? PhotoUrl { get; set; }

    public string? PhotoThumbnailUrl { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public DateTime UpdatedAtUtc { get; set; }

    public string? PhoneNumber { get; set; }

    public string? WhatsAppNumber { get; set; }

    public double? DeliveryRadiusKm { get; set; }
}