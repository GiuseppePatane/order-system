using ApiGateway.Core.Common;
using ApiGateway.Core.Order;

namespace ApiGateway.Core.Order;

/// <summary>
/// Client interface for Order gRPC service
/// </summary>
public interface IOrderServiceClient
{
    /// <summary>
    /// Creates an order with server-validated prices
    /// </summary>
    Task<ServiceResult<OrderDto>> CreateOrder(CreateOrderRequestWithPricesDto request);
    Task<ServiceResult<OrderDto>> GetOrderById(string orderId);
    Task<ServiceResult<List<OrderDto>>> GetOrdersByUserId(string userId);
    Task<ServiceResult<OrderDto>> CancelOrder(string orderId, string? reason = null);
    Task<ServiceResult<OrderDto>> UpdateOrderStatus(string orderId, string newStatus);

    // Item management methods
    Task<ServiceResult<AddOrderItemResultDto>> AddOrderItem(string orderId, AddOrderItemWithPriceDto request);
    Task<ServiceResult<RemoveOrderItemResultDto>> RemoveOrderItem(string orderId, string itemId);
    Task<ServiceResult<UpdateOrderItemQuantityResultDto>> UpdateOrderItemQuantity(string orderId, string itemId, int newQuantity);
}

