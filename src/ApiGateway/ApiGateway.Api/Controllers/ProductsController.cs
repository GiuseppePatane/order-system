using ApiGateway.Core.Common;
using ApiGateway.Core.Product;
using Microsoft.AspNetCore.Mvc;

namespace ApiGateway.Api.Controllers;

/// <summary>
/// Manages product operations through the Product gRPC service
/// </summary>
[ApiController]
[Route("api/products")]
[Produces("application/json")]
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

    /// <summary>
    /// Retrieves a single product by its ID
    /// </summary>
    /// <param name="id">The product ID (GUID format)</param>
    /// <returns>The product details</returns>
    /// <response code="200">Returns the product details</response>
    /// <response code="404">Product not found</response>
    /// <response code="400">Invalid product ID format</response>
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


    /// <summary>
    /// Creates a new product
    /// </summary>
    /// <param name="request">The product creation data</param>
    /// <returns>The created product</returns>
    /// <response code="201">Product created successfully</response>
    /// <response code="400">Invalid product data</response>
    [HttpPost]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateProduct([FromBody] CreateProductRequestDto request)
    {
        var result = await _productServiceClient.CreateProduct(request);
        if (result.IsSuccess && result.Data != null)
        {
            return CreatedAtAction(nameof(GetProduct), new { id = result.Data.ProductId }, result.Data);
        }

        return MapErrorToProblemDetails(result.Error!);
    }


    /// <summary>
    /// Retrieves a paginated list of products
    /// </summary>
    /// <param name="pageNumber">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 10, max: 100)</param>
    /// <param name="categoryId">Optional category filter (GUID)</param>
    /// <param name="searchTerm">Optional search term for product name/description</param>
    /// <param name="isActive">Optional filter for active products</param>
    /// <returns>A paginated list of products</returns>
    /// <response code="200">Returns the paginated product list</response>
    [HttpGet("{pageNumber}/{pageSize}")]
    [ProducesResponseType(typeof(PagedProductsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetProducts([FromRoute] int pageNumber = 1, [FromRoute] int pageSize = 10, [FromQuery] string? categoryId = null, [FromQuery] string? searchTerm = null, [FromQuery] bool? isActive = null)
    {
        var request = new GetProductsRequestDto
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            CategoryId = categoryId,
            IsActive = isActive,
            SearchTerm = searchTerm
        };

        var result = await _productServiceClient.GetProducts(request);
        if (result.IsSuccess && result.Data != null)
            return Ok(result.Data);

        return MapErrorToProblemDetails(result.Error!);
    }


    /// <summary>
    /// Updates an existing product
    /// </summary>
    /// <param name="id">The product ID to update</param>
    /// <param name="request">The product update data (only provide fields to update)</param>
    /// <returns>The updated product</returns>
    /// <response code="200">Product updated successfully</response>
    /// <response code="404">Product not found</response>
    /// <response code="400">Invalid product data</response>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateProduct(string id, [FromBody] UpdateProductRequestDto request)
    {
        request = request with { ProductId = id };
        var result = await _productServiceClient.UpdateProduct(request);
        if (result.IsSuccess && result.Data != null)
            return Ok(result.Data);

        return MapErrorToProblemDetails(result.Error!);
    }

    /// <summary>
    /// Deletes a product
    /// </summary>
    /// <param name="id">The product ID to delete</param>
    /// <returns>No content on success</returns>
    /// <response code="204">Product deleted successfully</response>
    /// <response code="404">Product not found</response>
    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(void), StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteProduct(string id)
    {
        var result = await _productServiceClient.DeleteProduct(id);
        if (result.IsSuccess && result.Data != null && result.Data.Success)
            return NoContent();

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

        var (statusCode, type) = error.Code switch
        {
            "PRODUCT_NOT_FOUND" => (StatusCodes.Status404NotFound, "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.4"),
            "INVALID_PRODUCT_ID" => (StatusCodes.Status400BadRequest, "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.1"),
            "GRPC_ERROR" => (StatusCodes.Status503ServiceUnavailable, "https://datatracker.ietf.org/doc/html/rfc7231#section-6.6.4"),
            "INVALID_ARGUMENT" => (StatusCodes.Status400BadRequest, "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.1"),
            "EMPTY_RESPONSE" => (StatusCodes.Status502BadGateway, "https://datatracker.ietf.org/doc/html/rfc7231#section-6.6.3"),
            _ => (StatusCodes.Status500InternalServerError, "https://datatracker.ietf.org/doc/html/rfc7231#section-6.6.1")
        };

        problemDetails.Status = statusCode;
        problemDetails.Type = type;

        _logger.LogWarning("Returning error response: {Code} with HTTP status {StatusCode}", error.Code, statusCode);

        return StatusCode(statusCode, problemDetails);
    }
}
