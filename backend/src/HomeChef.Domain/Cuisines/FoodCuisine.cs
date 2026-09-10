using HomeChef.Domain.Foods;

namespace HomeChef.Domain.Cuisines;

/// <summary>
/// Many-to-many join between <see cref="FoodItem"/> and <see cref="Cuisine"/>.
/// A dish must have at least one cuisine.
/// </summary>
public class FoodCuisine
{
    public Guid FoodItemId { get; set; }

    public FoodItem FoodItem { get; set; } = null!;

    public Guid CuisineId { get; set; }

    public Cuisine Cuisine { get; set; } = null!;
}
