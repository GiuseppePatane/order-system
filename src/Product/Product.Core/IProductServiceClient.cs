namespace Product.Core;

public interface IProductServiceClient
{
    Task GetProductById(string id);
}