namespace Order.Application.Commands.AddOrderItem;

public record AddOrderItemCommand(
    Guid OrderId,
    Guid ProductId,
    int Quantity,
    decimal UnitPrice
);
