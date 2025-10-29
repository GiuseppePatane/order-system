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

    public async Task<ServiceResult<OrderDto>> CreateOrderAsync(
        CreateOrderRequestDto request, 
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating order for user {UserId} with {ItemCount} items", 
            request.UserId, request.Items.Count);

       
        var userResult = await _userClient.GetUserById(request.UserId.ToString());
        if (!userResult.IsSuccess || userResult.Data == null)
        {
            _logger.LogWarning("User {UserId} not found", request.UserId);
            return ServiceResult<OrderDto>.Failure(
                new ErrorInfo { Code = "USER_NOT_FOUND", Message = $"User with ID {request.UserId} does not exist" });
        }

        _logger.LogInformation("User {UserId} validated successfully", request.UserId);

       
        var productValidationTasks = request.Items.Select(async item =>
        {
            var productResult = await _productClient.GetProductById(item.ProductId.ToString());
            
            if (!productResult.IsSuccess || productResult.Data == null)
            {
                return (Success: false, 
                        Error: $"Product {item.ProductId} not found",
                        Item: item,
                        Product: (ProductDto?)null);
            }

            var product = productResult.Data;
            

            return (Success: true, Error: string.Empty, Item: item, Product: product);
        }).ToList();

        var validationResults = await Task.WhenAll(productValidationTasks);

        // Check if any product validation failed
        var failedValidation = validationResults.FirstOrDefault(r => !r.Success);
        if (failedValidation.Success == false)
        {
            _logger.LogWarning("Product validation failed: {Error}", failedValidation.Error);
            return ServiceResult<OrderDto>.Failure(
                new ErrorInfo { Code = "PRODUCT_VALIDATION_FAILED", Message = failedValidation.Error });
        }

        _logger.LogInformation("All {Count} products validated successfully", request.Items.Count);

        // Step 3: Create the order via Order service
        var orderResult = await _orderClient.CreateOrder(request);
        
        if (!orderResult.IsSuccess || orderResult.Data == null)
        {
            _logger.LogError("Failed to create order: {Error}", orderResult.Error?.Message);
            return ServiceResult<OrderDto>.Failure(
                orderResult.Error ?? new ErrorInfo { Code = "ORDER_CREATION_FAILED", Message = "Failed to create order" });
        }

        _logger.LogInformation("Order {OrderId} created successfully", orderResult.Data.OrderId);

        // Step 4: Enrich order data with product names (optional but useful for client)
        var enrichedOrder = await EnrichOrderWithProductNames(orderResult.Data, validationResults);

        return ServiceResult<OrderDto>.Success(enrichedOrder);
    }

    public Task<ServiceResult<OrderDto>> GetOrderByIdAsync(string orderId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<ServiceResult<List<OrderDto>>> GetUserOrdersAsync(string userId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<ServiceResult<OrderDto>> CancelOrderAsync(string orderId, string? reason = null, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<ServiceResult<OrderDto>> UpdateOrderStatusAsync(string orderId, string newStatus, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }


    private Task<OrderDto> EnrichOrderWithProductNames(
        OrderDto order, 
        IEnumerable<(bool Success, string Error, OrderItemDto Item, ProductDto? Product)> validationResults)
    {
        foreach (var item in order.Items)
        {
            var valueTuples = validationResults as (bool Success, string Error, OrderItemDto Item, ProductDto Product)[] ?? validationResults.ToArray();
            var productInfo = valueTuples.FirstOrDefault(v => v.Item.ProductId.ToString() == item.ProductId);
            if (productInfo.Product != null)
            {
                item.ProductName = productInfo.Product.Name;
            }
        }

        return Task.FromResult(order);
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

