namespace ApiGateway.Core.Order.Dto;


public class OrderItemDto
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
}