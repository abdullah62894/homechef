using HomeChef.Domain.Chefs;

namespace HomeChef.Domain.Foods;

/// <summary>
/// A food or menu item offered by a home chef.
/// </summary>
public class FoodItem
{
    public Guid Id { get; set; }

    public Guid ChefProfileId { get; set; }

    public ChefProfile ChefProfile { get; set; } = null!;

    public Guid? CategoryId { get; set; }

    public FoodCategory? Category { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public string Currency { get; set; } = "PKR";

    public bool IsAvailable { get; set; } = true;

    /// <summary>Reserved for image storage stage.</summary>
    public string? ImageUrl { get; set; }

    public int? PreparationTimeMinutes { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public DateTime UpdatedAtUtc { get; set; }
}
