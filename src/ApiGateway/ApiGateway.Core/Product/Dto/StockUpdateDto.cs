namespace ApiGateway.Core.Product.Dto;

public class StockUpdateDto
{
    public string ProductId { get; set; } = string.Empty;
    public int UpdatedStock { get; set; }
    public int LockedQuantity { get; set; }
    public decimal LockedPrice { get; set; }
}

