using System.Web;
using HomeChef.Application.Common.Errors;
using HomeChef.Application.Common.Exceptions;
using HomeChef.Application.Features.Chefs;
using HomeChef.Application.Features.Foods;
using HomeChef.Domain.Chefs;
using HomeChef.Domain.Foods;
using HomeChef.Domain.Orders;

namespace HomeChef.Application.Features.Orders;

public interface IOrderService
{
    Task<Order> CreateOrderAsync(
        Guid customerUserId, Guid chefProfileId, List<OrderItemInput> items,
        string? deliveryAddress, string? customerPhone, string? customerName, string? deliveryMethod);
    Task<Order?> GetOrderAsync(Guid orderId);
    Task<IReadOnlyList<Order>> ListOrdersByCustomerAsync(Guid userId);
    Task<IReadOnlyList<Order>> ListOrdersByChefAsync(Guid chefProfileId);
    Task<IReadOnlyList<Order>> ListAllOrdersAsync(int page, int pageSize);
    Task MarkWhatsAppInitiatedAsync(Guid orderId);
    Task MarkCompletedAsync(Guid orderId, Guid chefUserId);
    string GenerateWhatsAppMessage(Order order, ChefProfile chef, List<OrderItem> items);
    string GenerateWhatsAppUrl(ChefProfile chef, string message);
    Task<List<OrderByChef>> GroupOrderItemsByChefAsync(List<CartInput> cartItems);
}

public record OrderItemInput(Guid FoodItemId, string DishName, int Quantity, decimal UnitPrice, string Currency);
public record CartInput(Guid FoodItemId, Guid ChefProfileId, string DishName, int Quantity, decimal UnitPrice, string Currency);
public record OrderByChef(ChefProfile Chef, List<CartInput> Items, decimal Subtotal);

