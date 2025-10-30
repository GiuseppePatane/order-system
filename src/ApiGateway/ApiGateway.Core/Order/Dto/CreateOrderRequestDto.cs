namespace ApiGateway.Core.Order.Dto;

public class CreateOrderRequestDto
{
    public Guid UserId { get; set; }
    public Guid ShippingAddressId { get; set; }
    public Guid? BillingAddressId { get; set; }
    public required OrderItemDto FirstItem { get; set; } 
}

