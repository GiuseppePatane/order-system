namespace ApiGateway.Core.Order.Dto;


public record OrderMutationResponseDto (string OrderId, string Status);


public class OrderDto
{
    public string OrderId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public List<OrderItemResponseDto> Items { get; set; } = new();
    public DateTime CreatedAt { get; set; }
}

public class OrderItemResponseDto
{
    public string OrderItemId { get; set; }
    public string ProductId { get; set; } 
    public string ProductName { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
 
}

