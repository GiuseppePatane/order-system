using ApiGateway.Core.Address;
using ApiGateway.Core.Common;
using ApiGateway.Core.Order;
using ApiGateway.Core.Order.Dto;
using ApiGateway.Core.Product;
using ApiGateway.Core.User;
using Microsoft.Extensions.Logging;

namespace ApiGateway.infrastructure.GrcpClient.Order;

/// <summary>
/// Orchestrates order creation by coordinating User, Product, and Order services
/// </summary>
public class OrderOrchestrationService : IOrderOrchestrationService
{
    private readonly ILogger<OrderOrchestrationService> _logger;
    private readonly IProductServiceClient _productClient;
    private readonly IUserServiceClient _userClient;
    private readonly IOrderServiceClient _orderClient;
    private readonly IAddressServiceClient _addressServiceClient;

    const int MaxItemsPerOrder = 100;

    public OrderOrchestrationService(
        ILogger<OrderOrchestrationService> logger,
        IProductServiceClient productClient,
        IUserServiceClient userClient,
        IOrderServiceClient orderClient,
        IAddressServiceClient addressServiceClient
    )
    {
        _logger = logger;
        _productClient = productClient;
        _userClient = userClient;
        _orderClient = orderClient;
        _addressServiceClient = addressServiceClient;
    }

