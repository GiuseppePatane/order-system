namespace ApiGateway.Core.Order;

/// <summary>
/// Result of adding an item to an order
/// </summary>
public class AddOrderItemResultDto
{
    public string OrderId { get; set; } = string.Empty;
    public string ItemId { get; set; } = string.Empty;
}