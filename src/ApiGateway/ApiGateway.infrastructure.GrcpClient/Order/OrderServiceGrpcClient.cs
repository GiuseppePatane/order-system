using ApiGateway.Core.Common;
using ApiGateway.Core.Order;
using ApiGateway.Core.Order.Dto;
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

    public async Task<ServiceResult<OrderDto>> CreateOrder(CreateOrderRequestWithPricesDto request, CancellationToken cancellationToken = default)
    {
        try
        {
            var grpcRequest = new CreateOrderRequest
            {
                UserId = request.UserId.ToString(),
                ShippingAddressId = request.ShippingAddressId.ToString(),
                BillingAddressId = request.BillingAddressId?.ToString()
            };

            // Map items with server-validated prices
            foreach (var item in request.Items)
            {
                grpcRequest.Items.Add(new OrderItemInput
                {
                    ProductId = item.ProductId.ToString(),
                    Quantity = item.Quantity,
                    UnitPrice = (double)item.LockedPrice // Price from ProductService
                });
            }

            var response = await _grpcClient.CreateOrderAsync(grpcRequest, cancellationToken: cancellationToken);

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

    public async Task<ServiceResult<OrderDto>> GetOrderById(string orderId,CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new GetOrderRequest { OrderId = orderId };
            var response = await _grpcClient.GetOrderAsync(request, cancellationToken: cancellationToken);

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

    public async Task<ServiceResult<List<OrderDto>>> GetOrdersByUserId(string userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new GetOrdersByUserRequest { UserId = userId };
            var response = await _grpcClient.GetOrdersByUserAsync(request, cancellationToken: cancellationToken);

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

    public async Task<ServiceResult<OrderDto>> CancelOrder(string orderId, string? reason = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new CancelOrderRequest
            {
                OrderId = orderId,
                Reason = reason
            };

            var response = await _grpcClient.CancelOrderAsync(request, cancellationToken: cancellationToken);

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

    public async Task<ServiceResult<OrderDto>> UpdateOrderStatus(string orderId, string newStatus, CancellationToken cancellationToken = default)
    {
        try
        {
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

            var response = await _grpcClient.UpdateOrderStatusAsync(request, cancellationToken: cancellationToken);

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
                OrderItemId = item.OrderItemId,
                ProductId = item.ProductId,
                ProductName = string.Empty,  //Product service 
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

    public async Task<ServiceResult<AddOrderItemResultDto>> AddOrderItem(string orderId, OrderItemWithPriceDto request, CancellationToken cancellationToken = default)
    {
        try
        {
            var grpcRequest = new AddOrderItemRequest
            {
                OrderId = orderId,
                ProductId = request.ProductId.ToString(),
                Quantity = request.Quantity,
                UnitPrice = (double)request.LockedPrice
            };

            var response = await _grpcClient.AddOrderItemAsync(grpcRequest, cancellationToken: cancellationToken);

            if (response.ResultCase == AddOrderItemResponse.ResultOneofCase.Data)
            {
                return ServiceResult<AddOrderItemResultDto>.Success(new AddOrderItemResultDto
                {
                    OrderId = response.Data.OrderId,
                    ItemId = response.Data.ItemId
                });
            }

            return ServiceResult<AddOrderItemResultDto>.Failure(new ErrorInfo
            {
                Code = response.Error.Code,
                Message = response.Error.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding item to order {OrderId} via gRPC", orderId);
            return ServiceResult<AddOrderItemResultDto>.Failure(new ErrorInfo
            {
                Code = "GRPC_ERROR",
                Message = $"Failed to add item to order: {ex.Message}"
            });
        }
    }

    public async Task<ServiceResult<RemoveOrderItemResultDto>> RemoveOrderItem(string orderId, string itemId, CancellationToken cancellationToken = default)
    {
        try
        {
            var grpcRequest = new RemoveOrderItemRequest
            {
                OrderId = orderId,
                ItemId = itemId
            };

            var response = await _grpcClient.RemoveOrderItemAsync(grpcRequest, cancellationToken: cancellationToken);

            if (response.ResultCase == RemoveOrderItemResponse.ResultOneofCase.Data)
            {
                return ServiceResult<RemoveOrderItemResultDto>.Success(new RemoveOrderItemResultDto
                {
                    OrderId = response.Data.OrderId
                });
            }

            return ServiceResult<RemoveOrderItemResultDto>.Failure(new ErrorInfo
            {
                Code = response.Error.Code,
                Message = response.Error.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing item {ItemId} from order {OrderId} via gRPC", itemId, orderId);
            return ServiceResult<RemoveOrderItemResultDto>.Failure(new ErrorInfo
            {
                Code = "GRPC_ERROR",
                Message = $"Failed to remove item from order: {ex.Message}"
            });
        }
    }

    public async Task<ServiceResult<UpdateOrderItemQuantityResultDto>> UpdateOrderItemQuantity(
        string orderId, string itemId, int newQuantity, CancellationToken cancellationToken = default)
    {
        try
        {
            var grpcRequest = new UpdateOrderItemQuantityRequest
            {
                OrderId = orderId,
                ItemId = itemId,
                NewQuantity = newQuantity
            };

            var response = await _grpcClient.UpdateOrderItemQuantityAsync(grpcRequest, cancellationToken: cancellationToken);

            if (response.ResultCase == UpdateOrderItemQuantityResponse.ResultOneofCase.Data)
            {
                return ServiceResult<UpdateOrderItemQuantityResultDto>.Success(new UpdateOrderItemQuantityResultDto
                {
                    OrderId = response.Data.OrderId,
                    ItemId = response.Data.ItemId,
                    NewQuantity = response.Data.NewQuantity
                });
            }

            return ServiceResult<UpdateOrderItemQuantityResultDto>.Failure(new ErrorInfo
            {
                Code = response.Error.Code,
                Message = response.Error.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating item {ItemId} quantity in order {OrderId} via gRPC", itemId, orderId);
            return ServiceResult<UpdateOrderItemQuantityResultDto>.Failure(new ErrorInfo
            {
                Code = "GRPC_ERROR",
                Message = $"Failed to update item quantity: {ex.Message}"
            });
        }
    }
}