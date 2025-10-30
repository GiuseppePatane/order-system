namespace ApiGateway.Core.Order.Dto;

/// <summary>
/// Internal DTO with prices retrieved from ProductService (server-side)
/// </summary>
public class CreateOrderRequestWithPricesDto
{
    public Guid UserId { get; set; }
    public Guid ShippingAddressId { get; set; }
    public Guid? BillingAddressId { get; set; }
    public List<OrderItemWithPriceDto> Items { get; set; } = new();
}