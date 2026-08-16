namespace HomeChef.Domain.Foods;

/// <summary>
/// A category for classifying food/menu items (e.g. "Main Course", "Bakery &amp; Desserts").
/// </summary>
public class FoodCategory
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Slug { get; set; } = string.Empty;

    public string? Description { get; set; }

    public int DisplayOrder { get; set; }

    public DateTime CreatedAtUtc { get; set; }
}
