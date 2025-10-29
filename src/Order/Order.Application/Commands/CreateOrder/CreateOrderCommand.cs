namespace Order.Application.Commands.CreateOrder;

public record CreateOrderCommand(
    Guid UserId,
    List<OrderItemInput> Items,
    Guid ShippingAddressId,
    Guid? BillingAddressId = null
);

public record OrderItemInput(
    Guid ProductId,
    int Quantity,
    decimal UnitPrice
);
