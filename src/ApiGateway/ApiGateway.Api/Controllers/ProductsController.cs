using ApiGateway.Core.Common;
using ApiGateway.Core.Product;
using Microsoft.AspNetCore.Mvc;

namespace OrderSystem.ApiGateway.Controllers;

[ApiController]
[Route("api/products")]
public class ProductsController : ControllerBase
{
    private readonly IProductServiceClient _productServiceClient;
    private readonly ILogger<ProductsController> _logger;

    public ProductsController(
        IProductServiceClient productServiceClient,
        ILogger<ProductsController> logger)
    {
        _productServiceClient = productServiceClient;
        _logger = logger;
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetProduct(string id)
    {
        var result = await _productServiceClient.GetProductById(id);

        if (result.IsSuccess)
        {
            return Ok(result.Data);
        }

        // Converti gli errori del gRPC service in Problem Details
        return MapErrorToProblemDetails(result.Error!);
    }

    private IActionResult MapErrorToProblemDetails(ErrorInfo error)
    {
        var problemDetails = new ProblemDetails
        {
            Title = error.Code,
            Detail = error.Message,
            Extensions = { ["errorCode"] = error.Code }
        };

        if (error.Details != null && error.Details.Count > 0)
        {
            problemDetails.Extensions["additionalDetails"] = error.Details;
        }

        // Mappa i codici errore gRPC agli status code HTTP appropriati
        var (statusCode, type) = error.Code switch
        {
            "PRODUCT_NOT_FOUND" => (StatusCodes.Status404NotFound, "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.4"),
            "INVALID_PRODUCT_ID" => (StatusCodes.Status400BadRequest, "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.1"),
            "GRPC_ERROR" => (StatusCodes.Status503ServiceUnavailable, "https://datatracker.ietf.org/doc/html/rfc7231#section-6.6.4"),
            "EMPTY_RESPONSE" => (StatusCodes.Status502BadGateway, "https://datatracker.ietf.org/doc/html/rfc7231#section-6.6.3"),
            _ => (StatusCodes.Status500InternalServerError, "https://datatracker.ietf.org/doc/html/rfc7231#section-6.6.1")
        };

        problemDetails.Status = statusCode;
        problemDetails.Type = type;

        _logger.LogWarning("Returning error response: {Code} with HTTP status {StatusCode}", error.Code, statusCode);

        return StatusCode(statusCode, problemDetails);
    }
}