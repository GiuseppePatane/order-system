using ApiGateway.Core.Common;

namespace ApiGateway.Core.Product;

public interface IProductServiceClient
{
    Task<ServiceResult<ProductDto>> GetProductById(string productId);
}