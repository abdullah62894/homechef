using System.ComponentModel.DataAnnotations;

namespace HomeChef.Application.Features.Foods.Contracts;

public sealed record CreateFoodItemRequest
{
    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string Name { get; init; } = string.Empty;

    [Required]
    [StringLength(2000, MinimumLength = 5)]
    public string Description { get; init; } = string.Empty;

    [Range(0.01, 100000.00)]
    public decimal Price { get; init; }

    [StringLength(10)]
    public string? Currency { get; init; }

    public Guid? CategoryId { get; init; }

    public bool IsAvailable { get; init; } = true;

    [StringLength(500)]
    public string? ImageUrl { get; init; }

    [Range(1, 1440)]
    public int? PreparationTimeMinutes { get; init; }
}
