namespace HomeChef.Application.Features.Favorites.Contracts;

public sealed class UserFavoriteIdsDto
{
    public IReadOnlyList<Guid> ChefIds { get; set; } = [];

    public IReadOnlyList<Guid> FoodIds { get; set; } = [];
}
