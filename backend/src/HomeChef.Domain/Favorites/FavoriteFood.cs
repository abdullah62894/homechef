using HomeChef.Domain.Foods;
using HomeChef.Domain.Identity;

namespace HomeChef.Domain.Favorites;

public class FavoriteFood
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public ApplicationUser? User { get; set; }

    public Guid FoodItemId { get; set; }

    public FoodItem? FoodItem { get; set; }

    public DateTime CreatedAtUtc { get; set; }
}
