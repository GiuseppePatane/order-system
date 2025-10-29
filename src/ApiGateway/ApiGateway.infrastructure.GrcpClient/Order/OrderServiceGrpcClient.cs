using ApiGateway.Core.Common;
using ApiGateway.Core.Order;
using Microsoft.Extensions.Logging;
using Order.Protos;

namespace ApiGateway.infrastructure.GrcpClient.Order;

public class OrderServiceGrpcClient : IOrderServiceClient
{
    private readonly OrderService.OrderServiceClient _grpcClient;
    private readonly ILogger<OrderServiceGrpcClient> _logger;

    public OrderServiceGrpcClient(
        OrderService.OrderServiceClient grpcClient,
        ILogger<OrderServiceGrpcClient> logger)
    {
        _grpcClient = grpcClient;
        _logger = logger;
    }

    public async Task<ServiceResult<OrderDto>> CreateOrder(CreateOrderRequestDto request)
    {
        try
        {
            var grpcRequest = new CreateOrderRequest
            {
                UserId = request.UserId.ToString(),
                ShippingAddressId = request.ShippingAddressId.ToString(),
                BillingAddressId = request.BillingAddressId?.ToString()
            };

            // Map items
            foreach (var item in request.Items)
            {
                grpcRequest.Items.Add(new OrderItemInput
                {
                    ProductId = item.ProductId.ToString(),
                    Quantity = item.Quantity,
                    UnitPrice = (double)item.UnitPrice
                });
            }

            var response = await _grpcClient.CreateOrderAsync(grpcRequest);

            if (response.ResultCase == OrderResponse.ResultOneofCase.Data)
            {
                return ServiceResult<OrderDto>.Success(MapToOrderDto(response.Data));
            }

            return ServiceResult<OrderDto>.Failure(new ErrorInfo
            {
                Code = response.Error.Code,
                Message = response.Error.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating order via gRPC");
            return ServiceResult<OrderDto>.Failure(new ErrorInfo
            {
                Code = "GRPC_ERROR",
                Message = $"Failed to create order: {ex.Message}"
            });
        }
    }

    public async Task<ServiceResult<OrderDto>> GetOrderById(string orderId)
    {
        try
        {
            var request = new GetOrderRequest { OrderId = orderId };
            var response = await _grpcClient.GetOrderAsync(request);

            if (response.ResultCase == OrderResponse.ResultOneofCase.Data)
            {
                return ServiceResult<OrderDto>.Success(MapToOrderDto(response.Data));
            }

            return ServiceResult<OrderDto>.Failure(new ErrorInfo
            {
                Code = response.Error.Code,
                Message = response.Error.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting order {OrderId} via gRPC", orderId);
            return ServiceResult<OrderDto>.Failure(new ErrorInfo
            {
                Code = "GRPC_ERROR",
                Message = $"Failed to get order: {ex.Message}"
            });
        }
    }

    public async Task<ServiceResult<List<OrderDto>>> GetOrdersByUserId(string userId)
    {
        try
        {
            var request = new GetOrdersByUserRequest { UserId = userId };
            var response = await _grpcClient.GetOrdersByUserAsync(request);

            if (response.ResultCase == GetOrdersResponse.ResultOneofCase.Data)
            {
                var orders = response.Data.Items
                    .Select(MapToOrderDto)
                    .ToList();

                return ServiceResult<List<OrderDto>>.Success(orders);
            }

            return ServiceResult<List<OrderDto>>.Failure(new ErrorInfo
            {
                Code = response.Error.Code,
                Message = response.Error.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting orders for user {UserId} via gRPC", userId);
            return ServiceResult<List<OrderDto>>.Failure(new ErrorInfo
            {
                Code = "GRPC_ERROR",
                Message = $"Failed to get user orders: {ex.Message}"
            });
        }
    }

    public async Task<ServiceResult<OrderDto>> CancelOrder(string orderId, string? reason = null)
    {
        try
        {
            var request = new CancelOrderRequest
            {
                OrderId = orderId,
                Reason = reason
            };

            var response = await _grpcClient.CancelOrderAsync(request);

            if (response.ResultCase == OrderResponse.ResultOneofCase.Data)
            {
                return ServiceResult<OrderDto>.Success(MapToOrderDto(response.Data));
            }

            return ServiceResult<OrderDto>.Failure(new ErrorInfo
            {
                Code = response.Error.Code,
                Message = response.Error.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling order {OrderId} via gRPC", orderId);
            return ServiceResult<OrderDto>.Failure(new ErrorInfo
            {
                Code = "GRPC_ERROR",
                Message = $"Failed to cancel order: {ex.Message}"
            });
        }
    }

    public async Task<ServiceResult<OrderDto>> UpdateOrderStatus(string orderId, string newStatus)
    {
        try
        {
            // Parse status string to enum
            if (!Enum.TryParse<OrderStatus>(newStatus, true, out var statusEnum))
            {
                return ServiceResult<OrderDto>.Failure(new ErrorInfo
                {
                    Code = "INVALID_STATUS",
                    Message = $"Invalid order status: {newStatus}"
                });
            }

            var request = new UpdateOrderStatusRequest
            {
                OrderId = orderId,
                Status = statusEnum
            };

            var response = await _grpcClient.UpdateOrderStatusAsync(request);

            if (response.ResultCase == OrderResponse.ResultOneofCase.Data)
            {
                return ServiceResult<OrderDto>.Success(MapToOrderDto(response.Data));
            }

            return ServiceResult<OrderDto>.Failure(new ErrorInfo
            {
                Code = response.Error.Code,
                Message = response.Error.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating order {OrderId} status via gRPC", orderId);
            return ServiceResult<OrderDto>.Failure(new ErrorInfo
            {
                Code = "GRPC_ERROR",
                Message = $"Failed to update order status: {ex.Message}"
            });
        }
    }

    private OrderDto MapToOrderDto(OrderData data)
    {
        return new OrderDto
        {
            OrderId = data.OrderId,
            UserId = data.UserId,
            Status = MapOrderStatus(data.Status),
            TotalAmount = (decimal)data.TotalAmount,
            Items = data.Items.Select(item => new OrderItemResponseDto
            {
                ProductId = item.ProductId,
                ProductName = string.Empty, // Will be enriched by orchestration service
                Quantity = item.Quantity,
                UnitPrice = (decimal)item.UnitPrice,
                TotalPrice = (decimal)item.TotalPrice
            }).ToList(),
            CreatedAt = data.CreatedAt.ToDateTime()
        };
    }

    private string MapOrderStatus(OrderStatus status)
    {
        return status switch
        {
            OrderStatus.Pending => "Pending",
            OrderStatus.Confirmed => "Confirmed",
            OrderStatus.Processing => "Processing",
            OrderStatus.Shipped => "Shipped",
            OrderStatus.Delivered => "Delivered",
            OrderStatus.Cancelled => "Cancelled",
            _ => "Unknown"
        };
    }
}