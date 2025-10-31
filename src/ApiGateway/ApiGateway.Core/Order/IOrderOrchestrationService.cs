using ApiGateway.Core.Common;
using ApiGateway.Core.Order.Dto;

namespace ApiGateway.Core.Order;

/// <summary>
/// Service for orchestrating order operations across multiple bounded contexts
/// </summary>
public interface IOrderOrchestrationService
{

    /// <summary>
    /// Gets an order by ID
    /// </summary>
    Task<ServiceResult<OrderDto>> GetOrderByIdAsync(string orderId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets orders for a specific user
    /// </summary>
    Task<ServiceResult<List<OrderDto>>> GetUserOrdersAsync(string userId, CancellationToken cancellationToken = default);

    
    /// <summary>
    /// Creates an order after validating user and products exist
    /// </summary>
    Task<ServiceResult<OrderMutationResponseDto>> CreateOrderAsync(CreateOrderRequestDto request, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Updates order status
    /// </summary>
    Task<ServiceResult<OrderMutationResponseDto>> UpdateOrderStatusAsync(string orderId, string newStatus, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Cancels an order
    /// </summary>
    Task<ServiceResult<OrderMutationResponseDto>> CancelOrderAsync(string orderId, string? reason = null, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Adds an item to an existing order with server-validated price
    /// </summary>
    Task<ServiceResult<AddOrderItemResultDto>> AddOrderItemAsync(string orderId, OrderItemDto request, CancellationToken cancellationToken = default);

    Task<ServiceResult<UpdateOrderItemQuantityResultDto>> UpdateOrderItemQuantity(string orderId, string itemId, int requestQuantity, CancellationToken cancellationToken = default);

    Task<ServiceResult<RemoveOrderItemResultDto>> RemoveOrderItem(string orderId, string itemId,
        CancellationToken cancellationToken = default);

}