public sealed class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IChefProfileRepository _chefRepository;
    private readonly IFoodRepository _foodRepository;

    public OrderService(IOrderRepository orderRepository, IChefProfileRepository chefRepository, IFoodRepository foodRepository)
    {
        _orderRepository = orderRepository;
        _chefRepository = chefRepository;
        _foodRepository = foodRepository;
    }

    public async Task<Order> CreateOrderAsync(
        Guid customerUserId, Guid chefProfileId, List<OrderItemInput> items,
        string? deliveryAddress, string? customerPhone, string? customerName, string? deliveryMethod)
    {
        var chef = await _chefRepository.GetByIdAsync(chefProfileId)
            ?? throw new BusinessException(ErrorCodes.ChefProfileNotFound, "Chef profile not found.");

        if (chef.ApprovalStatus != ChefApprovalStatus.Approved)
            throw new BusinessException(ErrorCodes.ChefNotApproved, "Chef not approved for orders.");

        decimal subtotal = 0;
        var orderItems = new List<OrderItem>();

        foreach (var item in items)
        {
            var foodItem = await _foodRepository.GetByIdAsync(item.FoodItemId)
                ?? throw new BusinessException(ErrorCodes.FoodItemNotFound, "Food item not found.");

            if (foodItem.ChefProfileId != chefProfileId)
                throw new BusinessException(ErrorCodes.OrderInvalid, "One or more items do not belong to this chef.");

            var orderItem = new OrderItem
            {
                Id = Guid.NewGuid(),
                FoodItemId = foodItem.Id,
                DishName = foodItem.Name,
                Quantity = item.Quantity,
                UnitPrice = foodItem.Price,
                Currency = foodItem.Currency,
                CreatedAtUtc = DateTime.UtcNow,
            };

            subtotal += orderItem.UnitPrice * orderItem.Quantity;
            orderItems.Add(orderItem);
        }

        var order = new Order
        {
            Id = Guid.NewGuid(),
            CustomerUserId = customerUserId,
            ChefProfileId = chefProfileId,
            Subtotal = subtotal,
            Currency = "PKR",
            Status = OrderStatus.Pending,
            DeliveryAddress = deliveryAddress,
            CustomerPhone = customerPhone,
            CustomerName = customerName,
            DeliveryMethod = deliveryMethod,
            CreatedAtUtc = DateTime.UtcNow,
            Items = orderItems,
        };

        await _orderRepository.AddAsync(order);
        return order;
    }

    public Task<Order?> GetOrderAsync(Guid orderId) => _orderRepository.GetByIdAsync(orderId);
    public Task<IReadOnlyList<Order>> ListOrdersByCustomerAsync(Guid userId) => _orderRepository.ListByCustomerAsync(userId);
    public Task<IReadOnlyList<Order>> ListOrdersByChefAsync(Guid chefProfileId) => _orderRepository.ListByChefAsync(chefProfileId);
    public Task<IReadOnlyList<Order>> ListAllOrdersAsync(int page, int pageSize) => _orderRepository.ListAllAsync(page, pageSize);

    public async Task MarkWhatsAppInitiatedAsync(Guid orderId)
    {
        var order = await _orderRepository.GetByIdAsync(orderId)
            ?? throw new BusinessException(ErrorCodes.OrderNotFound, "Order not found.");
        order.Status = OrderStatus.WhatsAppInitiated;
        order.WhatsAppInitiatedAtUtc = DateTime.UtcNow;
        await _orderRepository.UpdateAsync(order);
    }

    public async Task MarkCompletedAsync(Guid orderId, Guid chefUserId)
    {
        var order = await _orderRepository.GetByIdAsync(orderId)
            ?? throw new BusinessException(ErrorCodes.OrderNotFound, "Order not found.");

        var chef = await _chefRepository.GetByUserIdAsync(chefUserId)
            ?? throw new BusinessException(ErrorCodes.ChefProfileNotFound, "Chef profile not found.");

        if (order.ChefProfileId != chef.Id)
            throw new BusinessException(ErrorCodes.FoodItemForbidden, "You are not authorized to modify this order.");

        if (order.Status == OrderStatus.Completed || order.Status == OrderStatus.Cancelled)
            throw new BusinessException(ErrorCodes.OrderInvalid, "Order is already finalized.");

        order.Status = OrderStatus.Completed;
        order.CompletedAtUtc = DateTime.UtcNow;
        await _orderRepository.UpdateAsync(order);
    }

    public string GenerateWhatsAppMessage(Order order, ChefProfile chef, List<OrderItem> items)
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine($"Hello Chef {chef.DisplayName},");
        sb.AppendLine();
        sb.AppendLine("I would like to place an order:");
        sb.AppendLine();
        sb.AppendLine($"Chef: {chef.DisplayName}");
        sb.AppendLine();
        sb.AppendLine("Items:");

        foreach (var item in items)
            sb.AppendLine($"• {item.DishName} × {item.Quantity} — {item.Currency} {item.UnitPrice * item.Quantity:N0}");

        sb.AppendLine();
        sb.AppendLine($"Total: {order.Currency} {order.Subtotal:N0}");
        sb.AppendLine();

        if (!string.IsNullOrEmpty(order.CustomerName) || !string.IsNullOrEmpty(order.CustomerPhone))
        {
            sb.AppendLine("Customer:");
            if (!string.IsNullOrEmpty(order.CustomerName)) sb.AppendLine($"Name: {order.CustomerName}");
            if (!string.IsNullOrEmpty(order.CustomerPhone)) sb.AppendLine($"Phone: {order.CustomerPhone}");
            sb.AppendLine();
        }

        if (!string.IsNullOrEmpty(order.DeliveryAddress))
        {
            sb.AppendLine("Delivery:");
            sb.AppendLine(order.DeliveryAddress);
            sb.AppendLine();
        }

        sb.AppendLine("Please confirm availability and delivery details.");
        return sb.ToString();
    }

    public string GenerateWhatsAppUrl(ChefProfile chef, string message)
    {
        var phone = chef.WhatsAppNumber ?? chef.PhoneNumber;
        if (string.IsNullOrWhiteSpace(phone))
            throw new BusinessException(ErrorCodes.PhoneNotConfigured, "Chef phone number not configured.");

        var cleaned = new string(phone.Where(c => char.IsDigit(c) || c == '+').ToArray());
        var encoded = HttpUtility.UrlEncode(message);
        return $"https://wa.me/{cleaned}?text={encoded}";
    }

    public async Task<List<OrderByChef>> GroupOrderItemsByChefAsync(List<CartInput> cartItems)
    {
        var chefIds = cartItems.Select(c => c.ChefProfileId).Distinct().ToList();
        var chefs = new Dictionary<Guid, ChefProfile>();

        foreach (var id in chefIds)
        {
            var chef = await _chefRepository.GetByIdAsync(id);
            if (chef != null) chefs[id] = chef;
        }

        return cartItems
            .Where(c => chefs.ContainsKey(c.ChefProfileId))
            .GroupBy(c => c.ChefProfileId)
            .Select(g => new OrderByChef(
                chefs[g.Key],
                g.ToList(),
                g.Sum(i => i.UnitPrice * i.Quantity)))
            .ToList();
    }
}
