using Microsoft.EntityFrameworkCore;
using Order.Infrastructure.EF;
using Order.IntegrationTest.Fixtures;
using Order.IntegrationTest.Infrastructure;
using Order.Protos;
using Shouldly;
using Xunit.Abstractions;
using CoreOrderStatus = Order.Core.Domain.OrderStatus;

namespace Order.IntegrationTest;

[Collection("OrderGrpc")]
public class OrderGrpcServiceTests : IAsyncLifetime
{
    private readonly OrderIntegrationTestFactory _factory;
    private OrderService.OrderServiceClient _grpcClient = null!;

    public OrderGrpcServiceTests(ITestOutputHelper testOutputHelper)
    {
        _factory = new OrderIntegrationTestFactory(testOutputHelper);
    }

    public async Task InitializeAsync()
    {
        await _factory.InitializeAsync();
        _grpcClient = new OrderService.OrderServiceClient(_factory.Channel);
        await _factory.ResetDatabaseAsync();
    }

    public async Task DisposeAsync()
    {
        await _factory.DisposeAsync();
    }

    [Fact]
    public async Task GetOrder_WithExistingId_ShouldReturnOrder()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var shippingAddressId = Guid.NewGuid();
        Guid orderId = Guid.Empty;

        await _factory.SeedDatabaseAsync(context =>
        {
            var order = TestDataGenerator.CreateOrder(userId, shippingAddressId);
            context.Orders.Add(order);
            context.SaveChanges();
            orderId = order.Id;
        });

        var request = new GetOrderRequest { OrderId = orderId.ToString() };

        // Act
        var response = await _grpcClient.GetOrderAsync(request);

