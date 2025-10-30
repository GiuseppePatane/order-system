using ApiGateway.Core.Common;
using ApiGateway.Core.Product.Dto;

namespace ApiGateway.Core.Product;

public interface IProductServiceClient
{
    Task<ServiceResult<ProductDto>> GetProductById(string productId, CancellationToken cancellationToken=default);

    Task<ServiceResult<ProductMutationResultDto>> CreateProduct(CreateProductRequestDto request, CancellationToken cancellationToken=default);

    Task<ServiceResult<PagedProductsDto>> GetProducts(GetProductsRequestDto request, CancellationToken cancellationToken=default);

    Task<ServiceResult<ProductMutationResultDto>> UpdateProduct(string productId, UpdateProductRequestDto request, CancellationToken cancellationToken=default);

    Task<ServiceResult<ProductMutationResultDto>> DeleteProduct(string productId, CancellationToken cancellationToken=default);

    Task<ServiceResult<StockUpdateDto>> LockProductStock(string productId, int quantity,CancellationToken cancellationToken=default);

    Task<ServiceResult<StockUpdateDto>> ReleaseProductStock(string productId, int quantity,CancellationToken cancellationToken=default);

    Task<ServiceResult<PagedCategoriesDto>> GetCategories(GetCategoriesRequestDto request, CancellationToken cancellationToken=default);
}