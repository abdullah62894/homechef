namespace HomeChef.Domain.Chefs;

/// <summary>
/// Delivery methods a chef supports. Configurable rather than hardcoded.
/// </summary>
public enum DeliveryMethodType
{
    InDrive = 1,
    ThirdPartyService = 2,
    OwnRider = 3,
    Pickup = 4,
}
