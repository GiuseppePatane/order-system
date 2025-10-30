using ApiGateway.Core.Common;
using ApiGateway.Core.Product.Dto;

namespace ApiGateway.Core.Product;

public interface IProductServiceClient
{
    Task<ServiceResult<ProductDto>> GetProductById(string productId);

    Task<ServiceResult<ProductMutationResultDto>> CreateProduct(CreateProductRequestDto request);

    Task<ServiceResult<PagedProductsDto>> GetProducts(GetProductsRequestDto request);

    Task<ServiceResult<ProductMutationResultDto>> UpdateProduct(string productId, UpdateProductRequestDto request);

    Task<ServiceResult<ProductMutationResultDto>> DeleteProduct(string productId);

    Task<ServiceResult<StockUpdateDto>> LockProductStock(string productId, int quantity);

    Task<ServiceResult<StockUpdateDto>> ReleaseProductStock(string productId, int quantity);

    Task<ServiceResult<PagedCategoriesDto>> GetCategories(GetCategoriesRequestDto request);
}