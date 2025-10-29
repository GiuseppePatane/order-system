using Bogus;
using Order.Core.Domain;
using Order.Infrastructure.EF;

namespace Order.IntegrationTest.Fixtures;

/// <summary>
/// Generates realistic test data using Bogus
/// </summary>
public static class TestDataGenerator
{
    private static readonly Faker Faker = new();

    /// <summary>
    /// Creates a list of order items for testing
    /// </summary>
    public static List<OrderItem> CreateOrderItems(int count = 3)
    {
        var items = new List<OrderItem>();

        for (int i = 0; i < count; i++)
        {
            var result = OrderItem.Create(
                Guid.NewGuid(), // Product ID
                Faker.Random.Int(1, 10), // Quantity
                Faker.Random.Decimal(5m, 500m) // Unit Price
            );

            if (result.IsSuccess)
            {
                items.Add(result.Value!);
            }
        }

        return items;
    }

    /// <summary>
    /// Creates test orders for a specific user
    /// </summary>
    public static List<Core.Domain.Order> CreateOrders(
        Guid userId,
        Guid shippingAddressId,
        Guid? billingAddressId = null,
        int count = 5)
    {
        var orders = new List<Core.Domain.Order>();

        for (int i = 0; i < count; i++)
        {
            var items = CreateOrderItems(Faker.Random.Int(1, 5));
            var result = Core.Domain.Order.Create(
                userId,
                shippingAddressId,
                items,
                billingAddressId
            );

            if (result.IsSuccess)
            {
                var order = result.Value!;
                
                // Vary the order status
                if (i % 5 == 1) order.UpdateStatus(OrderStatus.Confirmed);
                if (i % 5 == 2)
                {
                    order.UpdateStatus(OrderStatus.Confirmed);
                    order.UpdateStatus(OrderStatus.Processing);
                }
                if (i % 5 == 3)
                {
                    order.UpdateStatus(OrderStatus.Confirmed);
                    order.UpdateStatus(OrderStatus.Processing);
                    order.UpdateStatus(OrderStatus.Shipped);
                }
                if (i % 5 == 4)
                {
                    order.Cancel("Test cancellation");
                }

                orders.Add(order);
            }
        }

        return orders;
    }

    /// <summary>
    /// Creates a single order with specific properties
    /// </summary>
    public static Core.Domain.Order CreateOrder(
        Guid? userId = null,
        Guid? shippingAddressId = null,
        Guid? billingAddressId = null,
        List<OrderItem>? items = null,
        OrderStatus? status = null)
    {
        var orderItems = items ?? CreateOrderItems(2);
        var result = Core.Domain.Order.Create(
            userId ?? Guid.NewGuid(),
            shippingAddressId ?? Guid.NewGuid(),
            orderItems,
            billingAddressId
        );

        var order = result.Value!;
        
        // Update status if specified
        if (status.HasValue && status.Value != OrderStatus.Pending)
        {
            order.UpdateStatus(status.Value);
        }

        return order;
    }

    /// <summary>
    /// Seeds database with test data for multiple users
    /// </summary>
    public static async Task SeedDatabase(
        OrderDbContext context,
        List<(Guid UserId, Guid ShippingAddressId, Guid? BillingAddressId)> users,
        int ordersPerUser = 3)
    {
        foreach (var user in users)
        {
            var orders = CreateOrders(
                user.UserId,
                user.ShippingAddressId,
                user.BillingAddressId,
                ordersPerUser);
            await context.Orders.AddRangeAsync(orders);
        }

        await context.SaveChangesAsync();
    }
}
