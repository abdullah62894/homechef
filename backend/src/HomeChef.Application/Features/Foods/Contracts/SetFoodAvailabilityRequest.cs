namespace HomeChef.Application.Features.Foods.Contracts;

public sealed record SetFoodAvailabilityRequest
{
    public bool IsAvailable { get; init; }
}
