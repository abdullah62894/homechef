using System.Text.Json.Serialization;
using HomeChef.Domain.Chefs;
using HomeChef.Domain.Identity;

namespace HomeChef.Domain.Orders;

/// <summary>
/// An order/request record created when a customer initiates checkout.
/// Payment and confirmation happen externally via WhatsApp.
/// </summary>
public class Order
{
    public Guid Id { get; set; }

    public Guid CustomerUserId { get; set; }

    [JsonIgnore]
    public ApplicationUser CustomerUser { get; set; } = null!;

    public Guid ChefProfileId { get; set; }

    [JsonIgnore]
    public ChefProfile ChefProfile { get; set; } = null!;

    public decimal Subtotal { get; set; }

    public string Currency { get; set; } = "PKR";

    public OrderStatus Status { get; set; } = OrderStatus.Pending;

    /// <summary>Customer delivery address or pickup note.</summary>
    public string? DeliveryAddress { get; set; }

    /// <summary>Customer phone number.</summary>
    public string? CustomerPhone { get; set; }

    /// <summary>Customer name for the order.</summary>
    public string? CustomerName { get; set; }

    /// <summary>Selected delivery method.</summary>
    public string? DeliveryMethod { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public DateTime? WhatsAppInitiatedAtUtc { get; set; }

    public DateTime? CompletedAtUtc { get; set; }

    public DateTime? CancelledAtUtc { get; set; }

    public ICollection<OrderItem> Items { get; set; } = [];
}
