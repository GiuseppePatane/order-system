namespace ApiGateway.Core.Order;

/// <summary>
/// Request DTO from client - does NOT include prices (security)
/// </summary>
public class CreateOrderRequestDto
{
    public Guid UserId { get; set; }
    public Guid ShippingAddressId { get; set; }
    public Guid? BillingAddressId { get; set; }
    public List<OrderItemDto> Items { get; set; } = new();
}

// ===== Item Management DTOs =====