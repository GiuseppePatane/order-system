using ApiGateway.Core.Common;

namespace ApiGateway.Core.Order;

/// <summary>
/// Service for orchestrating order operations across multiple bounded contexts
/// </summary>
public interface IOrderOrchestrationService
{
    /// <summary>
    /// Creates an order after validating user and products exist
    /// </summary>
    Task<ServiceResult<OrderDto>> CreateOrderAsync(CreateOrderRequestDto request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets an order by ID
    /// </summary>
    Task<ServiceResult<OrderDto>> GetOrderByIdAsync(string orderId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets orders for a specific user
    /// </summary>
    Task<ServiceResult<List<OrderDto>>> GetUserOrdersAsync(string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Cancels an order
    /// </summary>
    Task<ServiceResult<OrderDto>> CancelOrderAsync(string orderId, string? reason = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates order status
    /// </summary>
    Task<ServiceResult<OrderDto>> UpdateOrderStatusAsync(string orderId, string newStatus, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds an item to an existing order with server-validated price
    /// </summary>
    Task<ServiceResult<AddOrderItemResultDto>> AddOrderItemAsync(string orderId, AddOrderItemRequestDto request, CancellationToken cancellationToken = default);
}

