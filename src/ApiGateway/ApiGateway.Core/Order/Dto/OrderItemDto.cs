namespace ApiGateway.Core.Order;

/// <summary>
/// Order item from client - only ProductId and Quantity
/// </summary>
public abstract class OrderItemDto
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
}