using HomeChef.Domain.Chefs;

namespace HomeChef.Application.Features.Chefs;

public interface IChefAvailabilityRepository
{
    Task<IReadOnlyList<ChefAvailability>> ListByChefAsync(Guid chefProfileId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ChefAvailability>> ListByChefAndDayAsync(Guid chefProfileId, int dayOfWeek, CancellationToken cancellationToken = default);
    Task ReplaceAllAsync(Guid chefProfileId, List<ChefAvailability> windows, CancellationToken cancellationToken = default);
}
