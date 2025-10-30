namespace ApiGateway.Core.Order.Dto;

/// <summary>
/// Internal order item with server-validated price
/// </summary>
public class OrderItemWithPriceDto
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal LockedPrice { get; set; }
}