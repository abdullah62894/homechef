using System.Security.Claims;
using HomeChef.Api.Common;
using HomeChef.Application.Features.Chefs;
using HomeChef.Application.Features.Orders;
using HomeChef.Domain.Constants;
using HomeChef.Domain.Chefs;
using HomeChef.Domain.Orders;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HomeChef.Api.Controllers;

[ApiController]
[Route("api/orders")]
public sealed class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;
    private readonly IChefProfileRepository _chefRepository;

    public OrdersController(IOrderService orderService, IChefProfileRepository chefRepository)
    {
        _orderService = orderService;
        _chefRepository = chefRepository;
    }

    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<Order>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request, CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var inputs = request.Items.Select(i => new OrderItemInput(i.FoodItemId, i.DishName, i.Quantity, i.UnitPrice, i.Currency)).ToList();
        var order = await _orderService.CreateOrderAsync(
            userId, request.ChefProfileId, inputs,
            request.DeliveryAddress, request.CustomerPhone, request.CustomerName, request.DeliveryMethod);

        return Created(string.Empty, new ApiResponse<Order>(order));
    }

    [HttpGet("{id:guid}")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<Order>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetOrder(Guid id, CancellationToken cancellationToken)
    {
        var order = await _orderService.GetOrderAsync(id);
        if (order is null) return NotFound();
        return Ok(new ApiResponse<Order>(order));
    }

    [HttpGet("my")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<Order>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListMyOrders(CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        return Ok(new ApiResponse<IReadOnlyList<Order>>(await _orderService.ListOrdersByCustomerAsync(userId)));
    }

    [HttpGet("chef/{chefId:guid}")]
    [Authorize(Policy = Policies.RequireChef)]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<Order>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListChefOrders(Guid chefId, CancellationToken cancellationToken)
    {
        return Ok(new ApiResponse<IReadOnlyList<Order>>(await _orderService.ListOrdersByChefAsync(chefId)));
    }

    [HttpGet("admin")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<Order>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListAllOrders(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
    {
        return Ok(new ApiResponse<IReadOnlyList<Order>>(await _orderService.ListAllOrdersAsync(page, pageSize)));
    }

    [HttpPost("{id:guid}/whatsapp-initiated")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> MarkWhatsAppInitiated(Guid id, CancellationToken cancellationToken)
    {
        await _orderService.MarkWhatsAppInitiatedAsync(id);
        return NoContent();
    }

    [HttpPost("{id:guid}/complete")]
    [Authorize(Policy = Policies.RequireChef)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> MarkCompleted(Guid id, CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await _orderService.MarkCompletedAsync(id, userId);
        return NoContent();
    }

    [HttpGet("{id:guid}/whatsapp")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetWhatsAppUrl(Guid id, CancellationToken cancellationToken)
    {
        var order = await _orderService.GetOrderAsync(id);
        if (order is null) return NotFound();

        var chef = await _chefRepository.GetByIdAsync(order.ChefProfileId);
        if (chef is null) return NotFound();

        var message = _orderService.GenerateWhatsAppMessage(order, chef, order.Items.ToList());
        var url = _orderService.GenerateWhatsAppUrl(chef, message);

        return Ok(new ApiResponse<object>(new { url, message }));
    }

    [HttpPost("group-by-chef")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<List<OrderByChef>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GroupByChef([FromBody] List<CartInput> cartItems, CancellationToken cancellationToken)
    {
        return Ok(new ApiResponse<List<OrderByChef>>(await _orderService.GroupOrderItemsByChefAsync(cartItems)));
    }
}

public record CreateOrderRequest(
    Guid ChefProfileId,
    List<OrderItemRequest> Items,
    string? DeliveryAddress,
    string? CustomerPhone,
    string? CustomerName,
    string? DeliveryMethod);

public record OrderItemRequest(Guid FoodItemId, string DishName, int Quantity, decimal UnitPrice, string Currency);
