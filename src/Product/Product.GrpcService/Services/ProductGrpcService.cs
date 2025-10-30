using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Product.Application.Commands.CreateProduct;
using Product.Application.Commands.DeleteProduct;
using Product.Application.Commands.UpdateProduct;
using Product.Application.Commands.LockProductStock;
using Product.Application.Commands.ReleaseProductStock;
using Product.Core.Repositories;
using Products;
using Shared.Core.Domain.Errors;
using Shared.GrpcInfrastructure.Base;

namespace Product.GrpcService.Services;

public class ProductGrpcService : ProductService.ProductServiceBase
{
    private readonly ILogger<ProductGrpcService> _logger;
    private readonly IProductReadOnlyRepository _productRepository;
    private readonly ICategoryReadOnlyRepository _categoryRepository;
    private readonly CreateProductHandler _createProductHandler;
    private readonly UpdateProductHandler _updateProductHandler;
    private readonly DeleteProductHandler _deleteProductHandler;
    private readonly LockProductStockHandler _lockProductStockHandler;
    private readonly ReleaseProductStockHandler _releaseProductStockHandler;
    private readonly GrpcServiceBase<ProductGrpcService> _baseService;

    public ProductGrpcService(
        ILogger<ProductGrpcService> logger,
        IProductReadOnlyRepository productRepository,
        ICategoryReadOnlyRepository categoryRepository,
        CreateProductHandler createProductHandler,
        UpdateProductHandler updateProductHandler,
        DeleteProductHandler deleteProductHandler,
        LockProductStockHandler lockProductStockHandler,
        ReleaseProductStockHandler releaseProductStockHandler)
    {
        _logger = logger;
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
        _createProductHandler = createProductHandler;
        _updateProductHandler = updateProductHandler;
        _deleteProductHandler = deleteProductHandler;
        _lockProductStockHandler = lockProductStockHandler;
        _releaseProductStockHandler = releaseProductStockHandler;
        _baseService = new InternalGrpcServiceBase(logger);
    }

    public override async Task<ProductResponse> GetProduct(
        GetProductRequest request,
        ServerCallContext context
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
                if (error is NotFoundError)
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
                Data = product.ToProductDataResponse(),
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
        ServerCallContext context
    )
    {
        try
        {
            // Validate CategoryId is a valid GUID
            if (!_baseService.IsValidGuid(request.CategoryId, out var categoryId))
            {
                return new ProductResponse
                {
                    Error = _baseService.CreateInvalidArgumentError(
                        "CategoryId",
                        "invalid or missing GUID"
                    ),
                };
            }

            var command = new CreateProductCommand(
                request.Name,
                request.Description,
                (decimal)request.Price,
                request.Stock,
                request.Sku,
                categoryId
            );

            var result = await _createProductHandler.Handle(command, context.CancellationToken);

            if (result.IsFailure)
            {
                var error = result.Error;

                return error switch
                {
                    ValidationError ve => new ProductResponse
                    {
                        Error = _baseService.CreateInvalidArgumentError(ve.FieldName, ve.Reason),
                    },
                    NotFoundError nf => new ProductResponse
                    {
                        Error = _baseService.CreateNotFoundError(nf.EntityType, nf.EntityId),
                    },
                    DuplicateError de => new ProductResponse
                    {
                        Error = _baseService.CreateError(de.Code, de.Message),
                    },
                    PersistenceError pe => new ProductResponse
                    {
                        Error = _baseService.CreateInternalError(pe.Message),
                    },
                    _ => new ProductResponse { Error = _baseService.CreateInternalError() },
                };
            }

            var createdId = result.Value.ProductId.ToString();

            return new ProductResponse { Data = new ProductData { ProductId = createdId } };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in CreateProduct");
            return new ProductResponse { Error = _baseService.CreateInternalError() };
        }
    }

