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

/// <summary>
/// Order item from client - only ProductId and Quantity
/// </summary>
public class OrderItemDto
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
}

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

/// <summary>
/// Internal order item with server-validated price
/// </summary>
public class OrderItemWithPriceDto
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}

// ===== Item Management DTOs =====

/// <summary>
/// Request to add an item to an existing order
/// </summary>
public class AddOrderItemRequestDto
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}

/// <summary>
/// Result of adding an item to an order
/// </summary>
public class AddOrderItemResultDto
{
    public string OrderId { get; set; } = string.Empty;
    public string ItemId { get; set; } = string.Empty;
}

/// <summary>
/// Result of removing an item from an order.
/// If this is returned successfully, the item was removed.
/// </summary>
public class RemoveOrderItemResultDto
{
    public string OrderId { get; set; } = string.Empty;
}

/// <summary>
/// Result of updating an order item quantity
/// </summary>
public class UpdateOrderItemQuantityResultDto
{
    public string OrderId { get; set; } = string.Empty;
    public string ItemId { get; set; } = string.Empty;
    public int NewQuantity { get; set; }
}

