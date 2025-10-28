using Grpc.Core;
using Product.Core.Repositories;
using Products;
using Shared.GrpcInfrastructure.Base;

namespace Product.GrpcService.Services;

public class ProductGrpcService : ProductService.ProductServiceBase
{
    private readonly ILogger<ProductGrpcService> _logger;
    private readonly IProductRepository _productRepository;
    private readonly GrpcServiceBase<ProductGrpcService> _baseService;

    public ProductGrpcService(ILogger<ProductGrpcService> logger, IProductRepository productRepository)
    {
        _logger = logger;
        _productRepository = productRepository;
        _baseService = new InternalGrpcServiceBase(logger);
    }


    public override Task<ProductResponse> CreateProduct(CreateProductRequest request, ServerCallContext context)
    {
      
        
    }


    public override async Task<ProductResponse> GetProduct(GetProductRequest request, Grpc.Core.ServerCallContext context)
    {
        try
        {
            if (!_baseService.IsValidGuid(request.ProductId, out var  productId))
            {
                return new ProductResponse
                {
                    Error = _baseService.CreateInvalidArgumentError("ProductId", "cannot be empty")
                };
            }
            
            var product =await  _productRepository.GetByIdAsync(productId, context.CancellationToken);
            if (product == null)
            {
                return new ProductResponse
                {
                    Error = _baseService.CreateNotFoundError("Product", request.ProductId)
                };
            }
            return new ProductResponse
            {
                Data = new ProductData
                {
                    ProductId = product.Id.ToString(),
                    Name = product.Name,
                    Description = product.Description
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in GetProduct for ProductId: {ProductId}", request.ProductId);

            return new ProductResponse
            {
                Error = _baseService.CreateInternalError()
            };
        }
    }

    // Helper class to access protected methods
    private class InternalGrpcServiceBase : GrpcServiceBase<ProductGrpcService>
    {
        public InternalGrpcServiceBase(ILogger<ProductGrpcService> logger) : base(logger) { }
    }
}