    public override async Task<ProductResponse> UpdateProduct(
        UpdateProductRequest request,
        ServerCallContext context
    )
    {
        if (!_baseService.IsValidGuid(request.CategoryId, out var categoryId))
        {
            return new ProductResponse
            {
                Error = _baseService.CreateInvalidArgumentError(
                    "CategoryId",
                    "invalid or missing GUID"
                ),
            };
        }
        if (!_baseService.IsValidGuid(request.ProductId, out var productId))
        {
            return new ProductResponse
            {
                Error = _baseService.CreateInvalidArgumentError(
                    "ProductId",
                    "invalid or missing GUID"
                ),
            };
        }

        var updateCommand = new UpdateProductCommand(
            productId,
            request.Name,
            request.Description,
            (decimal)request.Price,
            request.Stock,
            request.Sku,
            categoryId
        );
        var result = await _updateProductHandler.Handle(updateCommand, context.CancellationToken);
        if (result.IsFailure)
        {
            var error = result.Error;

            return error switch
            {
                ValidationError ve => new ProductResponse
                {
                    Error = _baseService.CreateInvalidArgumentError(ve.FieldName, ve.Reason),
                },
                NotFoundError nf => new ProductResponse
                {
                    Error = _baseService.CreateNotFoundError(nf.EntityType, nf.EntityId),
                },
                PersistenceError pe => new ProductResponse
                {
                    Error = _baseService.CreateInternalError(pe.Message),
                },
                _ => new ProductResponse { Error = _baseService.CreateInternalError() },
            };
        }

        return new ProductResponse
        {
            Data = new ProductData
            {
                ProductId = result.Value.ProductId.ToString()
            },
        };
    }

    public override async Task<DeleteProductResponse> DeleteProduct(
        DeleteProductRequest request,
        ServerCallContext context
    )
    {
        if (!_baseService.IsValidGuid(request.ProductId, out var productId))
        {
            return new DeleteProductResponse
            {
                Error = _baseService.CreateInvalidArgumentError(
                    "CategoryId",
                    "invalid or missing GUID"
                ),
            };
        }

        var deleteCommand = new DeleteProductCommand(productId);
        var result = await _deleteProductHandler.Handle(deleteCommand, context.CancellationToken);
        if (result.IsFailure)
        {
            var error = result.Error;

            return error switch
            {
                NotFoundError nf => new DeleteProductResponse
                {
                    Error = _baseService.CreateNotFoundError(nf.EntityType, nf.EntityId),
                },
                PersistenceError pe => new DeleteProductResponse
                {
                    Error = _baseService.CreateInternalError(pe.Message),
                },
                _ => new DeleteProductResponse { Error = _baseService.CreateInternalError() },
            };
        }

        return new DeleteProductResponse
        {
            Data = new DeleteProductData
            {
                ProductId = result.Value.ProductId.ToString(),
                Success = true
            },
        };
    }

    public override async Task<GetProductsResponse> GetProducts(GetProductsRequest request, ServerCallContext context)
    {
        Guid? categoryId = null;
        if (!string.IsNullOrWhiteSpace(request.CategoryId))
        {
            categoryId = Guid.Parse(request.CategoryId);
        }
        bool? isActive = null;
        if (request.HasIsActive)
        {
            isActive = request.IsActive;
        }
        string? searchTerm = null;
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            searchTerm = request.SearchTerm;
        }

        var result = await _productRepository.GetPagedAsync(
            request.PageNumber,
            request.PageSize,
            categoryId,
            isActive,
            searchTerm,
            context.CancellationToken
        );
        
         if (result.IsFailure)   
         {
            var error = result.Error;
            return new GetProductsResponse
            {
                Error = _baseService.CreateInternalError()
            };
         }

