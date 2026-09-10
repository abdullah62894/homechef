namespace HomeChef.Domain.Orders;

/// <summary>
/// Status of an order/order-request.
/// </summary>
public enum OrderStatus
{
    Pending = 0,
    WhatsAppInitiated = 1,
    Completed = 2,
    Cancelled = 3,
}
