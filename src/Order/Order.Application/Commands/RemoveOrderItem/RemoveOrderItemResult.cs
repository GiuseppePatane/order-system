namespace Order.Application.Commands.RemoveOrderItem;

/// <summary>
/// Result of removing an item from an order.
/// If this result is returned successfully, the item was removed.
/// </summary>
public record RemoveOrderItemResult(Guid OrderId);