         return new GetProductsResponse()
         {
             Data = new ProductListData()
             {
                 TotalCount = result.Value.TotalCount,
                 PageNumber = result.Value.PageNumber,
                 PageSize = result.Value.PageSize,
                 TotalPages = result.Value.TotalPages,
                 Items =
                 {
                     result.Value.Items != null ?
                     result.Value.Items.Select(p => p.ToProductDataResponse()) : []
                 }
             }
         };
    }

    public override async Task<UpdateStockResponse> LockProductStock(
        UpdateStockRequest request,
        ServerCallContext context)
    {
        try
        {
            if (!_baseService.IsValidGuid(request.ProductId, out var productId))
            {
                return new UpdateStockResponse
                {
                    Error = _baseService.CreateInvalidArgumentError("ProductId", "Invalid product ID")
                };
            }

            var command = new LockProductStockCommand(productId, request.Quantity);
            var result = await _lockProductStockHandler.Handle(command, context.CancellationToken);

            if (result.IsFailure)
            {
                var error = result.Error;

                if (error is ValidationError ve)
                    return new UpdateStockResponse { Error = _baseService.CreateInvalidArgumentError(ve.FieldName, ve.Reason) };

                if (error is NotFoundError nf)
                    return new UpdateStockResponse { Error = _baseService.CreateNotFoundError(nf.EntityType, nf.EntityId) };

                return new UpdateStockResponse { Error = _baseService.CreateInternalError() };
            }

            return new UpdateStockResponse
            {
                Data = new StockUpdateData
                {
                    ProductId = result.Value.ProductId.ToString(),
                    UpdatedStock = result.Value.UpdatedStock,
                    LockedPrice = (double) result.Value.LockedPrice, 
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error locking product stock for {ProductId}", request.ProductId);
            return new UpdateStockResponse { Error = _baseService.CreateInternalError() };
        }
    }

    public override async Task<UpdateStockResponse> ReleaseProductStock(
        UpdateStockRequest request,
        ServerCallContext context)
    {
        try
        {
            if (!_baseService.IsValidGuid(request.ProductId, out var productId))
            {
                return new UpdateStockResponse
                {
                    Error = _baseService.CreateInvalidArgumentError("ProductId", "Invalid product ID")
                };
            }

            var command = new ReleaseProductStockCommand(productId, request.Quantity);
            var result = await _releaseProductStockHandler.Handle(command, context.CancellationToken);

            if (result.IsFailure)
            {
                var error = result.Error;

                if (error is ValidationError ve)
                    return new UpdateStockResponse { Error = _baseService.CreateInvalidArgumentError(ve.FieldName, ve.Reason) };

                if (error is NotFoundError nf)
                    return new UpdateStockResponse { Error = _baseService.CreateNotFoundError(nf.EntityType, nf.EntityId) };

                return new UpdateStockResponse { Error = _baseService.CreateInternalError() };
            }

            return new UpdateStockResponse
            {
                Data = new StockUpdateData
                {
                    ProductId = result.Value.ProductId.ToString(),
                    UpdatedStock = result.Value.UpdatedStock,
                    LockedPrice = 0.0
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error releasing product stock for {ProductId}", request.ProductId);
            return new UpdateStockResponse { Error = _baseService.CreateInternalError() };
        }
    }

    public override async Task<GetCategoriesResponse> GetCategories(
        GetCategoriesRequest request,
        ServerCallContext context)
    {
        try
        {
            bool? isActive = null;
            if (request.HasIsActive)
            {
                isActive = request.IsActive;
            }

            var result = await _categoryRepository.GetPagedAsync(
                request.PageNumber,
                request.PageSize,
                isActive,
                context.CancellationToken
            );

            return new GetCategoriesResponse
            {
                Data = new CategoryListData
                {
                    TotalCount = result.TotalCount,
                    PageNumber = result.PageNumber,
                    PageSize = result.PageSize,
                    TotalPages = result.TotalPages,
                    Items =
                    {
                        result.Items != null ?
                        result.Items.Select(c => c.ToCategoryDataResponse()) : []
                    }
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving categories");
            return new GetCategoriesResponse
            {
                Error = _baseService.CreateInternalError()
            };
        }
    }

    // Helper class to access protected methods
    private class InternalGrpcServiceBase : GrpcServiceBase<ProductGrpcService>
    {
        public InternalGrpcServiceBase(ILogger<ProductGrpcService> logger)
            : base(logger) { }
    }


}

public static class Mapping
{
    public static ProductData ToProductDataResponse(this Core.Domain.Product product)
    {
        return new ProductData
        {
            ProductId = product.Id.ToString(),
            Name = product.Name,
            Description = product.Description,
            Price = (double)product.Price,
            Stock = product.Stock,
            Sku = product.Sku,
            CategoryId = product.CategoryId.ToString(),
            IsActive = product.IsActive,
            CreatedAt = product.CreatedAt.ToTimestamp(),
            UpdatedAt = product.UpdatedAt?.ToTimestamp()
        };
    }

    public static CategoryData ToCategoryDataResponse(this Core.Domain.Category category)
    {
        return new CategoryData
        {
            CategoryId = category.Id.ToString(),
            Name = category.Name,
            Description = category.Description,
            IsActive = category.IsActive,
            CreatedAt = category.CreatedAt.ToTimestamp(),
            UpdatedAt = category.UpdatedAt?.ToTimestamp()
        };
    }
}