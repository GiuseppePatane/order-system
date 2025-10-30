using ApiGateway.Core.Common;
using ApiGateway.Core.Product;
using ApiGateway.Core.Product.Dto;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using Products;

namespace ApiGateway.infrastructure.GrcpClient.Product;

public class ProductServiceGrpcClient : IProductServiceClient
{
    private readonly ProductService.ProductServiceClient _grpcClient;
    private readonly ILogger<ProductServiceGrpcClient> _logger;

    public ProductServiceGrpcClient(
        ProductService.ProductServiceClient grpcClient,
        ILogger<ProductServiceGrpcClient> logger)
    {
        _grpcClient = grpcClient;
        _logger = logger;
    }

    public async Task<ServiceResult<ProductDto>> GetProductById(string productId,CancellationToken cancellationToken=default)
    {
        try
        {
            _logger.LogInformation("Calling gRPC service to get product with ID: {ProductId}", productId);

            var request = new GetProductRequest
            {
                ProductId = productId
            };

            var response = await _grpcClient.GetProductAsync(request, cancellationToken: cancellationToken);

            return response.ResultCase switch
            {
                ProductResponse.ResultOneofCase.Data => ServiceResult<ProductDto>.Success(MapToDto(response.Data)),
                ProductResponse.ResultOneofCase.Error => ServiceResult<ProductDto>.Failure(MapToErrorInfo(response.Error)),
                _ => ServiceResult<ProductDto>.Failure(new ErrorInfo
                {
                    Code = "EMPTY_RESPONSE",
                    Message = "The gRPC service returned an empty response"
                })
            };
        }
        catch (RpcException ex)
        {
            _logger.LogError(ex, "gRPC call failed for ProductId: {ProductId}", productId);

            return ServiceResult<ProductDto>.Failure(new ErrorInfo
            {
                Code = "GRPC_ERROR",
                Message = $"Failed to communicate with the product service: {ex.Status.Detail}",
                Details = new Dictionary<string, string>
                {
                    ["StatusCode"] = ex.StatusCode.ToString(),
                    ["Detail"] = ex.Status.Detail ?? string.Empty
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while calling product service for ProductId: {ProductId}", productId);

            return ServiceResult<ProductDto>.Failure(new ErrorInfo
            {
                Code = "UNEXPECTED_ERROR",
                Message = "An unexpected error occurred while retrieving the product",
                Details = new Dictionary<string, string>
                {
                    ["ExceptionType"] = ex.GetType().Name,
                    ["Message"] = ex.Message
                }
            });
        }
    }

    public async Task<ServiceResult<ProductMutationResultDto>> CreateProduct(CreateProductRequestDto requestDto,CancellationToken cancellationToken=default)
    {
        try
        {
            var request = new CreateProductRequest
            {
                Name = requestDto.Name,
                Description = requestDto.Description ?? string.Empty,
                Price = requestDto.Price,
                Stock = requestDto.Stock,
                Sku = requestDto.Sku,
                CategoryId = requestDto.CategoryId
            };

            var response = await _grpcClient.CreateProductAsync(request, cancellationToken: cancellationToken);

            return response.ResultCase switch
            {
                ProductResponse.ResultOneofCase.Data => ServiceResult<ProductMutationResultDto>.Success(
                    new ProductMutationResultDto(response.Data.ProductId)),
                ProductResponse.ResultOneofCase.Error => ServiceResult<ProductMutationResultDto>.Failure(MapToErrorInfo(response.Error)),
                _ => ServiceResult<ProductMutationResultDto>.Failure(new ErrorInfo { Code = "EMPTY_RESPONSE", Message = "Empty response from gRPC" })
            };
        }
        catch (RpcException ex)
        {
            _logger.LogError(ex, "gRPC CreateProduct failed");
            return ServiceResult<ProductMutationResultDto>.Failure(new ErrorInfo { Code = "GRPC_ERROR", Message = ex.Status.Detail });
        }
    }

    public async Task<ServiceResult<PagedProductsDto>> GetProducts(GetProductsRequestDto requestDto,CancellationToken cancellationToken=default)
    {
        try
        {
            var request = new GetProductsRequest
            {
                PageNumber = requestDto.PageNumber,
                PageSize = requestDto.PageSize,
            };

            if (!string.IsNullOrWhiteSpace(requestDto.CategoryId))
                request.CategoryId = requestDto.CategoryId;

            if (requestDto.IsActive.HasValue)
                request.IsActive = requestDto.IsActive.Value;

            if (!string.IsNullOrWhiteSpace(requestDto.SearchTerm))
                request.SearchTerm = requestDto.SearchTerm;

            var response = await _grpcClient.GetProductsAsync(request, cancellationToken: cancellationToken);

            return response.ResultCase switch
            {
                GetProductsResponse.ResultOneofCase.Data => ServiceResult<PagedProductsDto>.Success(MapToPagedDto(response.Data)),
                GetProductsResponse.ResultOneofCase.Error => ServiceResult<PagedProductsDto>.Failure(MapToErrorInfo(response.Error)),
                _ => ServiceResult<PagedProductsDto>.Failure(new ErrorInfo { Code = "EMPTY_RESPONSE", Message = "Empty response from gRPC" })
            };
        }
        catch (RpcException ex)
        {
            _logger.LogError(ex, "gRPC GetProducts failed");
            return ServiceResult<PagedProductsDto>.Failure(new ErrorInfo { Code = "GRPC_ERROR", Message = ex.Status.Detail });
        }
    }

    public async Task<ServiceResult<ProductMutationResultDto>> UpdateProduct(string productId, UpdateProductRequestDto dto,CancellationToken cancellationToken=default)
    {
        try
        {
            var request = new UpdateProductRequest
            {
                ProductId = productId
            };

            if (!string.IsNullOrWhiteSpace(dto.Name)) request.Name = dto.Name;
            if (!string.IsNullOrWhiteSpace(dto.Description)) request.Description = dto.Description;
            if (dto.Price.HasValue) request.Price = dto.Price.Value;
            if (!string.IsNullOrWhiteSpace(dto.Sku)) request.Sku = dto.Sku;
            if (!string.IsNullOrWhiteSpace(dto.CategoryId)) request.CategoryId = dto.CategoryId;

            var response = await _grpcClient.UpdateProductAsync(request, cancellationToken: cancellationToken);

            return response.ResultCase switch
            {
                ProductResponse.ResultOneofCase.Data => ServiceResult<ProductMutationResultDto>.Success(
                    new ProductMutationResultDto(response.Data.ProductId)),
                ProductResponse.ResultOneofCase.Error => ServiceResult<ProductMutationResultDto>.Failure(MapToErrorInfo(response.Error)),
                _ => ServiceResult<ProductMutationResultDto>.Failure(new ErrorInfo { Code = "EMPTY_RESPONSE", Message = "Empty response from gRPC" })
            };
        }
        catch (RpcException ex)
        {
            _logger.LogError(ex, "gRPC UpdateProduct failed");
            return ServiceResult<ProductMutationResultDto>.Failure(new ErrorInfo { Code = "GRPC_ERROR", Message = ex.Status.Detail });
        }
    }

    public async Task<ServiceResult<ProductMutationResultDto>> DeleteProduct(string productId,CancellationToken cancellationToken=default)
    {
        try
        {
            var request = new DeleteProductRequest { ProductId = productId };
            var response = await _grpcClient.DeleteProductAsync(request, cancellationToken: cancellationToken);

            return response.ResultCase switch
            {
                DeleteProductResponse.ResultOneofCase.Data => ServiceResult<ProductMutationResultDto>.Success(new ProductMutationResultDto (response.Data.ProductId)),
                DeleteProductResponse.ResultOneofCase.Error => ServiceResult<ProductMutationResultDto>.Failure(MapToErrorInfo(response.Error)),
                _ => ServiceResult<ProductMutationResultDto>.Failure(new ErrorInfo { Code = "EMPTY_RESPONSE", Message = "Empty response from gRPC" })
            };
        }
        catch (RpcException ex)
        {
            _logger.LogError(ex, "gRPC DeleteProduct failed");
            return ServiceResult<ProductMutationResultDto>.Failure(new ErrorInfo { Code = "GRPC_ERROR", Message = ex.Status.Detail });
        }
    }

    private static ProductDto MapToDto(ProductData data)
    {
        return new ProductDto
        {
            ProductId = data.ProductId,
            Name = data.Name,
            Description = data.Description,
            Price = data.Price,
            Stock = data.Stock,
            Sku = data.Sku,
            CategoryId = data.CategoryId,
            IsActive = data.IsActive,
            CreatedAt = data.CreatedAt.ToDateTime(),
            UpdatedAt = data.UpdatedAt == null ? null : data.UpdatedAt.ToDateTime()
        };
    }

    private static ErrorInfo MapToErrorInfo(Shared.Contracts.ErrorResponse error)
    {
        return new ErrorInfo
        {
            Code = error.Code,
            Message = error.Message,
            Details = error.Details?.Count > 0 ? new Dictionary<string, string>(error.Details) : null
        };
    }

    private static PagedProductsDto MapToPagedDto(ProductListData data)
    {
        return new PagedProductsDto
        {
            Items = data.Items.Select(MapToDto).ToList(),
            PageNumber = data.PageNumber,
            PageSize = data.PageSize,
            TotalCount = data.TotalCount
        };
    }

    public async Task<ServiceResult<StockUpdateDto>> LockProductStock(string productId, int quantity,CancellationToken cancellationToken=default)
    {
        try
        {
            _logger.LogInformation("Calling gRPC service to lock stock for product {ProductId}, quantity: {Quantity}", 
                productId, quantity);

            var request = new UpdateStockRequest
            {
                ProductId = productId,
                Quantity = quantity
            };

            var response = await _grpcClient.LockProductStockAsync(request, cancellationToken: cancellationToken);

            return response.ResultCase switch
            {
                UpdateStockResponse.ResultOneofCase.Data => ServiceResult<StockUpdateDto>.Success(
                    new StockUpdateDto
                    {
                        ProductId = response.Data.ProductId,
                        UpdatedStock = response.Data.UpdatedStock,
                        LockedPrice = (decimal) response.Data.LockedPrice,
                        LockedQuantity = quantity
                    }),
                UpdateStockResponse.ResultOneofCase.Error => ServiceResult<StockUpdateDto>.Failure(
                    MapToErrorInfo(response.Error)),
                _ => ServiceResult<StockUpdateDto>.Failure(new ErrorInfo
                {
                    Code = "EMPTY_RESPONSE",
                    Message = "The gRPC service returned an empty response"
                })
            };
        }
        catch (RpcException ex)
        {
            _logger.LogError(ex, "gRPC call failed for LockProductStock - ProductId: {ProductId}, Quantity: {Quantity}", 
                productId, quantity);

            return ServiceResult<StockUpdateDto>.Failure(new ErrorInfo
            {
                Code = ex.StatusCode.ToString(),
                Message = $"gRPC call failed: {ex.Message}"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error calling LockProductStock - ProductId: {ProductId}", productId);

            return ServiceResult<StockUpdateDto>.Failure(new ErrorInfo
            {
                Code = "UNEXPECTED_ERROR",
                Message = $"An unexpected error occurred: {ex.Message}"
            });
        }
    }

    public async Task<ServiceResult<StockUpdateDto>> ReleaseProductStock(string productId, int quantity,CancellationToken cancellationToken=default)
    {
        try
        {
            _logger.LogInformation("Calling gRPC service to release stock for product {ProductId}, quantity: {Quantity}", 
                productId, quantity);

            var request = new UpdateStockRequest
            {
                ProductId = productId,
                Quantity = quantity
            };

            var response = await _grpcClient.ReleaseProductStockAsync(request, cancellationToken: cancellationToken);

            return response.ResultCase switch
            {
                UpdateStockResponse.ResultOneofCase.Data => ServiceResult<StockUpdateDto>.Success(
                    new StockUpdateDto
                    {
                        ProductId = response.Data.ProductId,
                        UpdatedStock = response.Data.UpdatedStock
                    }),
                UpdateStockResponse.ResultOneofCase.Error => ServiceResult<StockUpdateDto>.Failure(
                    MapToErrorInfo(response.Error)),
                _ => ServiceResult<StockUpdateDto>.Failure(new ErrorInfo
                {
                    Code = "EMPTY_RESPONSE",
                    Message = "The gRPC service returned an empty response"
                })
            };
        }
        catch (RpcException ex)
        {
            _logger.LogError(ex, "gRPC call failed for ReleaseProductStock - ProductId: {ProductId}, Quantity: {Quantity}", 
                productId, quantity);

            return ServiceResult<StockUpdateDto>.Failure(new ErrorInfo
            {
                Code = ex.StatusCode.ToString(),
                Message = $"gRPC call failed: {ex.Message}"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error calling ReleaseProductStock - ProductId: {ProductId}", productId);

            return ServiceResult<StockUpdateDto>.Failure(new ErrorInfo
            {
                Code = "UNEXPECTED_ERROR",
                Message = $"An unexpected error occurred: {ex.Message}"
            });
        }
    }

    public async Task<ServiceResult<PagedCategoriesDto>> GetCategories(GetCategoriesRequestDto requestDto, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Calling gRPC service to get categories - Page: {PageNumber}, Size: {PageSize}",
                requestDto.PageNumber, requestDto.PageSize);

            var request = new GetCategoriesRequest
            {
                PageNumber = requestDto.PageNumber,
                PageSize = requestDto.PageSize
            };

            if (requestDto.IsActive.HasValue)
                request.IsActive = requestDto.IsActive.Value;

            var response = await _grpcClient.GetCategoriesAsync(request, cancellationToken: cancellationToken);

            return response.ResultCase switch
            {
                GetCategoriesResponse.ResultOneofCase.Data => ServiceResult<PagedCategoriesDto>.Success(
                    MapToPagedCategoriesDto(response.Data)),
                GetCategoriesResponse.ResultOneofCase.Error => ServiceResult<PagedCategoriesDto>.Failure(
                    MapToErrorInfo(response.Error)),
                _ => ServiceResult<PagedCategoriesDto>.Failure(new ErrorInfo
                {
                    Code = "EMPTY_RESPONSE",
                    Message = "The gRPC service returned an empty response"
                })
            };
        }
        catch (RpcException ex)
        {
            _logger.LogError(ex, "gRPC call failed for GetCategories");

            return ServiceResult<PagedCategoriesDto>.Failure(new ErrorInfo
            {
                Code = "GRPC_ERROR",
                Message = $"Failed to communicate with the product service: {ex.Status.Detail}",
                Details = new Dictionary<string, string>
                {
                    ["StatusCode"] = ex.StatusCode.ToString(),
                    ["Detail"] = ex.Status.Detail ?? string.Empty
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while calling GetCategories");

            return ServiceResult<PagedCategoriesDto>.Failure(new ErrorInfo
            {
                Code = "UNEXPECTED_ERROR",
                Message = "An unexpected error occurred while retrieving categories",
                Details = new Dictionary<string, string>
                {
                    ["ExceptionType"] = ex.GetType().Name,
                    ["Message"] = ex.Message
                }
            });
        }
    }

    private static PagedCategoriesDto MapToPagedCategoriesDto(CategoryListData data)
    {
        return new PagedCategoriesDto
        {
            Items = data.Items.Select(MapToCategoryDto).ToList(),
            PageNumber = data.PageNumber,
            PageSize = data.PageSize,
            TotalCount = data.TotalCount,
            TotalPages = data.TotalPages
        };
    }

    private static CategoryDto MapToCategoryDto(CategoryData data)
    {
        return new CategoryDto
        {
            CategoryId = data.CategoryId,
            Name = data.Name,
            Description = data.Description,
            IsActive = data.IsActive,
            CreatedAt = data.CreatedAt.ToDateTime(),
            UpdatedAt = data.UpdatedAt == null ? null : data.UpdatedAt.ToDateTime()
        };
    }
}