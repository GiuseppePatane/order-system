using Grpc.Core;
using Product.Core.Repositories;
using Products;
using Shared.GrpcInfrastructure.Base;
using Product.Application.Commands.CreateProduct;
using Shared.Core.Domain.Errors;

namespace Product.GrpcService.Services;

public class ProductGrpcService : ProductService.ProductServiceBase
{
    private readonly ILogger<ProductGrpcService> _logger;
    private readonly IProductReadOnlyRepository _productRepository;
    private readonly CreateProductHandler _createProductHandler;
    private readonly GrpcServiceBase<ProductGrpcService> _baseService;

    public ProductGrpcService(
        ILogger<ProductGrpcService> logger,
        IProductReadOnlyRepository productRepository,
        CreateProductHandler createProductHandler
    )
    {
        _logger = logger;
        _productRepository = productRepository;
        _createProductHandler = createProductHandler;
        _baseService = new InternalGrpcServiceBase(logger);
    }
    

    public override async Task<ProductResponse> GetProduct(
        GetProductRequest request,
        Grpc.Core.ServerCallContext context
    )
    {
        try
        {
            if (!_baseService.IsValidGuid(request.ProductId, out var productId))
            {
                return new ProductResponse
                {
                    Error = _baseService.CreateInvalidArgumentError("ProductId", "cannot be empty"),
                };
            }

            var productResult = await _productRepository.GetByIdAsync(
                productId,
                context.CancellationToken
            );

            if (productResult.IsFailure)
            {
                var error = productResult.Error;
                if (error is Shared.Core.Domain.Errors.NotFoundError)
                {
                    return new ProductResponse
                    {
                        Error = _baseService.CreateNotFoundError("Product", request.ProductId),
                    };
                }

                return new ProductResponse { Error = _baseService.CreateInternalError() };
            }
            var product = productResult.Value;
            return new ProductResponse
            {
                Data = new ProductData
                {
                    ProductId = product.Id.ToString(),
                    Name = product.Name,
                    Description = product.Description,
                },
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Unexpected error in GetProduct for ProductId: {ProductId}",
                request.ProductId
            );

            return new ProductResponse { Error = _baseService.CreateInternalError() };
        }
    }

    public override async Task<ProductResponse> CreateProduct(
        CreateProductRequest request,
        Grpc.Core.ServerCallContext context)
    {
        try
        {
            // Validate CategoryId is a valid GUID
            if (!_baseService.IsValidGuid(request.CategoryId, out var categoryId))
            {
                return new ProductResponse
                {
                    Error = _baseService.CreateInvalidArgumentError("CategoryId", "invalid or missing GUID")
                };
            }

            var command = new CreateProductCommand(
                request.Name,
                request.Description,
                (decimal)request.Price,
                request.Stock,
                request.Sku,
                categoryId);

            var result = await _createProductHandler.Handle(command, context.CancellationToken);

            if (result.IsFailure)
            {
                var error = result.Error;

                return error switch
                {
                    ValidationError ve => new ProductResponse
                    {
                        Error = _baseService.CreateInvalidArgumentError(ve.FieldName, ve.Reason)
                    },
                    NotFoundError nf => new ProductResponse
                    {
                        Error = _baseService.CreateNotFoundError(nf.EntityType, nf.EntityId)
                    },
                    DuplicateError de => new ProductResponse { Error = _baseService.CreateError(de.Code, de.Message) },
                    PersistenceError pe => new ProductResponse { Error = _baseService.CreateInternalError(pe.Message) },
                    _ => new ProductResponse { Error = _baseService.CreateInternalError() }
                };
            }

            var createdId = result.Value.ProductId.ToString();

            return new ProductResponse
            {
                Data = new ProductData { ProductId = createdId }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in CreateProduct");
            return new ProductResponse { Error = _baseService.CreateInternalError() };
        }
    }

    // Helper class to access protected methods
    private class InternalGrpcServiceBase : GrpcServiceBase<ProductGrpcService>
    {
        public InternalGrpcServiceBase(ILogger<ProductGrpcService> logger)
            : base(logger) { }
    }
}
