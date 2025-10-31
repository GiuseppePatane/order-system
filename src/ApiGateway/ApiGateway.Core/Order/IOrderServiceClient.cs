using ApiGateway.Core.Common;
using ApiGateway.Core.Order.Dto;

namespace ApiGateway.Core.Order;

/// <summary>
/// Client interface for Order gRPC service
/// </summary>
public interface IOrderServiceClient
{
  
    Task<ServiceResult<OrderDto>> CreateOrder(CreateOrderRequestWithPricesDto request);
    Task<ServiceResult<OrderDto>> GetOrderById(string orderId);
    Task<ServiceResult<List<OrderDto>>> GetOrdersByUserId(string userId);
    Task<ServiceResult<OrderDto>> CancelOrder(string orderId, string? reason = null);
    Task<ServiceResult<OrderDto>> UpdateOrderStatus(string orderId, string newStatus);
    Task<ServiceResult<AddOrderItemResultDto>> AddOrderItem(string orderId, OrderItemWithPriceDto request);
    Task<ServiceResult<RemoveOrderItemResultDto>> RemoveOrderItem(string orderId, string itemId);
    Task<ServiceResult<UpdateOrderItemQuantityResultDto>> UpdateOrderItemQuantity(string orderId, string itemId, int newQuantity);
}

