namespace HomeChef.Application.Features.Foods.Contracts;

public sealed record FoodCategoryDto
{
    public required Guid Id { get; init; }

    public required string Name { get; init; }

    public required string Slug { get; init; }

    public string? Description { get; init; }

    public int DisplayOrder { get; init; }
}
