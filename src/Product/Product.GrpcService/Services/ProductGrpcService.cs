using Products;
using Shared.GrpcInfrastructure.Base;

namespace Product.GrpcService.Services;

public class ProductGrpcService : ProductService.ProductServiceBase
{
    private readonly ILogger<ProductGrpcService> _logger;
    private readonly GrpcServiceBase<ProductGrpcService> _baseService;

    public ProductGrpcService(ILogger<ProductGrpcService> logger)
    {
        _logger = logger;
        _baseService = new InternalGrpcServiceBase(logger);
    }

    public override Task<ProductResponse> GetProduct(GetProductRequest request, Grpc.Core.ServerCallContext context)
    {
        try
        {
            if (!_baseService.IsValidString(request.ProductId))
            {
                return Task.FromResult(new ProductResponse
                {
                    Error = _baseService.CreateInvalidArgumentError("ProductId", "cannot be empty")
                });
            }

            // Simulazione: prodotto non trovato
            if (request.ProductId == "999")
            {
                return Task.FromResult(new ProductResponse
                {
                    Error = _baseService.CreateNotFoundError("Product", request.ProductId)
                });
            }

            // Successo
            return Task.FromResult(new ProductResponse
            {
                Data = new ProductData
                {
                    ProductId = request.ProductId,
                    Name = "Sample Product",
                    Description = "This is a sample product description.",
                    Price = 19.99
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in GetProduct for ProductId: {ProductId}", request.ProductId);

            return Task.FromResult(new ProductResponse
            {
                Error = _baseService.CreateInternalError()
            });
        }
    }

    // Helper class to access protected methods
    private class InternalGrpcServiceBase : GrpcServiceBase<ProductGrpcService>
    {
        public InternalGrpcServiceBase(ILogger<ProductGrpcService> logger) : base(logger) { }
    }
}