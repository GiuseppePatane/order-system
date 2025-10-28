using ApiGateway.Core.Common;

namespace ApiGateway.Core.Product;

public interface IProductServiceClient
{
    Task<ServiceResult<ProductDto>> GetProductById(string productId);

    Task<ServiceResult<ProductDto>> CreateProduct(CreateProductRequestDto request);

    Task<ServiceResult<PagedProductsDto>> GetProducts(GetProductsRequestDto request);

    Task<ServiceResult<ProductDto>> UpdateProduct(UpdateProductRequestDto request);

    Task<ServiceResult<DeleteProductResultDto>> DeleteProduct(string productId);
}