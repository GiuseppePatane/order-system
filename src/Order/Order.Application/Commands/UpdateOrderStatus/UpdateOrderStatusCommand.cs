using Order.Core.Domain;

namespace Order.Application.Commands.UpdateOrderStatus;

public record UpdateOrderStatusCommand(
    Guid OrderId,
    OrderStatus Status
);
