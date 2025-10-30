namespace ApiGateway.Core.Order;

/// <summary>
/// Internal request with server-validated price
/// </summary>
public class AddOrderItemWithPriceDto
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}