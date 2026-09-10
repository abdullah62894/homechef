using HomeChef.Domain.Foods;

namespace HomeChef.Domain.Meals;

/// <summary>
/// Many-to-many join between <see cref="FoodItem"/> and <see cref="MealCategory"/>.
/// </summary>
public class FoodMealCategory
{
    public Guid FoodItemId { get; set; }

    public FoodItem FoodItem { get; set; } = null!;

    public Guid MealCategoryId { get; set; }

    public MealCategory MealCategory { get; set; } = null!;
}
