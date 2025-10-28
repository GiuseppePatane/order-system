using ApiGateway.Core.Common;
using ApiGateway.Core.Product;
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

    public async Task<ServiceResult<ProductDto>> GetProductById(string productId)
    {
        try
        {
            _logger.LogInformation("Calling gRPC service to get product with ID: {ProductId}", productId);

            var request = new GetProductRequest
            {
                ProductId = productId
            };

            var response = await _grpcClient.GetProductAsync(request);

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

    private static ProductDto MapToDto(ProductData data)
    {
        return new ProductDto
        {
            ProductId = data.ProductId,
            Name = data.Name,
            Description = data.Description,
            Price = data.Price
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
}