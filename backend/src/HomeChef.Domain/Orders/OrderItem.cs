using System.Text.Json.Serialization;
using HomeChef.Domain.Foods;

namespace HomeChef.Domain.Orders;

/// <summary>
/// A line item in an order. Stores the price at time of checkout.
/// </summary>
public class OrderItem
{
    public Guid Id { get; set; }

    public Guid OrderId { get; set; }

    [JsonIgnore]
    public Order Order { get; set; } = null!;

    public Guid FoodItemId { get; set; }

    [JsonIgnore]
    public FoodItem FoodItem { get; set; } = null!;

    /// <summary>Dish name at time of order.</summary>
    public string DishName { get; set; } = string.Empty;

    public int Quantity { get; set; }

    public decimal UnitPrice { get; set; }

    public string Currency { get; set; } = "PKR";

    public DateTime CreatedAtUtc { get; set; }
}
