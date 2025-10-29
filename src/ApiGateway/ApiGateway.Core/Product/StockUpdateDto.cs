namespace ApiGateway.Core.Product;

public class StockUpdateDto
{
    public string ProductId { get; set; } = string.Empty;
    public int UpdatedStock { get; set; }
}

