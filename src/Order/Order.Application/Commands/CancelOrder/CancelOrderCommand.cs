namespace Order.Application.Commands.CancelOrder;

public record CancelOrderCommand(
    Guid OrderId,
    string? Reason = null
);
