using ApiGateway.Core.Common;
using ApiGateway.Core.Order;
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

    public OrderOrchestrationService(
        ILogger<OrderOrchestrationService> logger,
        IProductServiceClient productClient,
        IUserServiceClient userClient,
        IOrderServiceClient orderClient)
    {
        _logger = logger;
        _productClient = productClient;
        _userClient = userClient;
        _orderClient = orderClient;
    }

    /// <summary>
    ///  This proces should be transactional, but for simplicity, we are not implementing distributed transactions saga here.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<ServiceResult<OrderDto>> CreateOrderAsync(
        CreateOrderRequestDto request, 
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating order for user {UserId} with {ItemCount} items", 
            request.UserId, request.Items.Count);

        // Step 1: Validate user exists
        var userResult = await _userClient.GetUserById(request.UserId.ToString());
        if (!userResult.IsSuccess || userResult.Data == null)
        {
            _logger.LogWarning("User {UserId} not found", request.UserId);
            return ServiceResult<OrderDto>.Failure(
                new ErrorInfo { Code = "USER_NOT_FOUND", Message = $"User with ID {request.UserId} does not exist" });
        }

        _logger.LogInformation("User {UserId} validated successfully", request.UserId);

        // Step 2: Validate products, get prices, and lock stock
        var orderItemsWithPrices = new List<OrderItemWithPriceDto>();

        foreach (var item in request.Items)
        {
       
            // Lock the stock
            var lockResult = await _productClient.LockProductStock(item.ProductId.ToString(), item.Quantity);
            if (!lockResult.IsSuccess)
            {
                _logger.LogWarning("Failed to lock stock for product {ProductId}: {Error}", item.ProductId, lockResult.Error?.Message);
                return ServiceResult<OrderDto>.Failure(
                    lockResult.Error ?? new ErrorInfo { Code = "STOCK_LOCK_FAILED", Message = $"Failed to lock stock for product {item.ProductId}" });
            }

            // Store item with the price from the server
            orderItemsWithPrices.Add(new OrderItemWithPriceDto
            {
                ProductId = item.ProductId,
                Quantity = lockResult.Data!.LockedQuantity,
                LockedPrice = lockResult.Data!.LockedPrice
            });
        }

        _logger.LogInformation("All {Count} products validated successfully", request.Items.Count);

        // Step 3: Create the order via Order service with server-side prices
        var orderRequestWithPrices = new CreateOrderRequestWithPricesDto
        {
            UserId = request.UserId,
            ShippingAddressId = request.ShippingAddressId,
            BillingAddressId = request.BillingAddressId,
            Items = orderItemsWithPrices
        };

        var orderResult = await _orderClient.CreateOrder(orderRequestWithPrices);
        
        if (!orderResult.IsSuccess || orderResult.Data == null)
        {
            _logger.LogError("Failed to create order: {Error}", orderResult.Error?.Message);
            var releaseTasks = request.Items.Select(item =>
                _productClient.ReleaseProductStock(item.ProductId.ToString(), item.Quantity));
            await Task.WhenAll(releaseTasks);
            _logger.LogInformation("Released locked stock for all products due to order creation failure");
            return ServiceResult<OrderDto>.Failure(
                orderResult.Error ?? new ErrorInfo { Code = "ORDER_CREATION_FAILED", Message = "Failed to create order" });
        }

        _logger.LogInformation("Order {OrderId} created successfully", orderResult.Data.OrderId);
        

        return ServiceResult<OrderDto>.Success(orderResult.Data);
    }

    public async Task<ServiceResult<OrderDto>> GetOrderByIdAsync(string orderId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting order {OrderId}", orderId);

        var orderResult = await _orderClient.GetOrderById(orderId);

        if (!orderResult.IsSuccess || orderResult.Data == null)
        {
            _logger.LogWarning("Order {OrderId} not found", orderId);
            return ServiceResult<OrderDto>.Failure(
                orderResult.Error ?? new ErrorInfo { Code = "ORDER_NOT_FOUND", Message = $"Order with ID {orderId} not found" });
        }

        // Enrich order with product names
        var enrichedOrder = await EnrichOrderWithProductNamesFromIds(orderResult.Data);

        _logger.LogInformation("Order {OrderId} retrieved successfully", orderId);
        return ServiceResult<OrderDto>.Success(enrichedOrder);
    }

    public async Task<ServiceResult<List<OrderDto>>> GetUserOrdersAsync(string userId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting orders for user {UserId}", userId);

        var ordersResult = await _orderClient.GetOrdersByUserId(userId);

        if (!ordersResult.IsSuccess || ordersResult.Data == null)
        {
            _logger.LogWarning("Failed to get orders for user {UserId}", userId);
            return ServiceResult<List<OrderDto>>.Failure(
                ordersResult.Error ?? new ErrorInfo { Code = "ORDERS_RETRIEVAL_FAILED", Message = $"Failed to get orders for user {userId}" });
        }

        // Enrich each order with product names
        var enrichedOrders = new List<OrderDto>();
        foreach (var order in ordersResult.Data)
        {
            var enrichedOrder = await EnrichOrderWithProductNamesFromIds(order);
            enrichedOrders.Add(enrichedOrder);
        }

        _logger.LogInformation("Retrieved {Count} orders for user {UserId}", enrichedOrders.Count, userId);
        return ServiceResult<List<OrderDto>>.Success(enrichedOrders);
    }

    public async Task<ServiceResult<OrderDto>> CancelOrderAsync(string orderId, string? reason = null, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Cancelling order {OrderId} with reason: {Reason}", orderId, reason ?? "No reason provided");

        var cancelResult = await _orderClient.CancelOrder(orderId, reason);
        
        //todo: release stock for cancelled order items

        if (!cancelResult.IsSuccess || cancelResult.Data == null)
        {
            _logger.LogWarning("Failed to cancel order {OrderId}", orderId);
            return ServiceResult<OrderDto>.Failure(
                cancelResult.Error ?? new ErrorInfo { Code = "ORDER_CANCELLATION_FAILED", Message = $"Failed to cancel order {orderId}" });
        }

        // Enrich order with product names
        var enrichedOrder = await EnrichOrderWithProductNamesFromIds(cancelResult.Data);

        _logger.LogInformation("Order {OrderId} cancelled successfully", orderId);
        return ServiceResult<OrderDto>.Success(enrichedOrder);
    }

    public async Task<ServiceResult<OrderDto>> UpdateOrderStatusAsync(string orderId, string newStatus, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Updating order {OrderId} status to {NewStatus}", orderId, newStatus);

        var updateResult = await _orderClient.UpdateOrderStatus(orderId, newStatus);

        if (!updateResult.IsSuccess || updateResult.Data == null)
        {
            _logger.LogWarning("Failed to update order {OrderId} status to {NewStatus}", orderId, newStatus);
            return ServiceResult<OrderDto>.Failure(
                updateResult.Error ?? new ErrorInfo { Code = "ORDER_STATUS_UPDATE_FAILED", Message = $"Failed to update order {orderId} status" });
        }

        // Enrich order with product names
        var enrichedOrder = await EnrichOrderWithProductNamesFromIds(updateResult.Data);

        _logger.LogInformation("Order {OrderId} status updated successfully to {NewStatus}", orderId, newStatus);
        return ServiceResult<OrderDto>.Success(enrichedOrder);
    }

    public async Task<ServiceResult<AddOrderItemResultDto>> AddOrderItemAsync(
        string orderId,
        AddOrderItemRequestDto request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Adding item to order {OrderId} - Product: {ProductId}, Quantity: {Quantity}",
            orderId, request.ProductId, request.Quantity);

        // Step 1: Get the order to validate it exists and check current items
        var orderResult = await _orderClient.GetOrderById(orderId);
        if (!orderResult.IsSuccess || orderResult.Data == null)
        {
            _logger.LogWarning("Order {OrderId} not found", orderId);
            return ServiceResult<AddOrderItemResultDto>.Failure(
                orderResult.Error ?? new ErrorInfo { Code = "ORDER_NOT_FOUND", Message = $"Order with ID {orderId} not found" });
        }

        var order = orderResult.Data;

        // Step 2: Check if the product is already in the order
        var existingItem = order.Items.FirstOrDefault(item => item.ProductId == request.ProductId.ToString());
        if (existingItem != null)
        {
            _logger.LogWarning("Product {ProductId} is already in order {OrderId}", request.ProductId, orderId);
            return ServiceResult<AddOrderItemResultDto>.Failure(
                new ErrorInfo
                {
                    Code = "PRODUCT_ALREADY_IN_ORDER",
                    Message = $"Product {request.ProductId} is already in the order. Use update quantity instead."
                });
        }

        // Step 3: Get product details and validate price
        var productResult = await _productClient.GetProductById(request.ProductId.ToString());
        if (!productResult.IsSuccess || productResult.Data == null)
        {
            _logger.LogWarning("Product {ProductId} not found", request.ProductId);
            return ServiceResult<AddOrderItemResultDto>.Failure(
                productResult.Error ?? new ErrorInfo { Code = "PRODUCT_NOT_FOUND", Message = $"Product with ID {request.ProductId} does not exist" });
        }

        var product = productResult.Data;
        _logger.LogInformation("Product {ProductId} validated successfully with price {Price}", request.ProductId, product.Price);

        // Step 4: Lock the stock
        var lockResult = await _productClient.LockProductStock(request.ProductId.ToString(), request.Quantity);
        if (!lockResult.IsSuccess)
        {
            _logger.LogWarning("Failed to lock stock for product {ProductId}: {Error}", request.ProductId, lockResult.Error?.Message);
            return ServiceResult<AddOrderItemResultDto>.Failure(
                lockResult.Error ?? new ErrorInfo { Code = "STOCK_LOCK_FAILED", Message = $"Failed to lock stock for product {request.ProductId}" });
        }

        _logger.LogInformation("Stock locked for product {ProductId}, quantity: {Quantity}", request.ProductId, request.Quantity);

        // Step 5: Add the item to the order with the server-validated price
        var addItemRequest = new AddOrderItemWithPriceDto
        {
            ProductId = request.ProductId,
            Quantity = request.Quantity,
            UnitPrice = (decimal)product.Price
        };

        var addResult = await _orderClient.AddOrderItem(orderId, addItemRequest);

        if (!addResult.IsSuccess || addResult.Data == null)
        {
            _logger.LogError("Failed to add item to order {OrderId}: {Error}", orderId, addResult.Error?.Message);

            // TODO: In a production system, we should implement a compensation mechanism here
            // to release the locked stock if adding the item fails

            return ServiceResult<AddOrderItemResultDto>.Failure(
                addResult.Error ?? new ErrorInfo { Code = "ADD_ITEM_FAILED", Message = "Failed to add item to order" });
        }

        _logger.LogInformation("Item added successfully to order {OrderId}, ItemId: {ItemId}", orderId, addResult.Data.ItemId);

        return ServiceResult<AddOrderItemResultDto>.Success(addResult.Data);
    }


    private async Task<OrderDto> EnrichOrderWithProductNamesFromIds(OrderDto order)
    {
        // Fetch product details in parallel
        var productTasks = order.Items.Select(async item =>
        {
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