        // Assert
        response.ShouldNotBeNull();
        response.ResultCase.ShouldBe(OrderResponse.ResultOneofCase.Data);
        response.Data.OrderId.ShouldBe(orderId.ToString());
        response.Data.UserId.ShouldBe(userId.ToString());
        response.Data.ShippingAddressId.ShouldBe(shippingAddressId.ToString());
        response.Data.Status.ShouldBe(Protos.OrderStatus.Pending);
        response.Data.Items.Count.ShouldBeGreaterThan(0);
        response.Data.TotalAmount.ShouldBeGreaterThan(0);
    }

    [Fact]
    public async Task GetOrdersByUser_WithExistingOrders_ShouldReturnOrders()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var shippingAddressId = Guid.NewGuid();

        await _factory.SeedDatabaseAsync(context =>
        {
            var orders = TestDataGenerator.CreateOrders(userId, shippingAddressId, null, 5);
            context.Orders.AddRange(orders);
            context.SaveChanges();
        });

        var request = new GetOrdersByUserRequest { UserId = userId.ToString() };

        // Act
        var response = await _grpcClient.GetOrdersByUserAsync(request);

        // Assert
        response.ShouldNotBeNull();
        response.ResultCase.ShouldBe(GetOrdersResponse.ResultOneofCase.Data);
        response.Data.Items.Count.ShouldBe(5);
        response.Data.Items.All(o => o.UserId == userId.ToString()).ShouldBeTrue();
    }

    [Fact]
    public async Task GetPagedOrdersByUser_ShouldReturnCorrectPage()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var shippingAddressId = Guid.NewGuid();

        await _factory.SeedDatabaseAsync(context =>
        {
            var orders = TestDataGenerator.CreateOrders(userId, shippingAddressId, null, 10);
            context.Orders.AddRange(orders);
            context.SaveChanges();
        });

        var request = new GetPagedOrdersRequest
        {
            UserId = userId.ToString(),
            PageNumber = 1,
            PageSize = 5
        };

        // Act
        var response = await _grpcClient.GetPagedOrdersByUserAsync(request);

        // Assert
        response.ShouldNotBeNull();
        response.ResultCase.ShouldBe(GetPagedOrdersResponse.ResultOneofCase.Data);
        response.Data.Items.Count.ShouldBe(5);
        response.Data.TotalCount.ShouldBe(10);
        response.Data.PageNumber.ShouldBe(1);
        response.Data.PageSize.ShouldBe(5);
    }

    [Fact]
    public async Task CreateOrder_WithValidData_ShouldCreateOrder()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var shippingAddressId = Guid.NewGuid();
        var billingAddressId = Guid.NewGuid();
        
        var request = new CreateOrderRequest
        {
            UserId = userId.ToString(),
            ShippingAddressId = shippingAddressId.ToString(),
            BillingAddressId = billingAddressId.ToString()
        };

        request.Items.Add(new Protos.OrderItemInput
        {
            ProductId = Guid.NewGuid().ToString(),
            Quantity = 2,
            UnitPrice = 10.50
        });

        request.Items.Add(new Protos.OrderItemInput
        {
            ProductId = Guid.NewGuid().ToString(),
            Quantity = 1,
            UnitPrice = 25.00
        });

        // Act
        var response = await _grpcClient.CreateOrderAsync(request);

        // Assert
        response.ShouldNotBeNull();
        response.ResultCase.ShouldBe(OrderResponse.ResultOneofCase.Data);
        response.Data.UserId.ShouldBe(userId.ToString());
        response.Data.ShippingAddressId.ShouldBe(shippingAddressId.ToString());
        response.Data.BillingAddressId.ShouldBe(billingAddressId.ToString());
        response.Data.Status.ShouldBe(Protos.OrderStatus.Pending);
        response.Data.Items.Count.ShouldBe(2);
        response.Data.TotalAmount.ShouldBe(46.00);

        // Verify in database
        var orderId = Guid.Parse(response.Data.OrderId);
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<OrderDbContext>();
        var dbOrder = await context.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == orderId);

        dbOrder.ShouldNotBeNull();
        dbOrder.Items.Count.ShouldBe(2);
        dbOrder.TotalAmount.ShouldBe(46.00m);
    }

    [Fact]
    public async Task UpdateOrderStatus_WithValidStatus_ShouldUpdateOrder()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var shippingAddressId = Guid.NewGuid();
        Guid orderId = Guid.Empty;

        await _factory.SeedDatabaseAsync(context =>
        {
            var order = TestDataGenerator.CreateOrder(userId, shippingAddressId);
            context.Orders.Add(order);
            context.SaveChanges();
            orderId = order.Id;
        });

        var request = new UpdateOrderStatusRequest
        {
            OrderId = orderId.ToString(),
            Status = Protos.OrderStatus.Confirmed
        };

        // Act
        var response = await _grpcClient.UpdateOrderStatusAsync(request);

        // Assert
        response.ShouldNotBeNull();
        response.ResultCase.ShouldBe(OrderResponse.ResultOneofCase.Data);
        response.Data.Status.ShouldBe(Protos.OrderStatus.Confirmed);
        response.Data.UpdatedAt.ShouldNotBeNull();

        // Verify in database
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<OrderDbContext>();
        var dbOrder = await context.Orders.FindAsync(orderId);
        dbOrder.ShouldNotBeNull();
        dbOrder!.Status.ShouldBe(CoreOrderStatus.Confirmed);
        dbOrder.UpdatedAt.ShouldNotBeNull();
    }

    [Fact]
    public async Task CancelOrder_WithReason_ShouldCancelOrder()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var shippingAddressId = Guid.NewGuid();
        Guid orderId = Guid.Empty;

        await _factory.SeedDatabaseAsync(context =>
        {
            var order = TestDataGenerator.CreateOrder(userId, shippingAddressId);
            context.Orders.Add(order);
            context.SaveChanges();
            orderId = order.Id;
        });

        var request = new CancelOrderRequest
        {
            OrderId = orderId.ToString(),
            Reason = "Customer requested cancellation"
        };

        // Act
        var response = await _grpcClient.CancelOrderAsync(request);

        // Assert
        response.ShouldNotBeNull();
        response.ResultCase.ShouldBe(OrderResponse.ResultOneofCase.Data);
        response.Data.Status.ShouldBe(Protos.OrderStatus.Cancelled);
        response.Data.CancellationReason.ShouldBe("Customer requested cancellation");

        // Verify in database
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<OrderDbContext>();
        var dbOrder = await context.Orders.FindAsync(orderId);
        dbOrder.ShouldNotBeNull();
        dbOrder!.Status.ShouldBe(CoreOrderStatus.Cancelled);
        dbOrder.CancellationReason.ShouldBe("Customer requested cancellation");
    }

    [Fact]
    public async Task CancelOrder_AlreadyCancelled_ShouldReturnError()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var shippingAddressId = Guid.NewGuid();
        Guid orderId = Guid.Empty;

        await _factory.SeedDatabaseAsync(context =>
        {
            var order = TestDataGenerator.CreateOrder(userId, shippingAddressId);
            order.Cancel("First cancellation");
            context.Orders.Add(order);
            context.SaveChanges();
            orderId = order.Id;
        });

        var request = new CancelOrderRequest
        {
            OrderId = orderId.ToString(),
            Reason = "Second cancellation"
        };

        // Act
        var response = await _grpcClient.CancelOrderAsync(request);

        // Assert
        response.ShouldNotBeNull();
        response.ResultCase.ShouldBe(OrderResponse.ResultOneofCase.Error);
    }
}
