using Grpc.Core;
using Google.Protobuf.WellKnownTypes;
using Order.Protos;
using Order.Core.Repositories;
using Order.Application.Commands.CreateOrder;
using Order.Application.Commands.UpdateOrderStatus;
using Order.Application.Commands.CancelOrder;
using Order.Application.Commands.AddOrderItem;
using Order.Application.Commands.RemoveOrderItem;
using Order.Application.Commands.UpdateOrderItemQuantity;
using Shared.GrpcInfrastructure.Base;
using Shared.Core.Domain.Errors;

namespace Order.GrpcService.Services;

public class OrderGrpcService : OrderService.OrderServiceBase
{
    private readonly ILogger<OrderGrpcService> _logger;
    private readonly IOrderReadOnlyRepository _orderRepository;
    private readonly CreateOrderHandler _createOrderHandler;
    private readonly UpdateOrderStatusHandler _updateOrderStatusHandler;
    private readonly CancelOrderHandler _cancelOrderHandler;
    private readonly AddOrderItemHandler _addOrderItemHandler;
    private readonly RemoveOrderItemHandler _removeOrderItemHandler;
    private readonly UpdateOrderItemQuantityHandler _updateOrderItemQuantityHandler;
    private readonly GrpcServiceBase<OrderGrpcService> _baseService;

    public OrderGrpcService(
        ILogger<OrderGrpcService> logger,
        IOrderReadOnlyRepository orderRepository,
        CreateOrderHandler createOrderHandler,
        UpdateOrderStatusHandler updateOrderStatusHandler,
        CancelOrderHandler cancelOrderHandler,
        AddOrderItemHandler addOrderItemHandler,
        RemoveOrderItemHandler removeOrderItemHandler,
        UpdateOrderItemQuantityHandler updateOrderItemQuantityHandler)
    {
        _logger = logger;
        _orderRepository = orderRepository;
        _createOrderHandler = createOrderHandler;
        _updateOrderStatusHandler = updateOrderStatusHandler;
        _cancelOrderHandler = cancelOrderHandler;
        _addOrderItemHandler = addOrderItemHandler;
        _removeOrderItemHandler = removeOrderItemHandler;
        _updateOrderItemQuantityHandler = updateOrderItemQuantityHandler;
        _baseService = new InternalGrpcServiceBase(logger);
    }

