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
            // Get product details to retrieve the current price
            var productResult = await _productClient.GetProductById(item.ProductId.ToString());
            if (!productResult.IsSuccess || productResult.Data == null)
            {
                _logger.LogWarning("Product {ProductId} not found", item.ProductId);
                return ServiceResult<OrderDto>.Failure(
                    productResult.Error ?? new ErrorInfo { Code = "PRODUCT_NOT_FOUND", Message = $"Product with ID {item.ProductId} does not exist" });
            }

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
                Quantity = item.Quantity,
                UnitPrice = (decimal)productResult.Data.Price
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
            return ServiceResult<OrderDto>.Failure(
                orderResult.Error ?? new ErrorInfo { Code = "ORDER_CREATION_FAILED", Message = "Failed to create order" });
        }

        _logger.LogInformation("Order {OrderId} created successfully", orderResult.Data.OrderId);
        

        return ServiceResult<OrderDto>.Success(orderResult.Data);
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

