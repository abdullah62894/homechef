using HomeChef.Domain.Chefs;

namespace HomeChef.Application.Features.Chefs;

public interface IChefDeliveryMethodRepository
{
    Task<IReadOnlyList<ChefDeliveryMethod>> ListByChefAsync(Guid chefProfileId, CancellationToken cancellationToken = default);
    Task ReplaceAllAsync(Guid chefProfileId, List<DeliveryMethodType> methods, CancellationToken cancellationToken = default);
}
