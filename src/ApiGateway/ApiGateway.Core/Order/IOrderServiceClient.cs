using ApiGateway.Core.Common;
using ApiGateway.Core.Order;

namespace ApiGateway.Core.Order;

/// <summary>
/// Client interface for Order gRPC service
/// </summary>
public interface IOrderServiceClient
{
    Task<ServiceResult<OrderDto>> CreateOrder(CreateOrderRequestDto request);
    Task<ServiceResult<OrderDto>> GetOrderById(string orderId);
    Task<ServiceResult<List<OrderDto>>> GetOrdersByUserId(string userId);
    Task<ServiceResult<OrderDto>> CancelOrder(string orderId, string? reason = null);
    Task<ServiceResult<OrderDto>> UpdateOrderStatus(string orderId, string newStatus);
}

