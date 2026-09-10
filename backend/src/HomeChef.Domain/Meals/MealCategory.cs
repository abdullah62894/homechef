namespace HomeChef.Domain.Meals;

/// <summary>
/// A meal-time category: Breakfast, Lunch, or Dinner.
/// A dish must belong to at least one meal category.
/// </summary>
public class MealCategory
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Slug { get; set; } = string.Empty;

    public int DisplayOrder { get; set; }

    public DateTime CreatedAtUtc { get; set; }
}
