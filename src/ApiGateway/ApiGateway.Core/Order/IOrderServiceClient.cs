using ApiGateway.Core.Common;
using ApiGateway.Core.Order.Dto;

namespace ApiGateway.Core.Order;

/// <summary>
/// Client interface for Order gRPC service
/// </summary>
public interface IOrderServiceClient
{
  
    Task<ServiceResult<OrderDto>> CreateOrder(CreateOrderRequestWithPricesDto request,CancellationToken cancellationToken = default);
    Task<ServiceResult<OrderDto>> GetOrderById(string orderId, CancellationToken cancellationToken = default);
    Task<ServiceResult<List<OrderDto>>> GetOrdersByUserId(string userId, CancellationToken cancellationToken = default);
    Task<ServiceResult<OrderDto>> CancelOrder(string orderId, string? reason = null, CancellationToken cancellationToken = default);
    Task<ServiceResult<OrderDto>> UpdateOrderStatus(string orderId, string newStatus, CancellationToken cancellationToken = default);
    Task<ServiceResult<AddOrderItemResultDto>> AddOrderItem(string orderId, OrderItemWithPriceDto request, CancellationToken cancellationToken = default);
    Task<ServiceResult<RemoveOrderItemResultDto>> RemoveOrderItem(string orderId, string itemId, CancellationToken cancellationToken = default);
    Task<ServiceResult<UpdateOrderItemQuantityResultDto>> UpdateOrderItemQuantity(string orderId, string itemId, int newQuantity, CancellationToken cancellationToken = default);
}

