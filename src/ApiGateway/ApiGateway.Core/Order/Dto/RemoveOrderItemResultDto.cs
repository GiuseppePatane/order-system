namespace ApiGateway.Core.Order.Dto;

/// <summary>
/// Result of removing an item from an order.
/// If this is returned successfully, the item was removed.
/// </summary>
public class RemoveOrderItemResultDto
{
    public string OrderId { get; set; } = string.Empty;
}