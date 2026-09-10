namespace HomeChef.Domain.Chefs;

/// <summary>
/// Join table linking a chef to their supported delivery methods.
/// </summary>
public class ChefDeliveryMethod
{
    public Guid Id { get; set; }

    public Guid ChefProfileId { get; set; }

    public ChefProfile ChefProfile { get; set; } = null!;

    public DeliveryMethodType Method { get; set; }

    public DateTime CreatedAtUtc { get; set; }
}