    /// <summary>
    ///  This proces should be transactional, but for simplicity, we are not implementing distributed transactions saga here.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<ServiceResult<OrderMutationResponseDto>> CreateOrderAsync(
        CreateOrderRequestDto request,
        CancellationToken cancellationToken = default
    )
    {
        var correlationId = Guid.NewGuid();
        using var scope = _logger.BeginScope(
            new Dictionary<string, object>
            {
                ["CorrelationId"] = correlationId,
                ["UserId"] = request.UserId,
            }
        );

        _logger.LogInformation(
            "Creating order for user {UserId} with first product {ProductId}",
            request.UserId,
            request.FirstItem.ProductId
        );

        // Step 1: Validate user exists

        var userValidation = await ValidateUserExistsAsync(request.UserId, cancellationToken);
        if (!userValidation.IsSuccess)
        {
            return ServiceResult<OrderMutationResponseDto>.Failure(userValidation.Error!);
        }

        _logger.LogInformation("User {UserId} validated successfully", request.UserId);

        var addressValidation = await ValidateAddressesAsync(request, cancellationToken);
        if (!addressValidation.IsSuccess)
        {
            return ServiceResult<OrderMutationResponseDto>.Failure(addressValidation.Error!);
        }

        var businessRulesValidation = ValidateBusinessRules(request.FirstItem);
        if (!businessRulesValidation.IsSuccess)
        {
            return ServiceResult<OrderMutationResponseDto>.Failure(businessRulesValidation.Error!);
        }

        var stockLockResult = await LockStockAndGetPrice(request.FirstItem, cancellationToken);
        if (!stockLockResult.IsSuccess)
        {
            return ServiceResult<OrderMutationResponseDto>.Failure(stockLockResult.Error!);
        }

        var orderItemsWithPrices = new List<OrderItemWithPriceDto>() { stockLockResult.Data! };

        var orderRequestWithPrices = new CreateOrderRequestWithPricesDto
        {
            UserId = request.UserId,
            ShippingAddressId = request.ShippingAddressId,
            BillingAddressId = request.BillingAddressId,
            Items = orderItemsWithPrices,
        };

        var orderResult = await _orderClient.CreateOrder(orderRequestWithPrices);

        if (!orderResult.IsSuccess || orderResult.Data == null)
        {
            _logger.LogError("Failed to create order: {Error}", orderResult.Error?.Message);

            await ReleaseProductStock(request.FirstItem);

            return ServiceResult<OrderMutationResponseDto>.Failure(
                orderResult.Error
                    ?? new ErrorInfo
                    {
                        Code = "ORDER_CREATION_FAILED",
                        Message = "Failed to create order",
                    }
            );
        }

        _logger.LogInformation("Order {OrderId} created successfully", orderResult.Data.OrderId);

        return ServiceResult<OrderMutationResponseDto>.Success(
            new OrderMutationResponseDto(orderResult.Data.OrderId, orderResult.Data.Status)
        );
    }

    private ServiceResult<string> ValidateBusinessRules(OrderItemDto item)
    {
        if (item.Quantity <= 0)
        {
            _logger.LogWarning("Order item quantity must be greater than zero");
            return ServiceResult<string>.Failure(
                new ErrorInfo
                {
                    Code = "INVALID_QUANTITY",
                    Message = "Item quantity must be greater than zero",
                }
            );
        }

        if (item.Quantity > MaxItemsPerOrder)
        {
            _logger.LogWarning("Order exceeds maximum items limit: {Count}", item.Quantity);
            return ServiceResult<string>.Failure(
                new ErrorInfo
                {
                    Code = "TOO_MANY_ITEMS",
                    Message = "Maximum 100 items allowed per order",
                }
            );
        }
        return ServiceResult<string>.Success("Business rules validated");
    }

    private async Task<ServiceResult<string>> ReleaseProductStock(OrderItemDto orderItem)
    {
        var releaseProductStockResult = await _productClient.ReleaseProductStock(
            orderItem.ProductId.ToString(),
            orderItem.Quantity
        );

        if (!releaseProductStockResult.IsSuccess)
        {
            _logger.LogCritical(
                "Failed to release locked stock for product {ProductId}: {Error}",
                orderItem.ProductId,
                releaseProductStockResult.Error?.Message
            );
            ///  Servirebbero dei meccanismi di compensazione più sofisticati in un sistema reale
            ///  come code di retry, alerting, o persino intervento manuale.
            ///  Per semplicità,  mi limito a loggare un errore critico.
            _logger.LogCritical(
                "Manual intervention may be required to reconcile stock for product {ProductId}",
                orderItem.ProductId
            );
            return ServiceResult<string>.Failure(
                releaseProductStockResult.Error
                    ?? new ErrorInfo
                    {
                        Code = "STOCK_RELEASE_FAILED",
                        Message =
                            $"Failed to release locked stock for product {orderItem.ProductId}",
                    }
            );
        }

        _logger.LogInformation(
            "Released locked stock for all products due to order creation failure"
        );

        return ServiceResult<string>.Success("Stock released successfully");
    }

    private async Task<ServiceResult<OrderItemWithPriceDto>> LockStockAndGetPrice(
        OrderItemDto item,
        CancellationToken cancellationToken
    )
    {
        var lockResult = await _productClient.LockProductStock(
            item.ProductId.ToString(),
            item.Quantity,
            cancellationToken
        );
        if (!lockResult.IsSuccess)
        {
            _logger.LogWarning(
                "Failed to lock stock for product {ProductId}: {Error}",
                item.ProductId,
                lockResult.Error?.Message
            );
            return ServiceResult<OrderItemWithPriceDto>.Failure(
                lockResult.Error
                    ?? new ErrorInfo
                    {
                        Code = "STOCK_LOCK_FAILED",
                        Message = $"Failed to lock stock for product {item.ProductId}",
                    }
            );
        }
        _logger.LogInformation(
            "Stock locked for product {ProductId}, quantity: {Quantity}",
            item.ProductId,
            item.Quantity
        );

        return ServiceResult<OrderItemWithPriceDto>.Success(
            new OrderItemWithPriceDto
            {
                ProductId = item.ProductId,
                Quantity = lockResult.Data!.LockedQuantity,
                LockedPrice = lockResult.Data!.LockedPrice,
            }
        );
    }

    private async Task<ServiceResult<string>> ValidateAddressesAsync(
        CreateOrderRequestDto request,
        CancellationToken cancellationToken
    )
    {
        var shippingAddressResult = await _addressServiceClient.GetAddressById(
            request.ShippingAddressId.ToString(),
            cancellationToken
        );
        if (!shippingAddressResult.IsSuccess)
        {
            _logger.LogWarning("Shipping address {AddressId} not found", request.ShippingAddressId);
            return ServiceResult<string>.Failure(
                shippingAddressResult.Error
                    ?? new ErrorInfo
                    {
                        Code = "SHIPPING_ADDRESS_NOT_FOUND",
                        Message = $"Shipping address with ID {request.ShippingAddressId} not found",
                    }
            );
        }

        if (shippingAddressResult?.Data?.UserId != request.UserId.ToString())
        {
            _logger.LogWarning(
                "Shipping address {AddressId} does not belong to user {UserId}",
                request.ShippingAddressId,
                request.UserId
            );
            return ServiceResult<string>.Failure(
                new ErrorInfo
                {
                    Code = "SHIPPING_ADDRESS_USER_MISMATCH",
                    Message =
                        $"Shipping address with ID {request.ShippingAddressId} does not belong to user {request.UserId}",
                }
            );
        }
        if (request.BillingAddressId != null)
        {
            var billingAddressResult = await _addressServiceClient.GetAddressById(
                request.BillingAddressId.ToString()!
            );
            if (!billingAddressResult.IsSuccess)
            {
                _logger.LogWarning(
                    "Billing address {AddressId} not found",
                    request.BillingAddressId
                );
                return ServiceResult<string>.Failure(
                    billingAddressResult.Error
                        ?? new ErrorInfo
                        {
                            Code = "BILLING_ADDRESS_NOT_FOUND",
                            Message =
                                $"Billing address with ID {request.BillingAddressId} not found",
                        }
                );
            }

            if (billingAddressResult?.Data?.UserId != request.UserId.ToString())
            {
                _logger.LogWarning(
                    "Billing address {AddressId} does not belong to user {UserId}",
                    request.BillingAddressId,
                    request.UserId
                );
                return ServiceResult<string>.Failure(
                    new ErrorInfo
                    {
                        Code = "BILLING_ADDRESS_USER_MISMATCH",
                        Message =
                            $"Billing address with ID {request.BillingAddressId} does not belong to user {request.UserId}",
                    }
                );
            }
        }
        _logger.LogInformation(
            "Addresses validated successfully for user {UserId}",
            request.UserId
        );
        return ServiceResult<string>.Success("Addresses are valid");
    }

    private async Task<ServiceResult<string>> ValidateUserExistsAsync(
        Guid requestUserId,
        CancellationToken cancellationToken
    )
    {
        _logger.LogDebug("Validating existence of user {UserId}", requestUserId);
        var userResult = await _userClient.GetUserById(requestUserId.ToString(), cancellationToken);

        if (!userResult.IsSuccess || userResult.Data == null)
        {
            _logger.LogWarning("User {UserId} not found", requestUserId);
            return ServiceResult<string>.Failure(
                userResult.Error
                    ?? new ErrorInfo
                    {
                        Code = "USER_NOT_FOUND",
                        Message = $"User with ID {requestUserId} not found",
                    }
            );
        }
        _logger.LogDebug("User {UserId} exists", requestUserId);
        return ServiceResult<string>.Success("User exists");
    }

    public async Task<ServiceResult<OrderDto>> GetOrderByIdAsync(
        string orderId,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogInformation("Getting order {OrderId}", orderId);

        var orderResult = await _orderClient.GetOrderById(orderId, cancellationToken);

        if (!orderResult.IsSuccess || orderResult.Data == null)
        {
            _logger.LogWarning("Order {OrderId} not found", orderId);
            return ServiceResult<OrderDto>.Failure(
                orderResult.Error
                    ?? new ErrorInfo
                    {
                        Code = "ORDER_NOT_FOUND",
                        Message = $"Order with ID {orderId} not found",
                    }
            );
        }

        var enrichedOrder = await EnrichOrderWithProductNamesFromIds(orderResult.Data);

        _logger.LogInformation("Order {OrderId} retrieved successfully", orderId);
        return ServiceResult<OrderDto>.Success(enrichedOrder);
    }

    public async Task<ServiceResult<List<OrderDto>>> GetUserOrdersAsync(
        string userId,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogInformation("Getting orders for user {UserId}", userId);

        var ordersResult = await _orderClient.GetOrdersByUserId(userId, cancellationToken);

        if (!ordersResult.IsSuccess || ordersResult.Data == null)
        {
            _logger.LogWarning("Failed to get orders for user {UserId}", userId);
            return ServiceResult<List<OrderDto>>.Failure(
                ordersResult.Error
                    ?? new ErrorInfo
                    {
                        Code = "ORDERS_RETRIEVAL_FAILED",
                        Message = $"Failed to get orders for user {userId}",
                    }
            );
        }

        var enrichedOrders = new List<OrderDto>();
        foreach (var order in ordersResult.Data)
        {
            var enrichedOrder = await EnrichOrderWithProductNamesFromIds(order);
            enrichedOrders.Add(enrichedOrder);
        }

        _logger.LogInformation(
            "Retrieved {Count} orders for user {UserId}",
            enrichedOrders.Count,
            userId
        );
        return ServiceResult<List<OrderDto>>.Success(enrichedOrders);
    }

    public async Task<ServiceResult<OrderMutationResponseDto>> CancelOrderAsync(
        string orderId,
        string? reason = null,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogInformation(
            "Cancelling order {OrderId} with reason: {Reason}",
            orderId,
            reason ?? "No reason provided"
        );

        var order = await _orderClient.GetOrderById(orderId);
        if (!order.IsSuccess || order.Data == null)
        {
            _logger.LogWarning("Order {OrderId} not found", orderId);
            return ServiceResult<OrderMutationResponseDto>.Failure(
                order.Error
                    ?? new ErrorInfo
                    {
                        Code = "ORDER_NOT_FOUND",
                        Message = $"Order with ID {orderId} not found",
                    }
            );
        }

        var cancelResult = await _orderClient.CancelOrder(orderId, reason);

        if (!cancelResult.IsSuccess || cancelResult.Data == null)
        {
            _logger.LogWarning("Failed to cancel order {OrderId}", orderId);
            return ServiceResult<OrderMutationResponseDto>.Failure(
                cancelResult.Error
                    ?? new ErrorInfo
                    {
                        Code = "ORDER_CANCELLATION_FAILED",
                        Message = $"Failed to cancel order {orderId}",
                    }
            );
        }

        foreach (var item in order.Data.Items)
        {
            await ReleaseProductStock(
                new OrderItemDto
                {
                    ProductId = Guid.Parse(item.ProductId),
                    Quantity = item.Quantity,
                }
            );
        }

        _logger.LogInformation("Order {OrderId} cancelled successfully", orderId);
        return ServiceResult<OrderMutationResponseDto>.Success(
            new OrderMutationResponseDto(cancelResult.Data.OrderId, cancelResult.Data.Status)
        );
    }

    public async Task<ServiceResult<OrderMutationResponseDto>> UpdateOrderStatusAsync(
        string orderId,
        string newStatus,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogInformation(
            "Updating order {OrderId} status to {NewStatus}",
            orderId,
            newStatus
        );

        var updateResult = await _orderClient.UpdateOrderStatus(orderId, newStatus);

        if (!updateResult.IsSuccess || updateResult.Data == null)
        {
            _logger.LogWarning(
                "Failed to update order {OrderId} status to {NewStatus}",
                orderId,
                newStatus
            );
            return ServiceResult<OrderMutationResponseDto>.Failure(
                updateResult.Error
                    ?? new ErrorInfo
                    {
                        Code = "ORDER_STATUS_UPDATE_FAILED",
                        Message = $"Failed to update order {orderId} status",
                    }
            );
        }

        // Enrich order with product names
        var enrichedOrder = await EnrichOrderWithProductNamesFromIds(updateResult.Data);

        _logger.LogInformation(
            "Order {OrderId} status updated successfully to {NewStatus}",
            orderId,
            newStatus
        );
        return ServiceResult<OrderMutationResponseDto>.Success(
            new OrderMutationResponseDto(enrichedOrder.OrderId, newStatus)
        );
    }

    public async Task<ServiceResult<AddOrderItemResultDto>> AddOrderItemAsync(
        string orderId,
        OrderItemDto request,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogInformation(
            "Adding item to order {OrderId} - Product: {ProductId}, Quantity: {Quantity}",
            orderId,
            request.ProductId,
            request.Quantity
        );

        var orderResult = await _orderClient.GetOrderById(orderId);
        if (!orderResult.IsSuccess || orderResult.Data == null)
        {
            _logger.LogWarning("Order {OrderId} not found", orderId);
            return ServiceResult<AddOrderItemResultDto>.Failure(
                orderResult.Error
                    ?? new ErrorInfo
                    {
                        Code = "ORDER_NOT_FOUND",
                        Message = $"Order with ID {orderId} not found",
                    }
            );
        }

        var order = orderResult.Data;

        var existingItem = order.Items.FirstOrDefault(item =>
            item.ProductId == request.ProductId.ToString()
        );
        if (existingItem != null)
        {
            _logger.LogWarning(
                "Product {ProductId} is already in order {OrderId}",
                request.ProductId,
                orderId
            );
            return ServiceResult<AddOrderItemResultDto>.Failure(
                new ErrorInfo
                {
                    Code = "PRODUCT_ALREADY_IN_ORDER",
                    Message =
                        $"Product {request.ProductId} is already in the order. Use update quantity instead.",
                }
            );
        }
        var businessRulesValidation = ValidateBusinessRules(request);
        if (!businessRulesValidation.IsSuccess)
        {
            return ServiceResult<AddOrderItemResultDto>.Failure(businessRulesValidation.Error!);
        }
        var orderItemResult = await LockStockAndGetPrice(request, cancellationToken);
        if (!orderItemResult.IsSuccess || orderItemResult.Data == null)
        {
            _logger.LogWarning(
                "Failed to lock stock for product {ProductId} when adding to order {OrderId}",
                request.ProductId,
                orderId
            );
            return ServiceResult<AddOrderItemResultDto>.Failure(
                orderItemResult.Error
                    ?? new ErrorInfo
                    {
                        Code = "STOCK_LOCK_FAILED",
                        Message = $"Failed to lock stock for product {request.ProductId}",
                    }
            );
        }

        var addResult = await _orderClient.AddOrderItem(orderId, orderItemResult.Data);

        if (!addResult.IsSuccess || addResult.Data == null)
        {
            _logger.LogError(
                "Failed to add item to order {OrderId}: {Error}",
                orderId,
                addResult.Error?.Message
            );

            await ReleaseProductStock(request);

            return ServiceResult<AddOrderItemResultDto>.Failure(
                addResult.Error
                    ?? new ErrorInfo
                    {
                        Code = "ADD_ITEM_FAILED",
                        Message = "Failed to add item to order",
                    }
            );
        }

        _logger.LogInformation(
            "Item added successfully to order {OrderId}, ItemId: {ItemId}",
            orderId,
            addResult.Data.ItemId
        );

        return ServiceResult<AddOrderItemResultDto>.Success(addResult.Data);
    }
   public async Task<ServiceResult<UpdateOrderItemQuantityResultDto>> UpdateOrderItemQuantity(
        string orderId,
        string itemId,
        int requestQuantity,
        CancellationToken cancellationToken = default
    )
    {
        
        _logger.LogInformation(
            "Updating quantity for item {ItemId} in order {OrderId} to {NewQuantity}",
            itemId,
            orderId,
            requestQuantity
        );
        var order = await _orderClient.GetOrderById(orderId, cancellationToken);
        if (!order.IsSuccess || order.Data == null)
        {
            _logger.LogWarning("Order {OrderId} not found", orderId);
            return ServiceResult<UpdateOrderItemQuantityResultDto>.Failure(
                order.Error
                    ?? new ErrorInfo
                    {
                        Code = "ORDER_NOT_FOUND",
                        Message = $"Order with ID {orderId} not found",
                    }
            );
        }
        _logger.LogInformation("Order {OrderId} retrieved successfully", orderId);
        var orderItem = order.Data.Items.FirstOrDefault(i => i.OrderItemId == itemId);
        if (orderItem == null)
        {
            _logger.LogWarning("Item {ItemId} not found in order {OrderId}", itemId, orderId);
            return ServiceResult<UpdateOrderItemQuantityResultDto>.Failure(
                new ErrorInfo
                {
                    Code = "ORDER_ITEM_NOT_FOUND",
                    Message = $"Order item with ID {itemId} not found in order {orderId}",
                }
            );
        }
        _logger.LogInformation(
            "Order item {ItemId} retrieved successfully from order {OrderId}",
            itemId,
            orderId
        );;
        var oldQty = orderItem.Quantity;
        var diff = requestQuantity - oldQty;
        _logger.LogInformation(
            "Calculated quantity difference for item {ItemId} in order {OrderId}: OldQty={OldQty}, NewQty={NewQty}, Diff={Diff}",
            itemId,
            orderId,
            oldQty,
            requestQuantity,
            diff
        );
        var orderItemDto = new OrderItemDto
        {
            ProductId = Guid.Parse(orderItem.ProductId),
            Quantity = requestQuantity,
        };

        var businessRulesValidation = ValidateBusinessRules(orderItemDto);

        if (!businessRulesValidation.IsSuccess)
        {
            _logger.LogWarning(
                "Business rules validation failed for item {ItemId} in order {OrderId}: {Error}",
                itemId,
                orderId,
                businessRulesValidation.Error?.Message
            );
            return ServiceResult<UpdateOrderItemQuantityResultDto>.Failure(
                businessRulesValidation.Error!
            );
        }

        if (diff > 0)
        {
            orderItemDto.Quantity = diff;
            _logger.LogInformation(
                "Locking additional {diff} stock for product {ProductId} due to quantity increase from {OldQty} to {NewQty}",
                diff,
                orderItemDto.ProductId,
                oldQty,
                requestQuantity
            );
          
            var lockQuantityResult = await LockStockAndGetPrice(orderItemDto, cancellationToken);
            if (!lockQuantityResult.IsSuccess)
            {
                return ServiceResult<UpdateOrderItemQuantityResultDto>.Failure(
                    lockQuantityResult.Error!
                );
            }
            _logger.LogInformation(
                "Updating order item {ItemId} quantity in order {OrderId} to {NewQuantity}",
                itemId,
                orderId,
                requestQuantity
            );
            var updateResult = await _orderClient.UpdateOrderItemQuantity(
                orderId,
                itemId,
                requestQuantity,
                cancellationToken
            );
            if (!updateResult.IsSuccess)
            {
                await ReleaseProductStock(orderItemDto);
                return ServiceResult<UpdateOrderItemQuantityResultDto>.Failure(updateResult.Error!);
            }
            
            _logger.LogInformation(
                "Order item {ItemId} quantity in order {OrderId} updated successfully to {NewQuantity}",
                itemId,
                orderId,
                requestQuantity
            );
            
            return ServiceResult<UpdateOrderItemQuantityResultDto>.Success(updateResult.Data!);
        }

        _logger.LogInformation(
            "Releasing stock for product {ProductId} due to quantity decrease",
            orderItemDto.ProductId
        );
        orderItemDto.Quantity = -diff;
        
        _logger.LogInformation(
            "Releasing {Quantity} stock for product {ProductId} due to quantity decrease from {OldQty} to {NewQty}",
            orderItemDto.Quantity,
            orderItemDto.ProductId,
            oldQty,
            requestQuantity
        );;

        var releaseProductResult = await ReleaseProductStock(orderItemDto);
        if (!releaseProductResult.IsSuccess)
        {
            _logger.LogWarning(
                "Failed to release stock for product {ProductId} when decreasing quantity in order {OrderId}",
                orderItemDto.ProductId,
                orderId
            );
            return ServiceResult<UpdateOrderItemQuantityResultDto>.Failure(
                releaseProductResult.Error!
            );
        }
        _logger.LogInformation(
            "Updating order item {ItemId} quantity in order {OrderId} to {NewQuantity}",
            itemId,
            orderId,
            requestQuantity
        );
        var  updateOrderItemQuantityResult = await _orderClient.UpdateOrderItemQuantity(
            orderId,
            itemId,
            requestQuantity,
            cancellationToken
        );
        
        if (!updateOrderItemQuantityResult.IsSuccess)
        {
            _logger.LogWarning(
                "Failed to update quantity for item {ItemId} in order {OrderId} to {NewQuantity}",
                itemId,
                orderId,
                requestQuantity
            );
            return ServiceResult<UpdateOrderItemQuantityResultDto>.Failure(
                updateOrderItemQuantityResult.Error!
            );
        }
        _logger.LogInformation(
            "Order item {ItemId} quantity in order {OrderId} updated successfully to {NewQuantity}",
            itemId,
            orderId,
            requestQuantity
        );;
        return ServiceResult<UpdateOrderItemQuantityResultDto>.Success(
            updateOrderItemQuantityResult.Data!
        );
    }

 
    public async Task<ServiceResult<RemoveOrderItemResultDto>> RemoveOrderItem(string orderId, string itemId,CancellationToken cancellationToken = default)
    {
        var order = await _orderClient.GetOrderById(orderId, cancellationToken);
        if (!order.IsSuccess || order.Data == null)
        {
            _logger.LogWarning("Order {OrderId} not found", orderId);
            return ServiceResult<RemoveOrderItemResultDto>.Failure(
                order.Error
                ?? new ErrorInfo
                {
                    Code = "ORDER_NOT_FOUND",
                    Message = $"Order with ID {orderId} not found",
                }
            );
        }
        var orderItem = order.Data.Items.FirstOrDefault(i => i.OrderItemId == itemId);
        if (orderItem == null)
        {
            _logger.LogWarning("Item {ItemId} not found in order {OrderId}", itemId, orderId);
            return ServiceResult<RemoveOrderItemResultDto>.Failure(
                new ErrorInfo
                {
                    Code = "ORDER_ITEM_NOT_FOUND",
                    Message = $"Order item with ID {itemId} not found in order {orderId}",
                }
            );
        }
    
        var removeResult = await _orderClient.RemoveOrderItem(orderId, itemId, cancellationToken);
        if (!removeResult.IsSuccess || removeResult.Data == null)
        {
            _logger.LogWarning("Failed to remove item {ItemId} from order {OrderId}", itemId, orderId);
            return ServiceResult<RemoveOrderItemResultDto>.Failure(
                removeResult.Error
                ?? new ErrorInfo
                {
                    Code = "REMOVE_ITEM_FAILED",
                    Message = $"Failed to remove item {itemId} from order {orderId}"

                });
        }

        var orderItemDto = new OrderItemDto
        {
            ProductId = Guid.Parse(orderItem.ProductId),
            Quantity = orderItem.Quantity,
        };
        
         await ReleaseProductStock(orderItemDto); 
  
        _logger.LogInformation("Item {ItemId} removed successfully from order {OrderId}", itemId, orderId);
        return ServiceResult<RemoveOrderItemResultDto>.Success(removeResult.Data);
         
    }

    private async Task<OrderDto> EnrichOrderWithProductNamesFromIds(OrderDto order)
    {
        var productTasks = order.Items.Select(async item =>
        {
            // potrebbe essere ottimizzato con una chiamata batch, per adesso va bene cosi.
            var productResult = await _productClient.GetProductById(item.ProductId);
            return (Item: item, Product: productResult.Data);
        });

        var productResults = await Task.WhenAll(productTasks);

        foreach (var (item, product) in productResults)
        {
            if (product != null)
            {
                item.ProductName = product.Name;
            }
        }

        return order;
    }
}
