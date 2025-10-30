namespace ApiGateway.Core.Order;

/// <summary>
/// Request to add an item to an existing order (from client - no price for security)
/// </summary>
public class AddOrderItemRequestDto
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
}