    public override async Task<OrderResponse> GetOrder(GetOrderRequest request, ServerCallContext context)
    {
        try
        {
            if (!_baseService.IsValidGuid(request.OrderId, out var orderId))
            {
                return new OrderResponse
                {
                    Error = _baseService.CreateInvalidArgumentError("OrderId", "invalid or missing GUID")
                };
            }

            var order = await _orderRepository.GetByIdAsync(orderId, context.CancellationToken);

            if (order == null)
            {
                return new OrderResponse
                {
                    Error = _baseService.CreateNotFoundError("Order", request.OrderId)
                };
            }

            return new OrderResponse
            {
                Data = MapToOrderData(order)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in GetOrder for OrderId: {OrderId}", request.OrderId);
            return new OrderResponse { Error = _baseService.CreateInternalError() };
        }
    }

    public override async Task<GetOrdersResponse> GetOrdersByUser(GetOrdersByUserRequest request, ServerCallContext context)
    {
        try
        {
            if (!_baseService.IsValidGuid(request.UserId, out var userId))
            {
                return new GetOrdersResponse
                {
                    Error = _baseService.CreateInvalidArgumentError("UserId", "invalid or missing GUID")
                };
            }

            var orders = await _orderRepository.GetByUserIdAsync(userId, context.CancellationToken);

            var response = new OrderListData();
            response.Items.AddRange(orders.Select(MapToOrderData));

            return new GetOrdersResponse
            {
                Data = response
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in GetOrdersByUser for UserId: {UserId}", request.UserId);
            return new GetOrdersResponse { Error = _baseService.CreateInternalError() };
        }
    }

    public override async Task<GetPagedOrdersResponse> GetPagedOrdersByUser(GetPagedOrdersRequest request, ServerCallContext context)
    {
        try
        {
            if (!_baseService.IsValidGuid(request.UserId, out var userId))
            {
                return new GetPagedOrdersResponse
                {
                    Error = _baseService.CreateInvalidArgumentError("UserId", "invalid or missing GUID")
                };
            }

            Core.Domain.OrderStatus? status = null;
            if (request.Status != OrderStatus.Unspecified)
            {
                status = MapToOrderStatus(request.Status);
            }

            var (items, totalCount) = await _orderRepository.GetPagedByUserIdAsync(
                userId,
                request.PageNumber,
                request.PageSize,
                status,
                context.CancellationToken);

            var response = new PagedOrderListData
            {
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                TotalCount = totalCount
            };
            response.Items.AddRange(items.Select(MapToOrderData));

            return new GetPagedOrdersResponse
            {
                Data = response
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in GetPagedOrdersByUser for UserId: {UserId}", request.UserId);
            return new GetPagedOrdersResponse { Error = _baseService.CreateInternalError() };
        }
    }

    public override async Task<OrderResponse> CreateOrder(CreateOrderRequest request, ServerCallContext context)
    {
        try
        {
            if (!_baseService.IsValidGuid(request.UserId, out var userId))
            {
                return new OrderResponse
                {
                    Error = _baseService.CreateInvalidArgumentError("UserId", "invalid or missing GUID")
                };
            }

            if (!_baseService.IsValidGuid(request.ShippingAddressId, out var shippingAddressId))
            {
                return new OrderResponse
                {
                    Error = _baseService.CreateInvalidArgumentError("ShippingAddressId", "invalid or missing GUID")
                };
            }

            Guid? billingAddressId = null;
            if (!string.IsNullOrWhiteSpace(request.BillingAddressId))
            {
                if (_baseService.IsValidGuid(request.BillingAddressId, out var billingId))
                {
                    billingAddressId = billingId;
                }
            }

            var items = request.Items.Select(item => new Application.Commands.CreateOrder.OrderItemInput(
                Guid.Parse(item.ProductId),
                item.Quantity,
                (decimal)item.UnitPrice
            )).ToList();

            var command = new CreateOrderCommand(userId, items, shippingAddressId, billingAddressId);
            var result = await _createOrderHandler.Handle(command, context.CancellationToken);

            if (result.IsFailure)
            {
                var error = result.Error;
                if (error is ValidationError ve)
                {
                    return new OrderResponse
                    {
                        Error = _baseService.CreateInvalidArgumentError(ve.FieldName, ve.Reason)
                    };
                }

                return new OrderResponse { Error = _baseService.CreateInternalError(error.Message) };
            }

            var createdOrder = await _orderRepository.GetByIdAsync(result.Value.OrderId, context.CancellationToken);

            return new OrderResponse
            {
                Data = MapToOrderData(createdOrder!)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in CreateOrder");
            return new OrderResponse { Error = _baseService.CreateInternalError() };
        }
    }

    public override async Task<OrderResponse> UpdateOrderStatus(UpdateOrderStatusRequest request, ServerCallContext context)
    {
        try
        {
            if (!_baseService.IsValidGuid(request.OrderId, out var orderId))
            {
                return new OrderResponse
                {
                    Error = _baseService.CreateInvalidArgumentError("OrderId", "invalid or missing GUID")
                };
            }

            var status = MapToOrderStatus(request.Status);
            var command = new UpdateOrderStatusCommand(orderId, status);
            var result = await _updateOrderStatusHandler.Handle(command, context.CancellationToken);

            if (result.IsFailure)
            {
                var error = result.Error;
                if (error is ValidationError ve)
                {
                    return new OrderResponse
                    {
                        Error = _baseService.CreateInvalidArgumentError(ve.FieldName, ve.Reason)
                    };
                }
                if (error is NotFoundError)
                {
                    return new OrderResponse
                    {
                        Error = _baseService.CreateNotFoundError("Order", request.OrderId)
                    };
                }

                return new OrderResponse { Error = _baseService.CreateInternalError(error.Message) };
            }

            var updatedOrder = await _orderRepository.GetByIdAsync(result.Value.OrderId, context.CancellationToken);

            return new OrderResponse
            {
                Data = MapToOrderData(updatedOrder!)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in UpdateOrderStatus for OrderId: {OrderId}", request.OrderId);
            return new OrderResponse { Error = _baseService.CreateInternalError() };
        }
    }

    public override async Task<OrderResponse> CancelOrder(CancelOrderRequest request, ServerCallContext context)
    {
        try
        {
            if (!_baseService.IsValidGuid(request.OrderId, out var orderId))
            {
                return new OrderResponse
                {
                    Error = _baseService.CreateInvalidArgumentError("OrderId", "invalid or missing GUID")
                };
            }

            var command = new CancelOrderCommand(orderId, request.Reason);
            var result = await _cancelOrderHandler.Handle(command, context.CancellationToken);

            if (result.IsFailure)
            {
                var error = result.Error;
                if (error is ValidationError ve)
                {
                    return new OrderResponse
                    {
                        Error = _baseService.CreateInvalidArgumentError(ve.FieldName, ve.Reason)
                    };
                }
                if (error is NotFoundError)
                {
                    return new OrderResponse
                    {
                        Error = _baseService.CreateNotFoundError("Order", request.OrderId)
                    };
                }

                return new OrderResponse { Error = _baseService.CreateInternalError(error.Message) };
            }

            var cancelledOrder = await _orderRepository.GetByIdAsync(result.Value.OrderId, context.CancellationToken);

            return new OrderResponse
            {
                Data = MapToOrderData(cancelledOrder!)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in CancelOrder for OrderId: {OrderId}", request.OrderId);
            return new OrderResponse { Error = _baseService.CreateInternalError() };
        }
    }

    private static OrderData MapToOrderData(Core.Domain.Order order)
    {
        var orderData = new OrderData
        {
            OrderId = order.Id.ToString(),
            UserId = order.UserId.ToString(),
            ShippingAddressId = order.ShippingAddressId.ToString(),
            TotalAmount = (double)order.TotalAmount,
            Status = MapFromOrderStatus(order.Status),
            CreatedAt = Timestamp.FromDateTime(DateTime.SpecifyKind(order.CreatedAt, DateTimeKind.Utc)),
            UpdatedAt = order.UpdatedAt.HasValue
                ? Timestamp.FromDateTime(DateTime.SpecifyKind(order.UpdatedAt.Value, DateTimeKind.Utc))
                : null
        };

        if (order.BillingAddressId.HasValue)
        {
            orderData.BillingAddressId = order.BillingAddressId.Value.ToString();
        }

        if (!string.IsNullOrWhiteSpace(order.CancellationReason))
        {
            orderData.CancellationReason = order.CancellationReason;
        }

        orderData.Items.AddRange(order.Items.Select(MapToOrderItemData));

        return orderData;
    }

    private static OrderItemData MapToOrderItemData(Core.Domain.OrderItem item)
    {
        return new OrderItemData
        {
            OrderItemId = item.Id.ToString(),
            ProductId = item.ProductId.ToString(),
            Quantity = item.Quantity,
            UnitPrice = (double)item.UnitPrice,
            TotalPrice = (double)item.TotalPrice
        };
    }

    private static OrderStatus MapFromOrderStatus(Core.Domain.OrderStatus status)
    {
        return status switch
        {
            Core.Domain.OrderStatus.Pending => OrderStatus.Pending,
            Core.Domain.OrderStatus.Confirmed => OrderStatus.Confirmed,
            Core.Domain.OrderStatus.Processing => OrderStatus.Processing,
            Core.Domain.OrderStatus.Shipped => OrderStatus.Shipped,
            Core.Domain.OrderStatus.Delivered => OrderStatus.Delivered,
            Core.Domain.OrderStatus.Cancelled => OrderStatus.Cancelled,
            _ => OrderStatus.Unspecified
        };
    }

    private static Core.Domain.OrderStatus MapToOrderStatus(OrderStatus status)
    {
        return status switch
        {
            OrderStatus.Pending => Core.Domain.OrderStatus.Pending,
            OrderStatus.Confirmed => Core.Domain.OrderStatus.Confirmed,
            OrderStatus.Processing => Core.Domain.OrderStatus.Processing,
            OrderStatus.Shipped => Core.Domain.OrderStatus.Shipped,
            OrderStatus.Delivered => Core.Domain.OrderStatus.Delivered,
            OrderStatus.Cancelled => Core.Domain.OrderStatus.Cancelled,
            _ => Core.Domain.OrderStatus.Pending
        };
    }

    public override async Task<AddOrderItemResponse> AddOrderItem(AddOrderItemRequest request, ServerCallContext context)
    {
        try
        {
            if (!_baseService.IsValidGuid(request.OrderId, out var orderId))
            {
                return new AddOrderItemResponse
                {
                    Error = _baseService.CreateInvalidArgumentError("OrderId", "invalid or missing GUID")
                };
            }

            if (!_baseService.IsValidGuid(request.ProductId, out var productId))
            {
                return new AddOrderItemResponse
                {
                    Error = _baseService.CreateInvalidArgumentError("ProductId", "invalid or missing GUID")
                };
            }

            var command = new AddOrderItemCommand(orderId, productId, request.Quantity, (decimal)request.UnitPrice);
            var result = await _addOrderItemHandler.Handle(command, context.CancellationToken);

            if (result.IsFailure)
            {
                var error = result.Error;
                if (error is ValidationError ve)
                {
                    return new AddOrderItemResponse
                    {
                        Error = _baseService.CreateInvalidArgumentError(ve.FieldName, ve.Reason)
                    };
                }
                if (error is NotFoundError)
                {
                    return new AddOrderItemResponse
                    {
                        Error = _baseService.CreateNotFoundError("Order", request.OrderId)
                    };
                }

                return new AddOrderItemResponse { Error = _baseService.CreateInternalError(error.Message) };
            }

            return new AddOrderItemResponse
            {
                Data = new AddOrderItemData
                {
                    OrderId = result.Value.OrderId.ToString(),
                    ItemId = result.Value.ItemId.ToString()
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in AddOrderItem for OrderId: {OrderId}", request.OrderId);
            return new AddOrderItemResponse { Error = _baseService.CreateInternalError() };
        }
    }

    public override async Task<RemoveOrderItemResponse> RemoveOrderItem(RemoveOrderItemRequest request, ServerCallContext context)
    {
        try
        {
            if (!_baseService.IsValidGuid(request.OrderId, out var orderId))
            {
                return new RemoveOrderItemResponse
                {
                    Error = _baseService.CreateInvalidArgumentError("OrderId", "invalid or missing GUID")
                };
            }

            if (!_baseService.IsValidGuid(request.ItemId, out var itemId))
            {
                return new RemoveOrderItemResponse
                {
                    Error = _baseService.CreateInvalidArgumentError("ItemId", "invalid or missing GUID")
                };
            }

            var command = new RemoveOrderItemCommand(orderId, itemId);
            var result = await _removeOrderItemHandler.Handle(command, context.CancellationToken);

            if (result.IsFailure)
            {
                var error = result.Error;
                if (error is ValidationError ve)
                {
                    return new RemoveOrderItemResponse
                    {
                        Error = _baseService.CreateInvalidArgumentError(ve.FieldName, ve.Reason)
                    };
                }
                if (error is NotFoundError nfe)
                {
                    return new RemoveOrderItemResponse
                    {
                        Error = _baseService.CreateNotFoundError(nfe.EntityType, nfe.EntityId)
                    };
                }

                return new RemoveOrderItemResponse { Error = _baseService.CreateInternalError(error.Message) };
            }

            return new RemoveOrderItemResponse
            {
                Data = new RemoveOrderItemData
                {
                    OrderId = result.Value.OrderId.ToString()
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in RemoveOrderItem for OrderId: {OrderId}, ItemId: {ItemId}",
                request.OrderId, request.ItemId);
            return new RemoveOrderItemResponse { Error = _baseService.CreateInternalError() };
        }
    }

    public override async Task<UpdateOrderItemQuantityResponse> UpdateOrderItemQuantity(
        UpdateOrderItemQuantityRequest request, ServerCallContext context)
    {
        try
        {
            if (!_baseService.IsValidGuid(request.OrderId, out var orderId))
            {
                return new UpdateOrderItemQuantityResponse
                {
                    Error = _baseService.CreateInvalidArgumentError("OrderId", "invalid or missing GUID")
                };
            }

            if (!_baseService.IsValidGuid(request.ItemId, out var itemId))
            {
                return new UpdateOrderItemQuantityResponse
                {
                    Error = _baseService.CreateInvalidArgumentError("ItemId", "invalid or missing GUID")
                };
            }

            var command = new UpdateOrderItemQuantityCommand(orderId, itemId, request.NewQuantity);
            var result = await _updateOrderItemQuantityHandler.Handle(command, context.CancellationToken);

            if (result.IsFailure)
            {
                var error = result.Error;
                if (error is ValidationError ve)
                {
                    return new UpdateOrderItemQuantityResponse
                    {
                        Error = _baseService.CreateInvalidArgumentError(ve.FieldName, ve.Reason)
                    };
                }
                if (error is NotFoundError nfe)
                {
                    return new UpdateOrderItemQuantityResponse
                    {
                        Error = _baseService.CreateNotFoundError(nfe.EntityType, nfe.EntityId)
                    };
                }

                return new UpdateOrderItemQuantityResponse { Error = _baseService.CreateInternalError(error.Message) };
            }

            return new UpdateOrderItemQuantityResponse
            {
                Data = new UpdateOrderItemQuantityData
                {
                    OrderId = result.Value.OrderId.ToString(),
                    ItemId = result.Value.ItemId.ToString(),
                    NewQuantity = result.Value.NewQuantity
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in UpdateOrderItemQuantity for OrderId: {OrderId}, ItemId: {ItemId}",
                request.OrderId, request.ItemId);
            return new UpdateOrderItemQuantityResponse { Error = _baseService.CreateInternalError() };
        }
    }

    private class InternalGrpcServiceBase : GrpcServiceBase<OrderGrpcService>
    {
        public InternalGrpcServiceBase(ILogger<OrderGrpcService> logger) : base(logger)
        {
        }
    }
}
