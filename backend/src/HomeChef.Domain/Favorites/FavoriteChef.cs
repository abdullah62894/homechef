using HomeChef.Domain.Chefs;
using HomeChef.Domain.Identity;

namespace HomeChef.Domain.Favorites;

public class FavoriteChef
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public ApplicationUser? User { get; set; }

    public Guid ChefProfileId { get; set; }

    public ChefProfile? ChefProfile { get; set; }

    public DateTime CreatedAtUtc { get; set; }
}
