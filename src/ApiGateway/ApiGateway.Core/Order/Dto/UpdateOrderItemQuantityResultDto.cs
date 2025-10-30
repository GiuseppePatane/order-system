namespace ApiGateway.Core.Order;

/// <summary>
/// Result of updating an order item quantity
/// </summary>
public class UpdateOrderItemQuantityResultDto
{
    public string OrderId { get; set; } = string.Empty;
    public string ItemId { get; set; } = string.Empty;
    public int NewQuantity { get; set; }
}